using Microsoft.Extensions.Logging;
using SubFinder.Scanners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubFinder
{
    public class TestRunner
    {
        private readonly ILogger<TestRunner> _logger;
        private readonly IEnumerable<IMediaScanner> _scanners;

        public TestRunner(
            ILogger<TestRunner> logger,
            IEnumerable<IMediaScanner> scanners)
        {
            _logger = logger;
            _scanners = scanners;
        }

        public async Task Run(string scannerName)
        {
            var scanner = _scanners.FirstOrDefault(s => s.ScannerName == scannerName);
            var items = await scanner?.GetDownloadedItemsAsync();

            foreach (var item in items)
            {
                _logger.LogInformation(item.ToString());
            }
        }
    }
}
