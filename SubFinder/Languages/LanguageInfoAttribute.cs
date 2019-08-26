using System;

namespace SubFinder.Languages
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class LanguageInfoAttribute : Attribute
    {
        public string IsoPart2Bibliographic { get; set; }
        public string IsoPart2Terminological { get; set; }
        public string IsoPart1 { get; set; }
    }
}
