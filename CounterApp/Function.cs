using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Newtonsoft.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNet.SignalR;

namespace CounterApp
{
    internal class Function
    {
        private static HttpClient httpClient = new HttpClient();
        private static string Etag = string.Empty;
        private static string StarCount = "0";


        [FunctionName("negotiate")]
        public static SignalRConnectionInfo Negotiate(
             [HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req,
             [SignalRConnectionInfo(HubName = "serverless")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }

        [FunctionName("broadcast")]
        public static async Task Broadcast([TimerTrigger("*/2 * * * * *")] TimerInfo myTimer,
        [SignalR(HubName = "serverless")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://functionapp220220419172755.azurewebsites.net/api/GetCounter");
            request.Headers.UserAgent.ParseAdd("serverless");
            request.Headers.Add("If-None-Match", Etag);
            var response = await httpClient.SendAsync(request);
            if (response.Headers.Contains("Etag"))
            {
                Etag = response.Headers.GetValues("Etag").First();
            }
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = JsonConvert.DeserializeObject<GitResult>(await response.Content.ReadAsStringAsync());
                StarCount = result.StarCount;
            }
            //var serv = "serverless";
            //IHubContext context = GlobalHost.ConnectionManager.GetHubContext();
            //MyHub.SendMessage("The Message!");

            await signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "newMessage",
                    Arguments = new[] { $"Current counter is: {StarCount}" }
                });
        }

        private class GitResult
        {
            [JsonRequired]
            [JsonProperty("counter")]
            public string StarCount { get; set; }
        }
    }
}
