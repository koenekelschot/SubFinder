using SubFinder.Languages;
using System.IO;

namespace SubFinder.Models
{
    public abstract class Media
    {
        public string Title { get; set; }
        public int Year { get; set; }
        public string ImdbId { get; set; }
        public abstract string Folder { get; }
        public string File { get; set; }
        public string OriginalName { get; set; }
        public string Quality { get; set; }

        public string SubtitlePath(Language.IsoLanguage language)
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(File);
            var languageSuffix = Language.GetIsoPart1(language);

            return $"{Folder}{Path.DirectorySeparatorChar}{fileNameWithoutExtension}.{languageSuffix}.srt";
        }
    }
}
