using Microsoft.Extensions.Logging;
using SubFinder.Models;
using SubFinder.Scanners;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SubFinder
{
    public class TestRunner
    {
        private readonly ILogger<TestRunner> _logger;
        private readonly IEnumerable<IMediaScanner> _scanners;
        private readonly ISubtitleScanner _subtitleScanner;

        public TestRunner(
            ILogger<TestRunner> logger,
            IEnumerable<IMediaScanner> scanners,
            ISubtitleScanner subtitleScanner)
        {
            _logger = logger;
            _scanners = scanners;
            _subtitleScanner = subtitleScanner;
        }

        public async Task Run()
        {
            var missingSubtitles = new List<Media>();
            var scanTasks = new List<Task<IList<Media>>>();

            foreach (var scanner in _scanners)
            {
                scanTasks.Add(scanner.GetDownloadedItemsAsync());
            }

            await Task.WhenAll(scanTasks);

            foreach (var scanResult in scanTasks)
            {
                foreach (var media in await scanResult)
                {
                    if (!_subtitleScanner.HasSubtitle(media))
                    {
                        missingSubtitles.Add(media);
                        _logger.LogWarning($"No subtitle for {media.Title}");
                    }
                }
            }
        }
    }
}
