using SubFinder.Models;
using System.Threading.Tasks;

namespace SubFinder.Providers
{
    public interface ISubtitleProvider
    {
        string ProviderName { get; }
        Task SearchForMovieAsync(Movie movie);
        Task SearchForEpisodeAsync(Episode episode);
    }
}
