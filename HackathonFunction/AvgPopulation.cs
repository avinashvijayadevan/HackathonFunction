using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackathonFunction
{
	public class AvgPopulation
	{
		public string City { get; set; }
		public List<Avg> Avg { get; set; }
	}

	public class Avg
	{
		public int Year { get; set; }
		public int Month { get; set; }
	}
}
