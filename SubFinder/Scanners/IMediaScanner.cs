using System.Threading.Tasks;

namespace SubFinder.Scanners
{
    interface IMediaScanner
    {
        Task GetAllItemsAsync();
    }
}
