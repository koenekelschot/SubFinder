using Microsoft.Extensions.Logging;
using SubFinder.Models;
using SubFinder.Scanners;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SubFinder.Activities
{
    public class ScanActivity
    {
        private readonly ILogger<ScanActivity> _logger;
        private readonly IEnumerable<IMediaScanner> _scanners;
        private readonly ISubtitleScanner _subtitleScanner;
        private readonly DownloadSubtitleActivity _downloadSubtitleActivity;

        public ScanActivity(
            ILogger<ScanActivity> logger,
            IEnumerable<IMediaScanner> scanners,
            ISubtitleScanner subtitleScanner,
            DownloadSubtitleActivity downloadSubtitleActivity)
        {
            _logger = logger;
            _scanners = scanners;
            _subtitleScanner = subtitleScanner;
            _downloadSubtitleActivity = downloadSubtitleActivity;
        }

        public async Task ExecuteAsync()
        {
            var library = await ScanLibraryAsync();
            var downloadTasks = new List<Task>();

            foreach (var media in library)
            {
                if (!_subtitleScanner.HasSubtitle(media))
                {
                    _logger.LogWarning($"No subtitle found for {media.Title}");
                    downloadTasks.Add(_downloadSubtitleActivity.ExecuteAsync(media));
                }
            }

            await Task.WhenAll(downloadTasks);
        }

        private async Task<IList<Media>> ScanLibraryAsync()
        {
            var library = new List<Media>();
            var scanTasks = new List<Task<IList<Media>>>(_scanners.Count());

            foreach (var scanner in _scanners)
            {
                scanTasks.Add(scanner.GetDownloadedItemsAsync());
            }

            await Task.WhenAll(scanTasks);

            foreach (var scanResult in scanTasks)
            {
                library.AddRange(await scanResult);
            }

            return library;
        }
    }
}
