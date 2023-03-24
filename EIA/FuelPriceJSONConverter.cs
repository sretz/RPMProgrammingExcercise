using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

using Innovoft.Text.JSON;

namespace EIA
{
	public static class FuelPriceJSONConverter
	{
		#region Fields
		private static readonly byte[] propertyPeriod = Encoding.UTF8.GetBytes("period");
		//private static readonly byte[] propertyDuoArea = Encoding.UTF8.GetBytes("duoarea");
		//private static readonly byte[] propertyAreaName = Encoding.UTF8.GetBytes("area-name");
		//private static readonly byte[] propertyProduct = Encoding.UTF8.GetBytes("product");
		//private static readonly byte[] propertyProductName = Encoding.UTF8.GetBytes("product-name");
		//private static readonly byte[] propertyProcess = Encoding.UTF8.GetBytes("process");
		//private static readonly byte[] propertyProcessName = Encoding.UTF8.GetBytes("process-name");
		//private static readonly byte[] propertySeries = Encoding.UTF8.GetBytes("series");
		//private static readonly byte[] propertySeriesDescription = Encoding.UTF8.GetBytes("series-description");
		private static readonly byte[] propertyValue = Encoding.UTF8.GetBytes("value");
		//private static readonly byte[] propertyUnits = Encoding.UTF8.GetBytes("units");
		#endregion //Fields

		#region Methods
		public static FuelPrice ParseStarted(UTF8JSONReaderStream stream, ref Utf8JsonReader reader, string dateFormat)
		{
			var value = new FuelPrice();
			ParseStarted(value, stream, ref reader, dateFormat);
			return value;
		}

		public static void ParseStarted(FuelPrice value, UTF8JSONReaderStream stream, ref Utf8JsonReader reader, string dateFormat)
		{
			while (true)
			{
				if (!stream.Read(ref reader))
				{
					throw new EndOfStreamException();
				}
				if (reader.TokenType == JsonTokenType.EndObject)
				{
					return;
				}
				if (reader.TokenType != JsonTokenType.PropertyName)
				{
					throw new FormatException();
				}
				if (reader.ValueTextEquals(propertyPeriod))
				{
					if (!stream.Read(ref reader))
					{
						throw new EndOfStreamException();
					}
					var parse = reader.GetString();
					value.Period = DateTime.ParseExact(parse, dateFormat, null);
				}
				//else if (reader.ValueTextEquals(propertyDuoArea))
				//{
				//	if (!stream.Read(ref reader))
				//	{
				//		throw new EndOfStreamException();
				//	}
				//	value.DuoArea = reader.GetString();
				//}
				//else if (reader.ValueTextEquals(propertyAreaName))
				//{
				//	if (!stream.Read(ref reader))
				//	{
				//		throw new EndOfStreamException();
				//	}
				//	value.AreaName = reader.GetString();
				//}
				//else if (reader.ValueTextEquals(propertyProduct))
				//{
				//	if (!stream.Read(ref reader))
				//	{
				//		throw new EndOfStreamException();
				//	}
				//	value.Product = reader.GetString();
				//}
				//else if (reader.ValueTextEquals(propertyProductName))
				//{
				//	if (!stream.Read(ref reader))
				//	{
				//		throw new EndOfStreamException();
				//	}
				//	value.ProductName = reader.GetString();
				//}
				//else if (reader.ValueTextEquals(propertyProcess))
				//{
				//	if (!stream.Read(ref reader))
				//	{
				//		throw new EndOfStreamException();
				//	}
				//	value.Process = reader.GetString();
				//}
				//else if (reader.ValueTextEquals(propertyProcessName))
				//{
				//	if (!stream.Read(ref reader))
				//	{
				//		throw new EndOfStreamException();
				//	}
				//	value.ProcessName = reader.GetString();
				//}
				//else if (reader.ValueTextEquals(propertySeries))
				//{
				//	if (!stream.Read(ref reader))
				//	{
				//		throw new EndOfStreamException();
				//	}
				//	value.Series = reader.GetString();
				//}
				//else if (reader.ValueTextEquals(propertySeriesDescription))
				//{
				//	if (!stream.Read(ref reader))
				//	{
				//		throw new EndOfStreamException();
				//	}
				//	value.SeriesDescription = reader.GetString();
				//}
				else if (reader.ValueTextEquals(propertyValue))
				{
					if (!stream.Read(ref reader))
					{
						throw new EndOfStreamException();
					}
					value.Price = Utf8JsonReaderConverter.GetDouble(ref reader);
				}
				//else if (reader.ValueTextEquals(propertyUnits))
				//{
				//	if (!stream.Read(ref reader))
				//	{
				//		throw new EndOfStreamException();
				//	}
				//	value.Units = reader.GetString();
				//}
				else
				{
					if (!stream.TrySkip(ref reader))
					{
						throw new EndOfStreamException();
					}
				}
			}
		}
		#endregion //Methods
	}
}
