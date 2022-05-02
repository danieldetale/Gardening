using Azure;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using System.Net.Http;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using System.Reactive.Subjects;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.AspNetCore.Mvc;



namespace CounterApp
{
    public partial class MainPage : ContentPage
    {
        HubConnection hubConnection;
        private static readonly HttpClient client = new HttpClient();


        public MainPage()
        {
            InitializeComponent();
            SyncCounters();
        }

        async private void SyncCounters()
        {
            SignalRSetup();
            await SignalRConnect();
        }

        private void SignalRSetup()
        {
            hubConnection = new HubConnectionBuilder().WithUrl($"https://functionapp220220419172755.azurewebsites.net/api/").Build();
            hubConnection.On<string>("newMessage", (message) =>
            {
                var receivedMessage = $"{message}";
                label1.Text = receivedMessage;
                //label2.Text = "Current counter 2 is: 0";
            });

        }

        async Task SignalRConnect()
        {
            try
            {
                await hubConnection.StartAsync();
            }
            catch (Exception ex)
            {

                label2.Text = ex.ToString();
            }
        }

        private async void Button_Clicked_1Async(object sender, EventArgs e)
        {   
            var response = await client.GetAsync("https://functionapp220220419172755.azurewebsites.net/api/GetCounter");
            var content = await response.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(content);
            var counter = data?.counter;
            await Task.Yield();
            int num = Convert.ToInt32(counter) + 1;
            await client.GetAsync($"https://functionapp220220419172755.azurewebsites.net/api/newupdate?value={num.ToString()}");
        }
        private void Button_Clicked_2(object sender, EventArgs e)
        {
            Increase(label2);
        }

        private void Increase(Label label)
        {
            int num = int.Parse(label.Text);
            num++;
            label.Text = num.ToString();
        }
    }

}
