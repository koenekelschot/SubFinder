using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SubFinder.Config;

namespace SubFinder.Scanners.Implementations
{
    public class SonarrScanner : IMediaScanner
    {
        private readonly SonarrConfig _config;

        public SonarrScanner(IOptions<SonarrConfig> config)
        {
            _config = config.Value;
        }

        public Task GetAllItemsAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
