using SubFinder.Models;
using System;
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
            Task task;

            switch (media)
            {
                case Episode episode:
                    task = _searchEpisodeSubtitlesActivity.ExecuteAsync(episode);
                    break;
                case Movie movie:
                    task = _searchMovieSubtitlesActivity.ExecuteAsync(movie);
                    break;
                default:
                    throw new NotSupportedException();
            }

            await task;
        }
    }
}
