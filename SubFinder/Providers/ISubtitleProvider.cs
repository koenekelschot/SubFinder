using SubFinder.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SubFinder.Providers
{
    public interface ISubtitleProvider
    {
        string ProviderName { get; }
        Task<IEnumerable<Subtitle>> SearchForMovieAsync(Movie movie);
        Task<IEnumerable<Subtitle>> SearchForEpisodeAsync(Episode episode);
        Task<Memory<byte>> DownloadAsync(Subtitle subtitle);
    }
}
