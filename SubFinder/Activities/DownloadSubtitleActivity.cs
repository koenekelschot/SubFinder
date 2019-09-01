using SubFinder.Models;
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

        public DownloadSubtitleActivity(
            SearchEpisodeSubtitlesActivity searchEpisodeSubtitlesActivity,
            SearchMovieSubtitlesActivity searchMovieSubtitlesActivity)
        {
            _searchEpisodeSubtitlesActivity = searchEpisodeSubtitlesActivity;
            _searchMovieSubtitlesActivity = searchMovieSubtitlesActivity;
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
                .Select(lang => lang.First())
                .ToList();
        }
    }
}
