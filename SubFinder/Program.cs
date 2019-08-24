﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SubFinder.Config;
using SubFinder.Extensions;
using SubFinder.Scanners;
using SubFinder.Scanners.Implementations;
using System;
using System.IO;

namespace SubFinder
{
    class Program
    {
        private static IServiceProvider _serviceProvider;
        private static ILogger<Program> _logger;

        static void Main(string[] args)
        {
            var config = GetConfiguration();
            Configure(config);

            _logger = _serviceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger<Program>();

            _logger.LogInformation("Application started");

            _logger.LogInformation("Application finished");
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

            services.Configure<RadarrConfig>(configuration.TryGetSection("radarr"));
            services.Configure<SonarrConfig>(configuration.TryGetSection("sonarr"));

            _serviceProvider = services.BuildServiceProvider();
        }
    }
}