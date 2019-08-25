using System.IO;

namespace SubFinder.Models
{
    public class Episode : Media
    {
        public string SeriesFolder { private get; set; }
        public override string Folder => $"{SeriesFolder}{Path.DirectorySeparatorChar}Season {SeasonString}";
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public int TvDbEpisodeId { get; set; }
        public bool Proper { get; set; }
        private string SeasonString => SeasonNumber < 10 ? SeasonNumber.ToString("D2") : SeasonNumber.ToString();
        private string EpisodeString => EpisodeNumber < 10 ? EpisodeNumber.ToString("D2") : EpisodeNumber.ToString();

        public string EpisodeQualifier => $"S{SeasonString}E{EpisodeString}";
    }
}
