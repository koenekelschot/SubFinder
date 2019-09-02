using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Options;
using SubFinder.Config;
using SubFinder.Languages;
using SubFinder.Models;

namespace SubFinder.Scanners.Implementations
{
    public class SubtitleScanner : ISubtitleScanner
    {
        private readonly IList<Language.IsoLanguage> _languages;

        public SubtitleScanner(
            IOptions<SubtitleConfig> config)
        {
            _languages = config.Value.PreferredLanguages;
        }

        public bool HasSubtitle(Media media)
        {
            foreach (var language in _languages)
            {
                var subtitleFileName = media.SubtitlePath(language);
                if (File.Exists(subtitleFileName))
                {
                    return true;
                };
            }

            return false;
        }
    }
}
