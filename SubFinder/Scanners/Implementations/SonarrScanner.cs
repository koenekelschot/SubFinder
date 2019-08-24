using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SonarrSharp;
using SonarrSharp.Models;
using SubFinder.Config;
using SubFinder.Models;

namespace SubFinder.Scanners.Implementations
{
    public class SonarrScanner : IMediaScanner
    {
        private readonly SonarrClient _client;

        public SonarrScanner(
            IOptions<SonarrConfig> config)
        {
            var clientConfig = config.Value;
            _client = new SonarrClient(clientConfig.Host, clientConfig.Port, clientConfig.ApiKey);
        }

        public string ScannerName => nameof(SonarrScanner);

        public async Task<IList<Media>> GetDownloadedItemsAsync()
        {
            var media = new List<Media>();
            var series = await _client.Series.GetSeries();
            var tasks = new List<Task<IList<Media>>>();

            foreach (var serie in series)
            {
                tasks.Add(GetSeriesInformationAsync(serie));
            }

            await Task.WhenAll(tasks);

            foreach (var result in tasks)
            {
                media.AddRange(await result);
            }

            return media;
        }

        private async Task<IList<Media>> GetSeriesInformationAsync(Series serie)
        {
            var media = new List<Media>();

            var episodesTask = _client.Episode.GetEpisodes(serie.Id);
            var episodeFilesTask = _client.EpisodeFile.GetEpisodeFiles(serie.Id);

            await Task.WhenAll(episodesTask, episodeFilesTask);

            var episodes = await episodesTask;
            var episodeFiles = await episodeFilesTask;

            foreach (var episode in episodes.Where(episode => episode.HasFile))
            {
                var episodeFile = episodeFiles.FirstOrDefault(ef => ef.Id == episode.EpisodeFileId);
                media.Add(ConvertEpisode(serie, episode, episodeFile));
            }

            return media;
        }

        private SonarrEpisode ConvertEpisode(Series serie, Episode episode, EpisodeFile file)
        {
            var converted = new SonarrEpisode
            {
                Title = serie.Title,
                ImdbId = serie.ImdbId,
                Path = serie.Path,
                SeasonNumber = episode.SeasonNumber,
                EpisodeNumber = episode.EpisodeNumber
            };

            if (file != null)
            {
                converted.Path = file.Path;
                converted.OriginalName = file.SceneName;
                converted.Quality = file.Quality.Quality.Name.ToString();
                converted.Proper = file.Quality.Proper;
            }

            return converted;
        }
    }
}
