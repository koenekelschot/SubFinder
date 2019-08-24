using Microsoft.Extensions.Configuration;
using SubFinder.Exceptions;
using System.Linq;

namespace SubFinder.Extensions
{
    public static class ConfigurationExtensions
    {
        public static IConfigurationSection TryGetSection(this IConfiguration root, string section)
        {
            var config = root.GetChildren().FirstOrDefault(child => child.Key == section);
            
            if (config != null)
            {
                return config;
            }

            throw new ConfigurationException($"No configuration section found for name `{section}`");
        }
    }
}
