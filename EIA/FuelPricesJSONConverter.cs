using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;

using Innovoft.Text.JSON;

namespace EIA
{
	/// <summary>
	/// Handling situation if Date Format changes because it is being supplied in the response.
	/// </summary>
	public static class FuelPricesJSONConverter
	{
		#region Constants
		private const string defaultDateFormat = "yyyy-MM-dd";
		#endregion //Constants

		#region Fields
		private static readonly byte[] propertyResponse = Encoding.UTF8.GetBytes("response");
		private static readonly byte[] propertyDateFormat = Encoding.UTF8.GetBytes("dateFormat");
		private static readonly byte[] propertyData = Encoding.UTF8.GetBytes("data");

		private static readonly Dictionary<string, string> formats = new Dictionary<string, string>()
		{
			{ "", defaultDateFormat },
			{ "YYYY-MM-DD", defaultDateFormat },
		};
		private static SpinLock formatsLock = new SpinLock(false);
		#endregion //Fields

		#region Methods
		public static void Parse(Stream streamStream, byte[] buffer, Action<FuelPrice> action)
		{
			using (var stream = new UTF8JSONReaderStream(streamStream, buffer))
			{
				var reader = stream.Create();
				Parse(stream, ref reader, action);
			}
		}

		public static void Parse(UTF8JSONReaderStream stream, ref Utf8JsonReader reader, Action<FuelPrice> action)
		{
			if (!stream.Read(ref reader))
			{
				throw new EndOfStreamException();
			}
			if (reader.TokenType != JsonTokenType.StartObject)
			{
				throw new FormatException();
			}
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
				if (reader.ValueTextEquals(propertyResponse))
				{
					ParseResponse(stream, ref reader, action);
				}
				else
				{
					if (!stream.TrySkip(ref reader))
					{
						throw new EndOfStreamException();
					}
				}
			}
		}

		private static void ParseResponse(UTF8JSONReaderStream stream, ref Utf8JsonReader reader, Action<FuelPrice> action)
		{
			if (!stream.Read(ref reader))
			{
				throw new EndOfStreamException();
			}
			if (reader.TokenType != JsonTokenType.StartObject)
			{
				throw new FormatException();
			}
			var dateFormat = defaultDateFormat;
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
				if (reader.ValueTextEquals(propertyDateFormat))
				{
					if (!stream.Read(ref reader))
					{
						throw new EndOfStreamException();
					}
					dateFormat = GetDateFormat(reader.GetString());
				}
				else if (reader.ValueTextEquals(propertyData))
				{
					ParseDatas(stream, ref reader, dateFormat, action);
				}
				else
				{
					if (!stream.TrySkip(ref reader))
					{
						throw new EndOfStreamException();
					}
				}
			}
		}

		private static string GetDateFormat(string? value)
		{
			if (value == null)
			{
				return defaultDateFormat;
			}
			//Get
			var locked = false;
			try
			{
				formatsLock.Enter(ref locked);

				if (formats.TryGetValue(value, out var format))
				{
					return format;
				}
			}
			finally
			{
				if (locked)
				{
					formatsLock.Exit();
				}
			}
			//Create
			var updated = value
				.Replace('Y', 'y')
				.Replace('D', 'd');
			locked = false;
			try
			{
				formatsLock.Enter(ref locked);

				if (formats.TryGetValue(value, out var format))
				{
					return format;
				}
				else
				{
					formats[value] = updated;
					return updated;
				}
			}
			finally
			{
				if (locked)
				{
					formatsLock.Exit();
				}
			}
		}

		private static void ParseDatas(UTF8JSONReaderStream stream, ref Utf8JsonReader reader, string dateFormat, Action<FuelPrice> action)
		{
			if (!stream.Read(ref reader))
			{
				throw new EndOfStreamException();
			}
			if (reader.TokenType != JsonTokenType.StartArray)
			{
				throw new FormatException();
			}
			while (true)
			{
				if (!stream.Read(ref reader))
				{
					throw new EndOfStreamException();
				}
				if (reader.TokenType == JsonTokenType.EndArray)
				{
					return;
				}
				if (reader.TokenType != JsonTokenType.StartObject)
				{
					throw new FormatException();
				}
				var value = FuelPriceJSONConverter.ParseStarted(stream, ref reader, dateFormat);
				action(value);
			}
		}
		#endregion //Methods
	}
}
