using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Xunit;

using EIA;

namespace EIA.Tests
{
	public class FuelPriceConverterTests
	{
		[Fact]
		public void FuelPriceParseTest()
		{
			var buffer = new byte[1024 * 1024];
			var values = new List<FuelPrice>();
			using (var stream = new FileStream("api.eia.gov.json", FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				FuelPricesJSONConverter.Parse(stream, buffer, values.Add);
			}
			Assert.Equal(1514, values.Count);
			var value = values[0];
			Assert.Equal(new DateTime(2023, 03, 20), value.Period);
			//Assert.Equal("NUS", value.DuoArea);
			//Assert.Equal("U.S.", value.AreaName);
			//Assert.Equal("EPD2D", value.Product);
			//Assert.Equal("No 2 Diesel", value.ProductName);
			//Assert.Equal("PTE", value.Process);
			//Assert.Equal("Retail Sales", value.ProcessName);
			//Assert.Equal("EMD_EPD2D_PTE_NUS_DPG", value.Series);
			//Assert.Equal("U.S. No 2 Diesel Retail Prices (Dollars per Gallon)", value.SeriesDescription);
			Assert.Equal(4.185, value.Price);
			//Assert.Equal("$/GAL", value.Units);
		}
	}
}