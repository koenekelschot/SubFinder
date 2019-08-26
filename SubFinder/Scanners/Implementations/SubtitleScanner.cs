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
        private const string SearchPattern = "*.srt";
        private readonly IList<string> _languageSuffixes = new List<string>();

        public SubtitleScanner(
            IOptions<SubtitleConfig> config)
        {
            foreach(var preferredLanguage in config.Value.PreferredLanguages)
            {
                var isoPart = Language.GetIsoPart1(preferredLanguage);
                _languageSuffixes.Add($".{isoPart}");
            }
        }

        public bool HasSubtitle(Media media)
        {
            var directoryInfo = new DirectoryInfo(media.Folder);
            if (!directoryInfo.Exists)
            {
                return false;
            }

            var subtitles = directoryInfo.GetFiles(SearchPattern);
            if (subtitles.Length == 0)
            {
                return false;
            }

            foreach (var subtitle in subtitles)
            {
                var filenameWithoutExtension = Path.GetFileNameWithoutExtension(subtitle.Name);
                foreach (var languageSuffix in _languageSuffixes)
                {
                    if (filenameWithoutExtension.EndsWith(languageSuffix))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
