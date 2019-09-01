using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SubFinder.Config;
using SubFinder.Extensions;
using SubFinder.HttpClients;
using SubFinder.Languages;
using SubFinder.Models;

namespace SubFinder.Providers.Implementations
{
    public class OpenSubtitlesSubtitleProvider : ISubtitleProvider
    {
        public string ProviderName => nameof(OpenSubtitlesSubtitleProvider);

        private string MovieSearchUri(string imdbId) =>
            $"en/search/sublanguageid-{_languages}/searchonlymovies-on/hd-on/imdbid-{imdbId}/sort-7/asc-0/xml";
        private string SeriesSearchUri(string imdbId, int season, int episode) => 
            $"en/search/sublanguageid-{_languages}/searchonlytvseries-on/season-{season}/episode-{episode}/hd-on/imdbid-{imdbId}/sort-7/asc-0/xml";
        private static string DownloadUri(string subtitleId) =>
            $"en/download/vrf-108d030f/sub/{subtitleId}";

        private readonly ILogger<OpenSubtitlesSubtitleProvider> _logger;
        private readonly OpenSubtitlesHttpClient _httpClient;
        private readonly string _languages;

        public OpenSubtitlesSubtitleProvider(
            ILogger<OpenSubtitlesSubtitleProvider> logger,
            OpenSubtitlesHttpClient httpClient,
            IOptions<SubtitleConfig> config)
        {
            _logger = logger;
            _httpClient = httpClient;

            var isoLanguages = config.Value.PreferredLanguages.Select(lang => Language.GetIsoPart2Bibliographic(lang));
            _languages = string.Join(',', isoLanguages);
        }

        public async Task<IEnumerable<Subtitle>> SearchForEpisodeAsync(Episode episode)
        {
            var requestUrl = SeriesSearchUri(episode.ImdbId, episode.SeasonNumber, episode.EpisodeNumber);
            return await DoSearchRequestAsync(requestUrl);
        }

        public async Task<IEnumerable<Subtitle>> SearchForMovieAsync(Movie movie)
        {
            var requestUrl = MovieSearchUri(movie.ImdbId);
            return await DoSearchRequestAsync(requestUrl);
        }

        public async Task<Memory<byte>> DownloadAsync(Subtitle subtitle)
        {
            if (subtitle.Provider != ProviderName)
            {
                throw new NotSupportedException("Wrong provider");
            }

            var requestUrl = DownloadUri(subtitle.Id);
            return await DoDownloadRequestAsync(requestUrl);
        }

        private async Task<IEnumerable<Subtitle>> DoSearchRequestAsync(string requestUrl)
        {
            try
            {
                var document = new HtmlDocument();

                using (var responseStream = await _httpClient.GetStreamAsync(requestUrl))
                {
                    document.Load(responseStream);
                    responseStream.Close();
                }

                var subtitles = ParseSubtitles(document);
                document = null;
                return subtitles;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error parsing search results {ProviderName}");
                return Array.Empty<Subtitle>();
            }
        }

        private IEnumerable<Subtitle> ParseSubtitles(HtmlDocument document)
        {
            return document.DocumentNode
                .Descendants("subtitle")
                .Where(node => !node.ChildNodes
                    .Any(cn => cn.Name.StartsWith("ads")))
                .Select(node => ParseSubtitleNode(node));
        }

        private Subtitle ParseSubtitleNode(HtmlNode node)
        {
            return new Subtitle
            {
                Provider = ProviderName,
                Id = node.ChildNodes["idsubtitle"].InnerText,
                ReleaseName = node.ChildNodes["moviereleasename"].CharacterData(),
                Language = GetSubtitleLanguage(node),
                Added = GetSubtitleDateAdded(node),
                VotesBad = int.Parse(node.ChildNodes["subbad"].InnerText),
                Rating = decimal.Parse(node.ChildNodes["subrating"].InnerText),
                Downloads = int.Parse(node.ChildNodes["subdownloadscnt"].InnerText)
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

        private async Task<Memory<byte>> DoDownloadRequestAsync(string requestUrl)
        {
            try
            {
                using (var responseStream = await _httpClient.GetStreamAsync(requestUrl))
                {
                    return await responseStream.ExtractZippedSubtitleAsync();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error downloading subtitle {ProviderName}");
            }

            return new Memory<byte>();
        }
    }
}
