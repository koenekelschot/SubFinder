using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        private readonly IList<string> _languages = new List<string>();

        public OpenSubtitlesSubtitleProvider(
            IOptions<SubtitleConfig> config)
        {
            foreach (var preferredLanguage in config.Value.PreferredLanguages)
            {
                _languages.Add(Language.GetIsoPart2Bibliographic(preferredLanguage));
            }
        }

        public Task SearchForEpisodeAsync(Episode episode)
        {
            throw new NotImplementedException();
        }

        public Task SearchForMovieAsync(Movie movie)
        {
            throw new NotImplementedException();
        }
    }
}
