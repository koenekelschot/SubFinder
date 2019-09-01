using SubFinder.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SubFinder.Providers
{
    public interface ISubtitleProvider
    {
        string ProviderName { get; }
        Task<IList<Subtitle>> SearchForMovieAsync(Movie movie);
        Task<IList<Subtitle>> SearchForEpisodeAsync(Episode episode);
        Task<Memory<byte>> DownloadAsync(Subtitle subtitle);
    }
}
