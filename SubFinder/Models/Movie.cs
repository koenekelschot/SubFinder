namespace SubFinder.Models
{
    public class Movie : Media
    {
        public string MovieFolder { private get; set; }
        public override string Folder => MovieFolder;
        public long TmdbId { get; set; }
        public string ReleaseGroup { get; set; }
        public string Edition { get; set; }
    }
}
