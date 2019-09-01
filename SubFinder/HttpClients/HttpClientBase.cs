using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SubFinder.HttpClients
{
    public class HttpClientBase
    {
        protected readonly HttpClient _client;

        protected HttpClientBase(HttpClient client)
        {
            _client = client;
        }

        public async Task<Stream> GetStreamAsync(string requestUri)
        {
            return await _client.GetStreamAsync(requestUri);
        }
    }
}
