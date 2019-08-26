using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SubFinder.Config;
using SubFinder.Languages;
using SubFinder.Models;

namespace SubFinder.Providers.Implementations
{
    public class OpenSubtitlesSubtitleProvider : ISubtitleProvider
    {
        public string ProviderName => nameof(OpenSubtitlesSubtitleProvider);

        private static string UrlMovieSearch(string language, string imdbId) => 
            $"https://www.opensubtitles.org/en/search/sublanguageid-{language}/searchonlymovies-on/hd-on/imdbid-{imdbId}/sort-7/asc-0";
        private static string UrlSeriesSearch(string language, string imdbId, int season, int episode) => 
            $"https://www.opensubtitles.org/en/search/sublanguageid-{language}/searchonlytvseries-on/season-{season}/episode-{episode}/hd-on/imdbid-{imdbId}/sort-7/asc-0";
        private static string UrlDownload(string subtitleId) => 
            $"https://dl.opensubtitles.org/en/download/sub/{subtitleId}";

        private readonly ILogger<OpenSubtitlesSubtitleProvider> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IList<string> _languages = new List<string>();

        public OpenSubtitlesSubtitleProvider(
            ILogger<OpenSubtitlesSubtitleProvider> logger,
            IHttpClientFactory httpClientFactory,
            IOptions<SubtitleConfig> config)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;

            foreach (var preferredLanguage in config.Value.PreferredLanguages)
            {
                _languages.Add(Language.GetIsoPart2Bibliographic(preferredLanguage));
            }
        }

        public async Task<IList<Subtitle>> SearchForEpisodeAsync(Episode episode)
        {
            var requestUrls = new string[_languages.Count];

            for (var i = 0; i < _languages.Count; i++)
            {
                requestUrls[i] = UrlSeriesSearch(_languages[i], episode.ImdbId, episode.SeasonNumber, episode.EpisodeNumber);
            }

            return await SearchSubtitlesAsync(requestUrls);
        }

        public async Task<IList<Subtitle>> SearchForMovieAsync(Movie movie)
        {
            var requestUrls = new string[_languages.Count];

            for (var i = 0; i < _languages.Count; i++)
            {
                requestUrls[i] = UrlMovieSearch(_languages[i], movie.ImdbId);
            }

            return await SearchSubtitlesAsync(requestUrls);
        }

        private async Task<IList<Subtitle>> SearchSubtitlesAsync(string[] requestUrls)
        {
            var results = new List<Subtitle>();
            var searchTasks = new List<Task<IList<Subtitle>>>(requestUrls.Length);

            foreach (var requestUrl in requestUrls)
            {
                searchTasks.Add(DoSearchRequestAsync(requestUrl));
            }

            await Task.WhenAll(searchTasks);

            foreach (var searchResult in searchTasks)
            {
                results.AddRange(await searchResult);
            }

            return results;
        }

        private async Task<IList<Subtitle>> DoSearchRequestAsync(string requestUrl)
        {
            try
            {
                var response = await _httpClientFactory.CreateClient(ProviderName).GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Oops");
            }

            return Array.Empty<Subtitle>();
        }
    }
}
