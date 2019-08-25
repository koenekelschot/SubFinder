using Microsoft.Extensions.Logging;
using SubFinder.Models;
using System.Threading.Tasks;

namespace SubFinder.Activities
{
    public class SearchEpisodeSubtitlesActivity
    {
        private readonly ILogger<SearchEpisodeSubtitlesActivity> _logger;

        public SearchEpisodeSubtitlesActivity(ILogger<SearchEpisodeSubtitlesActivity> logger)
        {
            _logger = logger;
        }

        public async Task ExecuteAsync(Episode episode)
        {
            _logger.LogInformation($"Searching subtitle for episode {episode.Title} {episode.EpisodeQualifier}");
        }
    }
}
