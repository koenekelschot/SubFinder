using Microsoft.Extensions.Logging;
using SubFinder.Languages;
using SubFinder.Models;
using SubFinder.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SubFinder.Activities
{
    public class DownloadSubtitleActivity
    {
        private readonly ILogger<DownloadSubtitleActivity> _logger;
        private readonly SearchEpisodeSubtitlesActivity _searchEpisodeSubtitlesActivity;
        private readonly SearchMovieSubtitlesActivity _searchMovieSubtitlesActivity;
        private readonly IEnumerable<ISubtitleProvider> _subtitleProviders;

        public DownloadSubtitleActivity(
            ILogger<DownloadSubtitleActivity> logger,
            SearchEpisodeSubtitlesActivity searchEpisodeSubtitlesActivity,
            SearchMovieSubtitlesActivity searchMovieSubtitlesActivity,
            IEnumerable<ISubtitleProvider> subtitleProviders)
        {
            _logger = logger;
            _searchEpisodeSubtitlesActivity = searchEpisodeSubtitlesActivity;
            _searchMovieSubtitlesActivity = searchMovieSubtitlesActivity;
            _subtitleProviders = subtitleProviders;
        }

        public async Task ExecuteAsync(Media media)
        {
            IList<Subtitle> foundSubtitles;

            switch (media)
            {
                case Episode episode:
                    foundSubtitles = await _searchEpisodeSubtitlesActivity.ExecuteAsync(episode);
                    break;
                case Movie movie:
                    foundSubtitles = await _searchMovieSubtitlesActivity.ExecuteAsync(movie);
                    break;
                default:
                    throw new NotSupportedException();
            };

            var preferredSubtitles = GetPreferredSubtitles(foundSubtitles);
            var downloadedSubtitles = await DownloadSubtitlesAsync(preferredSubtitles);

            foreach (var (language, data) in downloadedSubtitles.Where(dl => !dl.data.IsEmpty))
            {
                await SaveSubtitleToFileAsync(media.SubtitlePath(language), data);
            }
        }

        private IEnumerable<Subtitle> GetPreferredSubtitles(IList<Subtitle> subtitles)
        {
            return subtitles
                .OrderByDescending(sub => sub.Downloads)
                .ThenByDescending(sub => sub.Rating)
                .ThenBy(sub => sub.VotesBad)
                .GroupBy(sub => sub.Language)
                .Select(lang => lang.First());
        }

        private async Task<(Language.IsoLanguage language, Memory<byte> data)[]> DownloadSubtitlesAsync(IEnumerable<Subtitle> subtitles)
        {
            var count = subtitles.Count();
            var result = new (Language.IsoLanguage language, Memory<byte> data)[count];

            for (var i = 0; i < count; i++)
            {
                var subtitle = subtitles.ElementAt(i);
                var subtitleProvider = _subtitleProviders.First(provider => provider.ProviderName == subtitle.Provider);
                result[i] = (subtitle.Language, await subtitleProvider.DownloadAsync(subtitle));
            }

            return result;
        }

        private async Task SaveSubtitleToFileAsync(string filePath, Memory<byte> data)
        {
            _logger.LogInformation($"Saving subtitle to {filePath}");
            using (var filestream = File.Create(filePath))
            {
                await filestream.WriteAsync(data);
            }
        }
    }
}
