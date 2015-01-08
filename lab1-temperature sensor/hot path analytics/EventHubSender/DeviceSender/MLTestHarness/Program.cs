using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MLTestHarness
{
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

    class Program
    {
        static void Main(string[] args)
        {
            var harness = new MachineLearningDeviceHarness();
            harness.InvokeRequestResponseService();
            Console.Read();
        }

    }

    public class MachineLearningDeviceHarness
    {
        public async void InvokeRequestResponseService()
        {
            using (var client = new HttpClient())
            {
                var scoreData = new ScoreData()
                {
                    FeatureVector = new Dictionary<string, string>()
                    {
                        {"temperature", "27.7894573765598762342"},
                    },
                    GlobalParameters =
                        new Dictionary<string, string>()
                        {
                        }
                };

                var scoreRequest = new ScoreRequest()
                {
                    Id = Guid.NewGuid().ToString("D"),
                    Instance = scoreData
                };

                const string apiKey =
                    "YA3EPPvvfk5wM7c1jI+Rp56zFnl8t5BFslorkQsfF4XIfqSL0mPaNce8wh1YB/hwLZCDihNQ6Ks4Oa5L47GkHQ==";
                // Replace this with the API key for the web service
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                client.BaseAddress =
                    new Uri(
                        "https://ussouthcentral.services.azureml.net/workspaces/2503fddba20a4d86854828bfe7fbda24/services/548d7d9ae5e249d19e594f8940cc3577/score");
                HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest);
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Result: {0}", result);
                }
                else
                {
                    Console.WriteLine("Failed with status code: {0}", response.StatusCode);
                }
            }
        }
    }
}
