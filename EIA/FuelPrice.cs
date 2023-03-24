using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EIA
{
	[DebuggerDisplay("{Period} {Price}")]
	public class FuelPrice
	{
		#region Properties
		public DateTime Period { get; set; }
		//public string DuoArea { get; set; }
		//public string AreaName { get; set; }
		//public string Product { get; set; }
		//public string ProductName { get; set; }
		//public string Process { get; set; }
		//public string ProcessName { get; set; }
		//public string Series { get; set; }
		//public string SeriesDescription { get; set; }
		public double Price { get; set; }
		//public string Units { get; set; }
		#endregion //Properties
	}
}
