using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
            $"https://www.opensubtitles.org/en/search/sublanguageid-{_languages}/searchonlytvseries-on/season-{season}/episode-{episode}/hd-on/imdbid-{imdbId}/sort-7/asc-0/xml";
        private static string UrlDownload(string subtitleId) =>
            $"http://dl.opensubtitles.org/en/download/vrf-108d030f/sub/{subtitleId}";

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

        public async Task<Memory<byte>> DownloadAsync(Subtitle subtitle)
        {
            if (subtitle.Provider != ProviderName)
            {
                throw new NotSupportedException("Wrong provider");
            }

            var requestUrl = UrlDownload(subtitle.Id);
            return await DoDownloadRequestAsync(requestUrl);
        }

        private async Task<IList<Subtitle>> DoSearchRequestAsync(string requestUrl)
        {
            try
            {
                var client = _httpClientFactory.CreateClient(ProviderName);
                var document = new HtmlDocument();

                using (var responseStream = await client.GetStreamAsync(requestUrl))
                {
                    document.Load(responseStream);
                    responseStream.Close();
                }

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
                Id = node.ChildNodes["idsubtitle"].InnerText,
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

        private async Task<Memory<byte>> DoDownloadRequestAsync(string requestUrl)
        {
            try
            {
                var client = _httpClientFactory.CreateClient(ProviderName);

                using (var responseStream = await client.GetStreamAsync(requestUrl))
                {
                    return await ExtractSubtitleFromResponseAsync(responseStream);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error downloading subtitle {ProviderName}");
            }

            return new Memory<byte>();
        }

        private async Task<Memory<byte>> ExtractSubtitleFromResponseAsync(Stream responseStream)
        {
            using (var archive = new ZipArchive(responseStream))
            {
                var subtitleEntry = archive.Entries.FirstOrDefault(entry => entry.Name.EndsWith(".srt"));
                var memory = new Memory<byte>(new byte[subtitleEntry.Length]);

                using (var stream = subtitleEntry.Open())
                {
                    await stream.ReadAsync(memory);
                }

                return memory;
            }
        }
    }
}
