using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SubFinder.Activities;
using SubFinder.Config;
using SubFinder.Extensions;
using SubFinder.Scanners;
using SubFinder.Scanners.Implementations;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SubFinder
{
    class Program
    {
        private static IServiceProvider _serviceProvider;
        private static ILogger<Program> _logger;

        static async Task Main(string[] args)
        {
            var config = GetConfiguration();
            Configure(config);

            _logger = _serviceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger<Program>();

            _logger.LogInformation("Application started");

            var activity = _serviceProvider.GetRequiredService<ScanActivity>();
            await activity.ExecuteAsync();

            _logger.LogInformation("Application finished");

            Console.ReadKey();
        }

        private static IConfigurationRoot GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.env.json", optional: true);

            return builder.Build();
        }

        private static void Configure(IConfigurationRoot configuration)
        {
            var services = new ServiceCollection()
                .AddLogging(config =>
                {
                    config.AddConsole().SetMinimumLevel(LogLevel.Debug);
                });

            services.AddSingleton<IMediaScanner, RadarrScanner>();
            services.AddSingleton<IMediaScanner, SonarrScanner>();
            services.AddSingleton<ISubtitleScanner, SubtitleScanner>();

            services.AddSingleton<ScanActivity>();
            services.AddSingleton<DownloadSubtitleActivity>();
            services.AddSingleton<SearchEpisodeSubtitlesActivity>();
            services.AddSingleton<SearchMovieSubtitlesActivity>();

            services.Configure<RadarrConfig>(configuration.TryGetSection("Radarr"));
            services.Configure<SonarrConfig>(configuration.TryGetSection("Sonarr"));
            services.Configure<SubtitleConfig>(configuration.TryGetSection("Subtitles"));

            _serviceProvider = services.BuildServiceProvider();
        }
    }
}
