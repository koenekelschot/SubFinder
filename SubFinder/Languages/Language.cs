using System;
using System.Linq;

namespace SubFinder.Languages
{
    public static class Language
    {
        //Out of laziness only English and Dutch are added
        //Other languages can be added in accordance with ISO-639-3
        //A list of languages can be found at http://www-01.sil.org/iso639-3/iso-639-3_Latin1.tab
        //via http://www-01.sil.org/iso639-3/download.asp
        public enum IsoLanguage
        {
            [LanguageInfo(IsoPart1 = "en", IsoPart2Bibliographic = "eng", IsoPart2Terminological = "eng")]
            English,
            [LanguageInfo(IsoPart1 = "nl", IsoPart2Bibliographic = "dut", IsoPart2Terminological = "nld")]
            Dutch
        }

        private static readonly Type LanguageType = typeof(IsoLanguage);

        public static IsoLanguage GetLanguageFromString(string language)
        {
            return (IsoLanguage)Enum.Parse(LanguageType, language, true);
        }

        public static string GetIsoPart1(IsoLanguage language)
        {
            var attribute = GetLanguageInfoAttribute(language);
            return attribute.IsoPart1;
        }

        public static string GetIsoPart2Bibliographic(IsoLanguage language)
        {
            var attribute = GetLanguageInfoAttribute(language);
            return attribute.IsoPart2Bibliographic;
        }

        public static string IsoPart2Terminological(IsoLanguage language)
        {
            var attribute = GetLanguageInfoAttribute(language);
            return attribute.IsoPart2Terminological;
        }

        private static LanguageInfoAttribute GetLanguageInfoAttribute(IsoLanguage language)
        {
            var languageName = Enum.GetName(LanguageType, language);
            return LanguageType.GetField(languageName)
                .GetCustomAttributes(false)
                .OfType<LanguageInfoAttribute>()
                .SingleOrDefault();
        }
    }
}
