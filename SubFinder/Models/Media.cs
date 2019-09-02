﻿namespace SubFinder.Models
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
    }
}
