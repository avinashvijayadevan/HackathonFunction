using HackathonFunction;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Pensive
{
	// Calculates the delta population for each city by subtracting the number of people 
	//who have moved into the city with number of people who have moved out of the city
	public static class DeltaCalculator
	{
		
		private static List<AvgPopulation> avgPopulations = null;
		 static DeltaCalculator()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = "HackathonFunction.AvgPopulation.json";

			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			using (StreamReader reader = new StreamReader(stream))
			{
				string jsonFile = reader.ReadToEnd(); //Make string equal to full file
				avgPopulations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AvgPopulation>>(jsonFile);
			}
		}
		
		
		public static void CalculateDeltaPopulationFromDummyData(TravelContext dbContext)
		{
			dbContext.DeltaPopulations.RemoveRange(dbContext.DeltaPopulations.Where(x => x.IsPredicted == false));
			dbContext.SaveChanges();

			List<DestinationAggregation> destinationList = dbContext.DestinationAggregations.ToList();
			List<OrignAggregation> originList = dbContext.OrignAggregations.ToList();
			List<DeltaPopulation> currentPopulationList = dbContext.DeltaPopulations.ToList();

			foreach (DestinationAggregation da in destinationList)
			{
				var match = originList.Find(oa => da.Destination == oa.Origin && da.Year == oa.Year && da.Month == oa.Month);
				DeltaPopulation delta = null;
				if (match != null)
				{
					delta = new DeltaPopulation();
					delta.Place = da.Destination;
					delta.DeltaCount = da.Count - match.Count;
					delta.Year = da.Year;
					delta.Month = da.Month;
				}
				else
				{
					delta = new DeltaPopulation();
					delta.Place = da.Destination;
					delta.DeltaCount = da.Count;
					delta.Year = da.Year;
					delta.Month = da.Month;
				}

				var cityPopulation = avgPopulations.Where(i => i.City == da.Destination).SingleOrDefault();
				if (cityPopulation != null)
				{
					var yearAvg = cityPopulation.Avg.Where(i => i.Year == da.Year).FirstOrDefault();
					if(yearAvg != null)
					delta.DeltaCount += yearAvg?.Month;
				}
				dbContext.Add(delta);
			}

			dbContext.SaveChanges();
		}
	}
}
