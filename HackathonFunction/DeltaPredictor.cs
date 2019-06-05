using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Pensive
{
    //Calls the Machine Learning service and gets the predicted populatiion for next 36 months from current month.
    //Saves the predicted population to database
    //The ML algorithm can either be Linear Regression or Decision Tree. Any one of these can be used.
    class DeltaPredictor
    {
        public static async Task PredictMigrationPopulation(TravelContext dbContext)
        {
            List<DeltaPopulation> currentPopulationList = dbContext.DeltaPopulations.ToList();

            List<string> places = (from deltaRecord in currentPopulationList
                                   select deltaRecord.Place).Distinct().ToList();

            StringTable inpuToMLService = new StringTable();
            inpuToMLService.Values = new string[places.Count * 36, 3];
            int rowIndex = 0;
            foreach (string place in places)
            {
                for (int i = 1; i <= 36; i++)
                {
                    inpuToMLService.Values.SetValue(place, rowIndex, 0);
                    inpuToMLService.Values.SetValue(DateTime.Today.AddMonths(i).Year.ToString(), rowIndex, 1);
                    inpuToMLService.Values.SetValue(DateTime.Today.AddMonths(i).Month.ToString(), rowIndex, 2);
                    rowIndex += 1;
                }
            }

            using (var client = new HttpClient())
            {
                var scoreRequest = new
                {
                    Inputs = new Dictionary<string, StringTable>() {
                        {
                            "input1",
                            new StringTable()
                            {
                                ColumnNames = new string[] {"Place", "Year", "Month"},
                                Values =inpuToMLService.Values
                            }
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>() { { "Append score columns to output", "True" }, }
                };

                string apiUrl = null;
                string apiKey = null;
                apiUrl = ConfigurationManager.AppSettings["LinerRegressionAPI"];
                apiKey = ConfigurationManager.AppSettings["LinerRegressionKey"];

                if (!bool.Parse(ConfigurationManager.AppSettings["UseLinerRegressionAlgorithm"]))
                {
                    apiUrl = ConfigurationManager.AppSettings["BoostedDecisionTreeRegressionAPI"];
                    apiKey = ConfigurationManager.AppSettings["BoostedDecisionTreeRegressionKey"];
                }


                client.BaseAddress = new Uri(apiUrl + "&details=true");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest);

                if (response.IsSuccessStatusCode)
                {
                    dbContext.DeltaPopulations.RemoveRange(dbContext.DeltaPopulations.Where(x => x.IsPredicted == true));
                    dbContext.SaveChanges();

                    string jsonRequest = await response.Content.ReadAsStringAsync();
                    RootObject mlResponse = JsonConvert.DeserializeObject<RootObject>(jsonRequest);
                    List<List<string>> predictedrecords = mlResponse.Results.output1.value.Values;
                    foreach (List<string> predictedRecord in predictedrecords)
                    {
                        DeltaPopulation prediction = new DeltaPopulation();
                        prediction.Place = predictedRecord[0];
                        prediction.Year = Int32.Parse(predictedRecord[1]);
                        prediction.Month = Int32.Parse(predictedRecord[2]);
                        prediction.DeltaCount = (int)decimal.Parse(predictedRecord[3]);
                        prediction.IsPredicted = true;
                        dbContext.Add(prediction);
                    }
                    dbContext.SaveChanges();
                }
                else
                {
                    Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

                    // Print the headers - they include the requert ID and the timestamp, which are useful for debugging the failure
                    Console.WriteLine(response.Headers.ToString());

                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseContent);
                }
            }
        }
    }
}
