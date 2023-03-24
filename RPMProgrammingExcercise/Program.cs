using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Xml;

using Innovoft.Collections;

using EIA;

namespace RPMProgrammingExcercise
{
	/// <summary>
	/// Program can run as a windows service or a console app
	/// </summary>
	internal static class Program
	{
		#region Main
		private static int Main(string[] args)
		{
			Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			StartLog();

			AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;

			//TODO: Update Program

			var errors = ParseCommandLine(args);
			if (errors != null)
			{
				PrintHelp(errors);
				FinishLog();
				return -1;
			}

			//Config
			if (!Config())
			{
				FinishLog();
				return -1;
			}

			Prepare();
			if (Environment.UserInteractive)
			{
				RunConsole();
			}
			else
			{
				RunService();
			}
			return Environment.ExitCode;
		}
		#endregion //Main

		#region Fields
		private static string configPath = "Config.config";
		private static ProcessPriorityClass priority = ProcessPriorityClass.Normal;

		private static int buffersLength;
		private static Converter<int, byte[]> buffersRent;
		private static Action<byte[], bool> buffersReturn;
		private static readonly HttpClient client = new HttpClient();
		private static string call;
		private static TimeSpan delay;
		private static TimeZoneInfo ageTimeZone;
		private static TimeSpan ageTimeSpan;
		private static string dbConnectionString;
		private static readonly RedBlackTree<DateTime, DateTime> dbContains = new RedBlackTree<DateTime, DateTime>(DateTimeAscendingComparison.Comparison);
		private static DateTime dbOldest;
		private static readonly List<FuelPrice> dbAdd = new List<FuelPrice>();
		private static readonly Action<FuelPrice> addFuelPrice = AddFuelPrice;

		private static int stopped;
		private static ManualResetEvent wait;
		private static Timer timer;
		private static int finished;
		private static Service? service;
		private static bool terminated;
		#endregion //Fields

		#region Properties
		internal static bool Terminated => terminated;
		#endregion //Properties

		#region Methods
		private static List<string>? ParseCommandLine(string[] args)
		{
			if (args == null || args.Length == 0)
			{
				return null;
			}

			var errors = new List<string>();

			for (var i = 0; i < args.Length; ++i)
			{
				switch (args[i])
				{
				default:
					errors.Add("Invalid Parameter " + args[i]);
					break;

				case "-Config":
					try
					{
						configPath = args[++i];
					}
					catch (Exception exception)
					{
						errors.Add("Problems processing -Config " + exception.Message);
					}
					break;
				}
			}

			if (string.IsNullOrEmpty(configPath))
			{
				errors.Add("Invalid -Config");
			}

			if (errors.Count > 0)
			{
				return errors;
			}

			return null;
		}

		private static void PrintHelp(List<string> errors)
		{
			Console.WriteLine();
			Console.WriteLine("RPMProgrammingExcercise.exe");
			Console.WriteLine(" Optional");
			Console.WriteLine("  -Config Config.config");
			Console.WriteLine();

			if (errors != null && errors.Count > 0)
			{
				foreach (var error in errors)
				{
					Console.Error.WriteLine(error);
				}
				Console.WriteLine();
				Console.WriteLine(Environment.CommandLine);
				Console.WriteLine();
			}
		}

		private static void Prepare()
		{
			//Wait
			wait = new ManualResetEvent(false);

			PrepareTimer();

			//Environment
			try
			{
				Process.GetCurrentProcess().PriorityClass = priority;
			}
			catch (Exception exception)
			{
				//TODO: Log
			}
			try
			{
				GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
			}
			catch (Exception exception)
			{
				//TODO: Log
			}

			GC.Collect();
		}

		private static void PrepareTimer()
		{
			timer = new Timer(OnTimer, null, Timeout.Infinite, Timeout.Infinite);
		}

		private static void RunConsole()
		{
			Console.Title = "RPMProgrammingExcercise";
			Console.CancelKeyPress += CancelKeyPress;

			Start();
			Wait();
			Finish();
		}

