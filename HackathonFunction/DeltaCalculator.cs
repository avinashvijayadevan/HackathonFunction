using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Pensive
{
    // Calculates the delta population for each city by subtracting the number of people 
    //who have moved into the city with number of people who have moved out of the city
    class DeltaCalculator
    {
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
                if (match != null)
                {
                    DeltaPopulation delta = new DeltaPopulation();
                    delta.Place = da.Destination;
                    delta.DeltaCount = da.Count - match.Count;
                    delta.Year = da.Year;
                    delta.Month = da.Month;
                    dbContext.Add(delta);
                }
                else
                {
                    DeltaPopulation delta = new DeltaPopulation();
                    delta.Place = da.Destination;
                    delta.DeltaCount = da.Count;
                    delta.Year = da.Year;
                    delta.Month = da.Month;
                    dbContext.Add(delta);
                }
            }

            dbContext.SaveChanges();
        }
    }
}
