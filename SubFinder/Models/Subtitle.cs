using System;
using static SubFinder.Languages.Language;

namespace SubFinder.Models
{
    public class Subtitle
    {
        public string Provider { get; set; }
        public string Id { get; set; }
        public string ReleaseName { get; set; }
        public IsoLanguage Language { get; set; }
        public DateTime Added { get; set; }
        public int VotesBad { get; set; }
        public decimal Rating { get; set; }
        public long Downloads { get; set; }
    }
}
