using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SubFinder.Config;

namespace SubFinder.Scanners.Implementations
{
    public class RadarrScanner : IMediaScanner
    {
        private readonly RadarrConfig _config;

        public RadarrScanner(IOptions<RadarrConfig> config)
        {
            _config = config.Value;
        }

        public Task GetAllItemsAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
