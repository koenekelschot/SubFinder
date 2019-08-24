using SubFinder.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SubFinder.Scanners
{
    public interface IMediaScanner
    {
        string ScannerName { get; }
        Task<IList<Media>> GetDownloadedItemsAsync();
    }
}
