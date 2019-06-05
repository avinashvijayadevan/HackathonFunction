using System;
using System.Collections.Generic;

namespace Pensive
{
    //All the entities used in this project are in this file.
    public class Value
    {
        public List<string> ColumnNames { get; set; }
        public List<string> ColumnTypes { get; set; }
        public List<List<string>> Values { get; set; }
    }
    public class Output1
    {
        public string type { get; set; }
        public Value value { get; set; }
    }
    public class Results
    {
        public Output1 output1 { get; set; }
    }
    public class RootObject
    {
        public Results Results { get; set; }
    }
    public class StringTable
    {
        public string[] ColumnNames { get; set; }
        public string[,] Values { get; set; }
    }
    public class PassengerInfo
    {
        public string StringDateOfBirth { get; set; }
        public string StringTravelDate { get; set; }
        public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-30);
        public string Origin { get; set; } = "Ahmedabad";
        public string Destination { get; set; } = "Mumbai";
        public TravelMode Mode { get; set; } = TravelMode.Train;
        public DateTime TravelDate { get; set; } = DateTime.Today.AddMonths(-3);
        public int Age { get; set; } = 30;
        public int Gender { get; set; } = 1;
    }
    public enum TravelMode
    {
        Bus = 1,
        Train,
        Air
    }
}
