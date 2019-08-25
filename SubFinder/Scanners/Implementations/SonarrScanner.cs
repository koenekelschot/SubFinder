using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SonarrSharp;
using SubFinder.Config;
using SubFinder.Models;
using SonarrEpisode = SonarrSharp.Models.Episode;
using SonarrEpisodeFile = SonarrSharp.Models.EpisodeFile;
using SonarrSeries = SonarrSharp.Models.Series;

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

        private async Task<IList<Media>> GetSeriesInformationAsync(SonarrSeries serie)
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

        private Episode ConvertEpisode(SonarrSeries serie, SonarrEpisode episode, SonarrEpisodeFile file)
        {
            var converted = new Episode
            {
                Title = serie.Title,
                ImdbId = serie.ImdbId,
                SeriesFolder = serie.Path,
                SeasonNumber = episode.SeasonNumber,
                EpisodeNumber = episode.EpisodeNumber
            };

            if (file != null)
            {
                converted.File = Path.GetFileName(file.Path);
                converted.OriginalName = file.SceneName;
                converted.Quality = file.Quality.Quality.Name.ToString();
                converted.Proper = file.Quality.Proper;
            }

            return converted;
        }
    }
}
