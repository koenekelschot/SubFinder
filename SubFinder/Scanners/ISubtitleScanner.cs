using SubFinder.Models;
using System.Threading.Tasks;

namespace SubFinder.Scanners
{
    public interface ISubtitleScanner
    {
        bool HasSubtitle(Media media);
    }
}
