using Microsoft.Extensions.Logging;
using SubFinder.Models;
using SubFinder.Providers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SubFinder.Activities
{
    public class SearchEpisodeSubtitlesActivity
    {
        private readonly ILogger<SearchEpisodeSubtitlesActivity> _logger;
        private readonly IEnumerable<ISubtitleProvider> _subtitleProviders;

        public SearchEpisodeSubtitlesActivity(
            ILogger<SearchEpisodeSubtitlesActivity> logger,
            IEnumerable<ISubtitleProvider> subtitleProviders)
        {
            _logger = logger;
            _subtitleProviders = subtitleProviders;
        }

        public async Task<IList<Subtitle>> ExecuteAsync(Episode episode)
        {
            _logger.LogInformation($"Searching subtitle for episode {episode.Title} {episode.EpisodeQualifier}");

            var searchTasks = new List<Task<IList<Subtitle>>>(_subtitleProviders.Count());

            foreach (var provider in _subtitleProviders)
            {
                searchTasks.Add(provider.SearchForEpisodeAsync(episode));
            }

            await Task.WhenAll(searchTasks);

            var subtitles = new List<Subtitle>();
            foreach (var result in searchTasks)
            {
                subtitles.AddRange(await result);
            }

            return subtitles;
        }
    }
}
