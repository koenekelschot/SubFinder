using Microsoft.Extensions.Logging;
using SubFinder.Models;
using System.Threading.Tasks;

namespace SubFinder.Activities
{
    public class SearchMovieSubtitlesActivity
    {
        private readonly ILogger<SearchMovieSubtitlesActivity> _logger;

        public SearchMovieSubtitlesActivity(ILogger<SearchMovieSubtitlesActivity> logger)
        {
            _logger = logger;
        }

        public async Task ExecuteAsync(Movie movie)
        {
            _logger.LogInformation($"Searching subtitle for movie {movie.Title}");
        }
    }
}
