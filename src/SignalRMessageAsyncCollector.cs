using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.SignalRService
{
    public class SignalRMessageAsyncCollector : IAsyncCollector<SignalRMessage>
    {
        private readonly AzureSignalR signalR;
        private readonly string hubName;
        
        public SignalRMessageAsyncCollector(SignalRConfiguration config, SignalRAttribute attr)
        {
            signalR = new AzureSignalR(attr.ConnectionStringSetting);
            hubName = attr.HubName;
        }
        
        public Task AddAsync(SignalRMessage item, CancellationToken cancellationToken = default(CancellationToken))
        {
            var httpClient = HttpClientFactory.GetInstance();
            var connectionInfo = signalR.GetServerConnectionInfo(hubName);
            return PostJsonAsync(httpClient, connectionInfo.Endpoint, item, connectionInfo.AccessKey);
        }

        public Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        private Task<HttpResponseMessage> PostJsonAsync(HttpClient httpClient, string url, object body, string bearer)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url)
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptCharset.Clear();
            request.Headers.AcceptCharset.Add(new StringWithQualityHeaderValue("UTF-8"));

            var content = JsonConvert.SerializeObject(body);
            request.Content = new StringContent(content, Encoding.UTF8, "application/json");
            return httpClient.SendAsync(request);
        }
    }
}