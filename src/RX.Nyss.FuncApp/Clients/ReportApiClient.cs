using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RX.Nyss.FuncApp.Contracts;

namespace RX.Nyss.FuncApp.Clients
{
    public class ReportApiClient
    {
        private readonly HttpClient _client;

        public ReportApiClient(HttpClient client)
        {
            _client = client;
        }

        public async Task PostReport(Report report)
        {
            var payload = JsonConvert.SerializeObject(report);
            await _client.PostAsync(new Uri("/api/report", UriKind.Relative), new StringContent(payload, Encoding.UTF8, "application/json"));
        }
    }
}