		private static void CancelKeyPress(object? sender, ConsoleCancelEventArgs args)
		{
			args.Cancel = true;

			//TODO: Log

			Stop();
		}

		private static void TerminateConsole()
		{
			//TODO: Log

			Environment.ExitCode = -1;
			Stop();
		}

		private static void RunService()
		{
			service = new Service();
			terminated = false;
			ServiceBase.Run(service);
		}

		private static void TerminateService()
		{
			terminated = true;
			Environment.ExitCode = -1;
			service.ExitCode = -1;
			service.Stop();
		}

		internal static void Start()
		{
			//TODO: Log
			StartStarting();
			GC.Collect();
		}

		private static void StartStarting()
		{
			LoadDBContains();
			timer.Change(0, Timeout.Infinite);
		}

		internal static void Stop()
		{
			if (Interlocked.CompareExchange(ref stopped, 1, 0) == 0)
			{
				StopStop();
				wait.Set();
			}
		}

		private static void StopStop()
		{
			timer.Change(Timeout.Infinite, Timeout.Infinite);
		}

		internal static void Wait()
		{
			wait.WaitOne();
		}

		internal static void Finish()
		{
			if (Interlocked.CompareExchange(ref finished, 1, 0) == 0)
			{
				FinishFinish();
				FinishLog();
			}
		}

		private static void FinishFinish()
		{
		}

		private static void StartLog()
		{
			//TODO: Log
		}

		private static void FinishLog()
		{
			//TODO: Log
		}

		/// <summary>
		/// Loads dbContains from the DB
		/// </summary>
		private static void LoadDBContains()
		{
			SetDBOldest(DateTime.UtcNow);

			SqlConnection? connection = null;
			SqlCommand? command = null;
			SqlDataReader? reader = null;
			try
			{
				connection = new SqlConnection(dbConnectionString);
				connection.Open();

				command = connection.CreateCommand();
				command.CommandType = CommandType.Text;
				command.CommandText = "SELECT [Period] FROM [FuelPrices] WHERE [Period] >= @period";
				var period = command.CreateParameter();
				period.ParameterName = "@period";
				period.DbType = DbType.Date;
				period.Direction = ParameterDirection.Input;
				period.Value = dbOldest;
				command.Parameters.Add(period);

				reader = command.ExecuteReader();
				while (reader.Read())
				{
					var date = reader.GetDateTime(0);
					dbContains.Set(date, date);
				}
			}
			catch (Exception exception)
			{
				//TODO: Email
				//TODO: Log
			}
			finally
			{
				if (reader != null)
				{
					reader.Close();
					reader.Dispose();
				}
				command?.Dispose();
				if (connection != null)
				{
					connection.Close();
					connection.Dispose();
				}
			}
		}

		private static void OnTimer(object state)
		{
			//For scheduling and dbOldest
			var started = DateTime.UtcNow;

			//Prepare
			dbAdd.Clear();

			//Work
			SetDBOldest(started);
			PrepareDBContains();
			GetFuelPrices();
			SaveFuelPrices();

			//Cleanup
			dbAdd.Clear();

			//Schedule
			if (stopped != 0)
			{
				return;
			}
			var when = started + delay;
			var now = DateTime.UtcNow;
			if (when <= now)
			{
				timer.Change(0, 0);
			}
			else
			{
				var wait = when - now;
				timer.Change(wait, Timeout.InfiniteTimeSpan);
			}
		}

		/// <summary>
		/// Sets dbOldest to be now - ageTimeSpan
		/// </summary>
		/// <param name="utc"></param>
		private static void SetDBOldest(DateTime utc)
		{
			var dateTime = TimeZoneInfo.ConvertTimeFromUtc(utc, ageTimeZone) - ageTimeSpan;
			dbOldest = dateTime.Date;
		}

		/// <summary>
		/// Prepares dbContains to not have entries older than dbOldest
		/// </summary>
		private static void PrepareDBContains()
		{
			while (dbContains.TryGetMinKey(out var key))
			{
				if (key >= dbOldest)
				{
					break;
				}

				dbContains.Remove(key);
			}
		}

