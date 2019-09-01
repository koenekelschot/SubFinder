using System;
using System.Collections.Generic;
using System.Globalization;
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
    public class PodnapisiSubtitleProvider : ISubtitleProvider
    {
        public string ProviderName => nameof(PodnapisiSubtitleProvider);

        private string MovieSearchUri(string movieName, int movieYear) =>
            SearchUri(movieName, movieYear, "movie");
        private string SeriesSearchUri(string seriesName, int seriesYear, int season, int episode) =>
            SearchUri(seriesName, seriesYear, "tv-series") + $"&seasons={season}&episodes={episode}";
        private static string DownloadUri(string subtitleId) =>
            $"en/subtitles/{subtitleId}/download";

        private string SearchUri(string name, int year, string type)
        {
            var url = $"subtitles/search/advanced?keywords={Uri.EscapeDataString(name)}&year={year}&movie_type={type}";
            foreach (var language in _languages)
            {
                url += $"&language={language}";
            }
            return url;
        }

        private readonly ILogger<PodnapisiSubtitleProvider> _logger;
        private readonly PodnapisiHttpClient _httpClient;
        private readonly IEnumerable<string> _languages;
        private readonly CultureInfo _dateCulture = new CultureInfo("en-US", false);

        public PodnapisiSubtitleProvider(
            ILogger<PodnapisiSubtitleProvider> logger,
            PodnapisiHttpClient httpClient,
            IOptions<SubtitleConfig> config)
        {
            _logger = logger;
            _httpClient = httpClient;

            _languages = config.Value.PreferredLanguages.Select(lang => Language.GetIsoPart1(lang));
        }

        public async Task<IEnumerable<Subtitle>> SearchForEpisodeAsync(Episode episode)
        {
            var requestUrl = SeriesSearchUri(episode.Title, episode.Year, episode.SeasonNumber, episode.EpisodeNumber);
            return await DoSearchRequestAsync(requestUrl);
        }

        public async Task<IEnumerable<Subtitle>> SearchForMovieAsync(Movie movie)
        {
            var requestUrl = MovieSearchUri(movie.Title, movie.Year);
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
                .Descendants()
                .Where(node => node.GetAttributeValue("class", string.Empty) == "subtitle-entry")
                .Select(node => ParseSubtitleNode(node));
        }

        private Subtitle ParseSubtitleNode(HtmlNode node)
        {
            return new Subtitle
            {
                Provider = ProviderName,
                Id = GetSubtitleId(node),
                ReleaseName = GetReleaseName(node),
                Language = GetSubtitleLanguage(node),
                Added = GetSubtitleDateAdded(node),
                VotesBad = 0,
                Rating = GetSubtitleRating(node),
                Downloads = GetSubtitleDownloads(node),
            };
        }

        private string GetSubtitleId(HtmlNode node)
        {
            var href = node.GetAttributeValue("data-href", string.Empty);
            return href.Split('/').Last();
        }

        private string GetReleaseName(HtmlNode node)
        {
            return node.Descendants("span")
                .First(desc => desc.GetAttributeValue("class", string.Empty) == "release")
                .InnerText;
        }

        private Language.IsoLanguage GetSubtitleLanguage(HtmlNode node)
        {
            var languageValue = node.Descendants("abbr")
                .First(desc => desc.GetAttributeValue("class", string.Empty).Contains("language-"))
                .GetAttributeValue("data-title", string.Empty);
            return Language.GetLanguageFromString(languageValue);
        }

        private DateTime GetSubtitleDateAdded(HtmlNode node)
        {
            var dateText = node.Descendants("td")
                .Last()
                .InnerText
                .Trim();
            return DateTime.Parse(dateText, _dateCulture);
        }

        private decimal GetSubtitleRating(HtmlNode node)
        {
            var rating = node.Descendants("div")
                .First(desc => desc.GetAttributeValue("class", string.Empty).Contains("rating"))
                .GetAttributeValue("data-title", "0.0% (0)");
            var percentage = rating.Split('%').First();
            var dec = decimal.Parse(percentage);
            return dec * 0.1M;
        }

        private int GetSubtitleDownloads(HtmlNode node)
        {
            var downloads = node.Descendants("td")
                .ElementAt(5)
                .InnerText
                .Trim();
            return int.Parse(downloads);
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
