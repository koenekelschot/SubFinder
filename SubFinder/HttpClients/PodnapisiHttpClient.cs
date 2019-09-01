using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace SubFinder.HttpClients
{
    public class PodnapisiHttpClient : HttpClientBase
    {
        private const string HtmlMediaType = "text/html";

        public PodnapisiHttpClient(HttpClient client) : base(client)
        {
            _client.BaseAddress = new Uri("https://www.podnapisi.net/");
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(HtmlMediaType));
        }
    }
}
