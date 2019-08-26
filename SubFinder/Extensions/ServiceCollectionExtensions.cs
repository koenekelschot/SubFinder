using Microsoft.Extensions.DependencyInjection;
using SubFinder.Providers;

namespace SubFinder.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSubtitleProvider<T>(this IServiceCollection services) where T : class, ISubtitleProvider
        {
            services.AddSingleton<ISubtitleProvider, T>();
            services.AddHttpClient(nameof(T));

            return services;
        }
    }
}
