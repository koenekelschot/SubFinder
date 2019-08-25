using System.IO;

namespace SubFinder.Models
{
    public class SonarrEpisode : Media
    {
        public string SeriesFolder { private get; set; }
        public override string Folder => $"{SeriesFolder}{Path.DirectorySeparatorChar}Season {SeasonString}";
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public int TvDbEpisodeId { get; set; }
        public bool Proper { get; set; }
        private string SeasonString => SeasonNumber < 10 ? SeasonNumber.ToString("D2") : SeasonNumber.ToString();
    }
}
