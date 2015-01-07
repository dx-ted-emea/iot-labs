using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace businessrules.AzureML
{
    public class DeviceReliabilityServiceClient
    {
        private FailAction _defaultReliability;
        public DeviceReliabilityServiceClient(FailAction defaultReliability)
        {
            _defaultReliability = defaultReliability;
        }

        public enum FailAction
        {
            Unreliable,
            Reliable
        }

        public class ScoreData
        {
            public Dictionary<string, string> FeatureVector { get; set; }
            public Dictionary<string, string> GlobalParameters { get; set; }
        }

        public class ScoreRequest
        {
            public string Id { get; set; }
            public ScoreData Instance { get; set; }
        }

        public async Task<bool> IsDeviceReliable(string correlationId, double temperature)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    ScoreData scoreData = new ScoreData()
                    {
                        FeatureVector = new Dictionary<string, string>()
                    {
                        {"temperature", temperature.ToString()},
                    },
                        GlobalParameters = new Dictionary<string, string>() { }

                    };

                    ScoreRequest scoreRequest = new ScoreRequest()
                    {
                        Id = correlationId,
                        Instance = scoreData
                    };

                    string apiKey = CloudConfigurationManager.GetSetting("AzureMLApiKey");
                    string endpoint = CloudConfigurationManager.GetSetting("AzureMLEndpoint");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                    client.BaseAddress = new Uri(endpoint);
                    HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest);
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        
                        var array = JArray.Parse(result);

                        return array.Last.Value<int>() == 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("There was an error with AzureML: {0}", ex);
            }

            return _defaultReliability == FailAction.Reliable;
        }
    }
}
