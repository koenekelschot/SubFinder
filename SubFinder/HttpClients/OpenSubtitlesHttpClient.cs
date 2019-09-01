using System;
using System.Net.Http;

namespace SubFinder.HttpClients
{
    public class OpenSubtitlesHttpClient : HttpClientBase
    {
        public OpenSubtitlesHttpClient(HttpClient client) : base(client)
        {
            _client.BaseAddress = new Uri("https://www.opensubtitles.org/");
        }
    }
}