		/// <summary>
		/// Gets FuelPrices from endPoint
		/// </summary>
		private static void GetFuelPrices()
		{
			byte[]? buffer = null;
			Stream? stream = null;
			try
			{
				buffer = buffersRent(buffersLength);
				stream = client.GetStreamAsync(call).Result;
				FuelPricesJSONConverter.Parse(stream, buffer, addFuelPrice);
			}
			catch (Exception exception)
			{
				//TODO: Email
				//TODO: Log
			}
			finally
			{
				stream?.Dispose();
				if (buffer != null)
				{
					buffersReturn(buffer, false);
				}
			}
		}

		/// <summary>
		/// Adds FuelPrice to dbAdd if passes filter
		/// </summary>
		private static void AddFuelPrice(FuelPrice value)
		{
			//Filter
			var period = value.Period;
			if (period < dbOldest || dbContains.ContainsKey(period))
			{
				return;
			}

			dbAdd.Add(value);
		}

		/// <summary>
		/// Saves FuelPrices to db
		/// </summary>
		private static void SaveFuelPrices()
		{
			if (dbAdd.Count <= 0)
			{
				return;
			}

			var enumerator = dbAdd.GetEnumerator();
			SqlConnection? connection = null;
			try
			{
				while (enumerator.MoveNext())
				{
					var value = enumerator.Current;
					var valuePeriod = value.Period;

					//Checking for duplicates
					if (dbContains.ContainsKey(valuePeriod))
					{
						continue;
					}

					if (connection == null)
					{
						connection = new SqlConnection(dbConnectionString);
						connection.Open();
					}

					SqlCommand? command = null;
					try
					{
						command = connection.CreateCommand();
						command.CommandType = CommandType.Text;
						command.CommandText = "INSERT INTO [FuelPrices] ([Period], [Price]) VALUES (@period, @price)";
						var period = command.CreateParameter();
						period.ParameterName = "@period";
						period.DbType = DbType.Date;
						period.Direction = ParameterDirection.Input;
						period.Value = valuePeriod;
						command.Parameters.Add(period);
						var price = command.CreateParameter();
						price.ParameterName = "@price";
						price.DbType = DbType.Double;
						price.Direction = ParameterDirection.Input;
						price.Value = value.Price;
						command.Parameters.Add(price);

						command.ExecuteNonQuery();
					}
					catch (Exception exception)
					{
						//TODO: Email
						//TODO: Log

						//Cleanup
						connection.Close();
						connection.Dispose();
						connection = null;

						//Next
						continue;
					}
					finally
					{
						command?.Dispose();
					}
					dbContains.Add(valuePeriod, valuePeriod);
				}
			}
			catch (Exception exception)
			{
				//TODO: Email
				//TODO: Log
			}
			finally
			{
				if (connection != null)
				{
					connection.Close();
					connection.Dispose();
				}
				enumerator.Dispose();
			}
		}

		private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
		{
			//TODO: Email

			//TODO: Log

			if (args.IsTerminating)
			{
				if (service == null)
				{
					TerminateConsole();
				}
				else
				{
					TerminateService();
				}
			}
		}
		#endregion //Methods

		#region Config
		private static bool Config()
		{
			var document = new XmlDocument();
			try
			{
				document.Load(configPath);
			}
			catch (Exception exception)
			{
				//TODO: Log
				return false;
			}
			var config = document.DocumentElement;

			ConfigPriority(config.SelectSingleNode("Priority"));
			ConfigBuffers(config.SelectSingleNode("Buffers"));
			if (!ConfigEndPoint(config.SelectSingleNode("EndPoint")))
			{
				return false;
			}
			if (!ConfigDelay(config.SelectSingleNode("Delay")))
			{
				return false;
			}
			if (!ConfigAge(config.SelectSingleNode("Age")))
			{
				return false;
			}
			if (!ConfigDB(config.SelectSingleNode("DB")))
			{
				return false;
			}

			return true;
		}

