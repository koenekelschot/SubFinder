using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RadarrSharp;
using RadarrSharp.Models;
using SubFinder.Config;
using SubFinder.Models;

namespace SubFinder.Scanners.Implementations
{
    public class RadarrScanner : IMediaScanner
    {
        private readonly RadarrClient _client;

        public RadarrScanner(IOptions<RadarrConfig> config)
        {
            var clientConfig = config.Value;
            _client = new RadarrClient(clientConfig.Host, clientConfig.Port, clientConfig.ApiKey);
        }

        public string ScannerName => nameof(RadarrScanner);

        public async Task<IList<Media>> GetDownloadedItemsAsync()
        {
            var media = new List<Media>();
            var movies = await _client.Movie.GetMovies();

            foreach (var movie in movies.Where(movie => movie.Downloaded && movie.HasFile))
            {
                media.Add(ConvertMovie(movie));
            }

            return media;
        }

        private RadarrMovie ConvertMovie(Movie movie)
        {
            var converted = new RadarrMovie
            {
                Title = movie.Title,
                ImdbId = movie.ImdbId,
                TmdbId = movie.TmdbId,
                Path = movie.Path
            };

            var fileInfo = movie.MovieFile;
            if (fileInfo != null)
            {
                converted.OriginalName = fileInfo.SceneName;
                converted.ReleaseGroup = fileInfo.ReleaseGroup;
                converted.Edition = fileInfo.Edition;

                var quality = fileInfo.Quality;
                if (quality != null)
                {
                    converted.Quality = quality.Quality.Name;
                }
            }

            return converted;
        }
    }
}
