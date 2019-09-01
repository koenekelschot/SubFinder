using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SubFinder.Config;
using SubFinder.Extensions;
using SubFinder.Languages;
using SubFinder.Models;

namespace SubFinder.Providers.Implementations
{
    public class OpenSubtitlesSubtitleProvider : ISubtitleProvider
    {
        public string ProviderName => nameof(OpenSubtitlesSubtitleProvider);

        private string UrlMovieSearch(string imdbId) =>
            $"https://www.opensubtitles.org/en/search/sublanguageid-{_languages}/searchonlymovies-on/hd-on/imdbid-{imdbId}/sort-7/asc-0/xml";
        private string UrlSeriesSearch(string imdbId, int season, int episode) => 
            $"https://www.opensubtitles.org/en/search/sublanguageid-{_languages}/searchonlytvseries-on/season-{season}/episode-{episode}/hd-on/imdbid-{imdbId}/sort-7/asc-0";
        private static string UrlDownload(string subtitleId) => 
            $"https://dl.opensubtitles.org/en/download/sub/{subtitleId}";

        private readonly ILogger<OpenSubtitlesSubtitleProvider> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _languages;

        public OpenSubtitlesSubtitleProvider(
            ILogger<OpenSubtitlesSubtitleProvider> logger,
            IHttpClientFactory httpClientFactory,
            IOptions<SubtitleConfig> config)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;

            var isoLanguages = config.Value.PreferredLanguages.Select(lang => Language.GetIsoPart2Bibliographic(lang));
            _languages = string.Join(',', isoLanguages);
        }

        public async Task<IList<Subtitle>> SearchForEpisodeAsync(Episode episode)
        {
            var requestUrl = UrlSeriesSearch(episode.ImdbId, episode.SeasonNumber, episode.EpisodeNumber);
            return await DoSearchRequestAsync(requestUrl);
        }

        public async Task<IList<Subtitle>> SearchForMovieAsync(Movie movie)
        {
            var requestUrl = UrlMovieSearch(movie.ImdbId);
            return await DoSearchRequestAsync(requestUrl);
        }

        private async Task<IList<Subtitle>> DoSearchRequestAsync(string requestUrl)
        {
            try
            {
                var response = await _httpClientFactory.CreateClient(ProviderName).GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(await response.Content.ReadAsStringAsync());

                return ParseSubtitles(document);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error parsing search results {ProviderName}");
                return Array.Empty<Subtitle>();
            }
        }

        private IList<Subtitle> ParseSubtitles(HtmlDocument document)
        {
            var nodes = document.DocumentNode
                    .Descendants("subtitle")
                    .Where(node => !node.ChildNodes
                        .Any(cn => cn.Name.StartsWith("ads")));

            var subtitles = new List<Subtitle>(nodes.Count());

            foreach (var node in nodes)
            {
                subtitles.Add(ParseSubtitleNode(node));
            }

            return subtitles;
        }

        private Subtitle ParseSubtitleNode(HtmlNode node)
        {
            return new Subtitle
            {
                Provider = ProviderName,
                Id = node.ChildNodes["idsubtitlefile"].InnerText,
                ReleaseName = node.ChildNodes["moviereleasename"].CharacterData(),
                Language = GetSubtitleLanguage(node),
                Added = GetSubtitleDateAdded(node),
                VotesBad = int.Parse(node.ChildNodes["subbad"].InnerText),
                Rating = decimal.Parse(node.ChildNodes["subrating"].InnerText),
                Downloads = int.Parse(node.ChildNodes["subdownloadscnt"].InnerText),
                Season = node.GetOptionalChildNodeValue("seriesseason"),
                Episode = node.GetOptionalChildNodeValue("seriesepisode")
            };
        }

        private Language.IsoLanguage GetSubtitleLanguage(HtmlNode node)
        {
            var languageString = node.ChildNodes["languagename"].InnerText;
            return Language.GetLanguageFromString(languageString);
        }

        private DateTime GetSubtitleDateAdded(HtmlNode node)
        {
            var date = node.ChildNodes["subadddate"].GetAttributeValue("rfc3339", string.Empty);
            return DateTime.Parse(date);
        }
    }
}