		private static void ConfigPriority(XmlNode? config)
		{
			if (config == null || string.IsNullOrEmpty(config.InnerText))
			{
				return;
			}

			try
			{
				priority = (ProcessPriorityClass)Enum.Parse(typeof(ProcessPriorityClass), config.InnerText, ignoreCase: true);
			}
			catch (Exception exception)
			{
				//TODO: Log
			}
		}

		private static void ConfigBuffers(XmlNode config)
		{
			ArrayPoolConverter.ConfigBuffers(config,
				out buffersLength, out buffersRent, out buffersReturn);
		}

		private static bool ConfigEndPoint(XmlNode config)
		{
			if (config == null)
			{
				//TODO: Log
				return false;
			}
			if (!ConfigEndPointBase(config.SelectSingleNode("Base")))
			{
				return false;
			}
			if (!ConfigEndPointCall(config.SelectSingleNode("Call")))
			{
				return false;
			}
			return true;
		}

		private static bool ConfigEndPointBase(XmlNode config)
		{
			if (config == null)
			{
				//TODO: Log
				return false;
			}
			var parse = config.InnerText;
			if (string.IsNullOrWhiteSpace(parse))
			{
				//TODO: Log
				return false;
			}
			try
			{
				client.BaseAddress = new Uri(parse);
			}
			catch (Exception exception)
			{
				//TODO: Log
				return false;
			}
			return true;
		}

		private static bool ConfigEndPointCall(XmlNode config)
		{
			if (config == null)
			{
				//TODO: Log
				return false;
			}
			var parse = config.InnerText;
			if (string.IsNullOrWhiteSpace(parse))
			{
				//TODO: Log
				return false;
			}
			try
			{
				call = parse;
			}
			catch (Exception exception)
			{
				//TODO: Log
				return false;
			}
			return true;
		}

		private static bool ConfigDelay(XmlNode config)
		{
			if (config == null)
			{
				//TODO: Log
				return false;
			}
			var parse = config.InnerText;
			if (string.IsNullOrWhiteSpace(parse))
			{
				//TODO: Log
				return false;
			}
			try
			{
				delay = TimeSpan.Parse(parse);
			}
			catch (Exception exception)
			{
				//TODO: Log
				return false;
			}
			return true;
		}

		private static bool ConfigAge(XmlNode config)
		{
			if (config == null)
			{
				//TODO: Log
				return false;
			}
			if (!ConfigAgeTimeZone(config.SelectSingleNode("TimeZone")))
			{
				return false;
			}
			if (!ConfigAgeDays(config.SelectSingleNode("Days")))
			{
				return false;
			}
			return true;
		}

		private static bool ConfigAgeTimeZone(XmlNode config)
		{
			if (config == null)
			{
				ageTimeZone = TimeZoneInfo.Local;
				return true;
			}
			var parse = config.InnerText;
			if (string.IsNullOrWhiteSpace(parse) || parse == "Local")
			{
				ageTimeZone = TimeZoneInfo.Local;
				return true;
			}
			try
			{
				ageTimeZone =  TimeZoneInfo.FindSystemTimeZoneById(parse);
			}
			catch (Exception exception)
			{
				//TODO: Log
				return false;
			}
			return true;
		}

		private static bool ConfigAgeDays(XmlNode config)
		{
			if (config == null)
			{
				//TODO: Log
				return false;
			}
			var parse = config.InnerText;
			if (!double.TryParse(parse, out var parsed) ||
				parsed <= 0)
			{
				return false;
			}
			ageTimeSpan = TimeSpan.FromDays(parsed);
			return true;
		}

		private static bool ConfigDB(XmlNode config)
		{
			if (config == null)
			{
				//TODO: Log
				return false;
			}
			if (!ConfigDBConnectionString(config.SelectSingleNode("ConnectionString")))
			{
				return false;
			}
			return true;
		}

		private static bool ConfigDBConnectionString(XmlNode config)
		{
			if (config == null)
			{
				//TODO: Log
				return false;
			}
			var parse = config.InnerText;
			if (string.IsNullOrWhiteSpace(parse))
			{
				//TODO: Log
				return false;
			}
			dbConnectionString = parse;
			return true;
		}
		#endregion //Config
	}
}