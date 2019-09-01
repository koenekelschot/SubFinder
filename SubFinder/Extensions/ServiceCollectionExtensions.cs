using Microsoft.Extensions.DependencyInjection;
using SubFinder.HttpClients;
using SubFinder.Providers;

namespace SubFinder.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSubtitleProvider<TProvider, TClient>(this IServiceCollection services) 
            where TProvider : class, ISubtitleProvider
            where TClient : HttpClientBase
        {
            services.AddSingleton<ISubtitleProvider, TProvider>();
            services.AddHttpClient<TClient>();

            return services;
        }
    }
}
