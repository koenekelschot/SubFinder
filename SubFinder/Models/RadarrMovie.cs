using System;
using System.Collections.Generic;
using System.Text;

namespace SubFinder.Models
{
    public class RadarrMovie : Media
    {

    }

    public class SonarrEpisode : Media
    {

    }

    public abstract class Media
    {
        public string Title { get; set; }
        public string ImdbId { get; set; }
        public string Path { get; set; }

        public int? SeasonNumber { get; set; } //episodes
        public int? EpisodeNumber { get; set; } //episodes
        public int? TvDbEpisodeId { get; set; } //episodes
        public long? TmdbId { get; set; } //movies

        public string OriginalName { get; set; }
        public string ReleaseGroup { get; set; } //movies
        public string Edition { get; set; } //movies
        public string Quality { get; set; }
        public bool? Proper { get; set; } //episodes

        public override string ToString()
        {
            return $"{Title}\r\nIMDB: {ImdbId}, TMDB: {TmdbId}\r\nPath: {Path}\r\nOriginalName: {OriginalName}\r\nReleasegroup: {ReleaseGroup}\r\nEdition: {Edition}\r\nQuality: {Quality}";
        }
    }
}
