using Microsoft.Extensions.Logging;
using SubFinder.Models;
using SubFinder.Providers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SubFinder.Activities
{
    public class SearchMovieSubtitlesActivity
    {
        private readonly ILogger<SearchMovieSubtitlesActivity> _logger;
        private readonly IEnumerable<ISubtitleProvider> _subtitleProviders;

        public SearchMovieSubtitlesActivity(
            ILogger<SearchMovieSubtitlesActivity> logger,
            IEnumerable<ISubtitleProvider> subtitleProviders)
        {
            _logger = logger;
            _subtitleProviders = subtitleProviders;
        }

        public async Task ExecuteAsync(Movie movie)
        {
            _logger.LogInformation($"Searching subtitle for movie {movie.Title}");

            var searchTasks = new List<Task>(_subtitleProviders.Count());

            foreach (var provider in _subtitleProviders)
            {
                searchTasks.Add(provider.SearchForMovieAsync(movie));
            }

            await Task.WhenAll(searchTasks);
        }
    }
}
