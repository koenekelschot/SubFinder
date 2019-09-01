using SubFinder.Models;
using SubFinder.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SubFinder.Activities
{
    public class DownloadSubtitleActivity
    {
        private readonly SearchEpisodeSubtitlesActivity _searchEpisodeSubtitlesActivity;
        private readonly SearchMovieSubtitlesActivity _searchMovieSubtitlesActivity;
        private readonly IEnumerable<ISubtitleProvider> _subtitleProviders;

        public DownloadSubtitleActivity(
            SearchEpisodeSubtitlesActivity searchEpisodeSubtitlesActivity,
            SearchMovieSubtitlesActivity searchMovieSubtitlesActivity,
            IEnumerable<ISubtitleProvider> subtitleProviders)
        {
            _searchEpisodeSubtitlesActivity = searchEpisodeSubtitlesActivity;
            _searchMovieSubtitlesActivity = searchMovieSubtitlesActivity;
            _subtitleProviders = subtitleProviders;
        }

        public async Task ExecuteAsync(Media media)
        {
            IList<Subtitle> results;

            switch (media)
            {
                case Episode episode:
                    results = await _searchEpisodeSubtitlesActivity.ExecuteAsync(episode);
                    break;
                case Movie movie:
                    results = await _searchMovieSubtitlesActivity.ExecuteAsync(movie);
                    break;
                default:
                    throw new NotSupportedException();
            }

            var subtitles = results
                .OrderByDescending(sub => sub.Downloads)
                .ThenByDescending(sub => sub.Rating)
                .ThenBy(sub => sub.VotesBad)
                .GroupBy(sub => sub.Language)
                .Select(lang => lang.First());

            foreach (var subtitle in subtitles)
            {
                var subtitleProvider = _subtitleProviders.First(provider => provider.ProviderName == subtitle.Provider);
                await subtitleProvider.DownloadAsync(subtitle);
            }
        }
    }
}
