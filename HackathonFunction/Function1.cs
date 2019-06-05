using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Pensive;
//microsoft.servicebus.messaging

namespace HackathonFunction
{
    public static class Function1
    {
        [FunctionName("PensiveJobRun")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            string id = req.GetQueryNameValuePairs().FirstOrDefault(q => string.Compare(q.Key, "id", true) == 0).Value;
            int argumentId = 0;
            Int32.TryParse(id, out argumentId);
            AssemblyVersionResolver.RedirectAssembly();
            TravelContext dbContext = new TravelContext();

            //1 - Generates only dummy data
            //2 - Calculates the delta population for the dummy data imported via data factory job.
            //3 - Predicts the population for next 3 years from the current month - calls the ML Service
            //23 - Does 2 and 3 together
            switch (argumentId)
            {
                case 1:
                    DummyDataSimulator.GenerateDummyTravelData(dbContext);
                    break;
                case 2:
                    DeltaCalculator.CalculateDeltaPopulationFromDummyData(dbContext);
                    break;
                case 3:
                    DeltaPredictor.PredictMigrationPopulation(dbContext).Wait();
                    break;
                case 23:
                    DeltaCalculator.CalculateDeltaPopulationFromDummyData(dbContext);
                    DeltaPredictor.PredictMigrationPopulation(dbContext).Wait();
                    break;
                default:
                    break;
            }

            await Task.Delay(1);
            return id == null
                  ? new HttpResponseMessage(HttpStatusCode.BadRequest)
                  {
                      Content = new StringContent(JsonConvert.SerializeObject("Please pass a name on the query string or in the request body"), Encoding.UTF8, "application/json")
                  }
                  : new HttpResponseMessage(HttpStatusCode.OK)
                  {
                      Content = new StringContent(JsonConvert.SerializeObject(new { result = "Hello " + id }), Encoding.UTF8, "application/json")
                  };
        }
    }
}
