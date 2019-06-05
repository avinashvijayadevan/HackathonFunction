using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Configuration;
using System.Threading;

namespace Pensive
{
    // This class generates the dummy travel data for all the given destinations randomly and saves it to database.
    class DummyDataSimulator
    {
        public static void GenerateDummyTravelData(TravelContext dbContext)
        {
            List<PassengerInfo> passengerInfoList = new List<PassengerInfo>();
            int reqCount = Int32.Parse(ConfigurationManager.AppSettings["NoOfTravelRecordsToGenerate"]);
            List<string> destList = (ConfigurationManager.AppSettings["Destinations"].Split(",".ToCharArray())).ToList<string>();

            Parallel.ForEach(destList, desti =>
            {
                Parallel.For(0, reqCount, i =>
                {
                    PassengerInfo passenger = new PassengerInfo();
                    Random randomNumber = new Random(2);
                    var age = GetRandomDate(1945);
                    var travelDate = GetRandomDate(2015).Item1.Date;
                    passenger.Gender = randomNumber.Next(1, 2);
                    passenger.StringDateOfBirth =
                    (age.Item1 < DateTime.MinValue) ? String.Format("{0:MM/dd/yyyy}", DateTime.Today.AddMonths(-4).Date) : String.Format("{0:MM/dd/yyyy}", age.Item1.Date);
                    passenger.Mode = (TravelMode)(new Random(1)).Next(1, 3);
                    passenger.StringTravelDate =
                    (travelDate < DateTime.MinValue) ? String.Format("{0:MM/dd/yyyy}", DateTime.Today.AddDays(-30)) : String.Format("{0:MM/dd/yyyy}", travelDate);
                    passenger.Origin = GetRandomPlaces().Item1;
                    passenger.Destination = desti;
                    Console.WriteLine(passenger.DateOfBirth + "--" + passenger.Origin + "--" + passenger.Destination + "--" + passenger.Age + "--" + passenger.TravelDate);
                    passengerInfoList.Add(passenger);
                });
            });

            Thread.Sleep(30000);
            int counter = 1;
            foreach (PassengerInfo passenger in passengerInfoList)
            {
                TravelRawData travelRecord = new TravelRawData()
                {
                    DateOfBirth = passenger.StringDateOfBirth,
                    Destination = passenger.Destination,
                    Gender = passenger.Gender == 1 ? true : false,
                    Mode = (int)passenger.Mode,
                    Origin = passenger.Origin,
                    TravelDate = passenger.StringTravelDate,
                };

                dbContext.Add(travelRecord);
                counter += 1;
                if (counter >= 100)
                {
                    dbContext.SaveChanges();
                    counter = 1;
                }
            }

            dbContext.SaveChanges();

            //string fileNameSuffix = DateTime.Now.ToString().Replace("/", "-").Replace(":", "-") + ".csv";
            //ExportToFile.CreateCSV<PassengerInfo>(passengerInfoList, "travelInfoSampe_" + fileNameSuffix);
        }
        private static Tuple<string> GetRandomPlaces()
        {
            string[] origin = System.Configuration.ConfigurationManager.AppSettings["Origins"].Split(",".ToCharArray());
            int y = ((new Random()).Next(10000, origin.Count() * 10000)) / 10000;
            return new Tuple<string>(origin[y]);
        }
        private static Tuple<DateTime, int> GetRandomDate(int fromYear)
        {
            DateTime start = new DateTime(fromYear, 1, 1);
            int range = (DateTime.Today - start).Days;
            DateTime randomDate = start.AddDays((new Random()).Next(range));
            return new Tuple<DateTime, int>(randomDate, DateTime.Today.Year - randomDate.Year);
        }

    }
}
