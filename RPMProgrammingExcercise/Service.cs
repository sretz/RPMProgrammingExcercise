using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace RPMProgrammingExcercise
{
	partial class Service : ServiceBase
	{
		public Service()
		{
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
			ThreadPool.QueueUserWorkItem(Start);
		}

		private static void Start(object? state)
		{
			Program.Start();
		}

		protected override void OnStop()
		{
			if (Program.Terminated)
			{
				//TODO: Log.
			}
			else
			{
				//TODO: Log
			}

			Program.Stop();
			Program.Wait();
			Program.Finish();
		}

		protected override void OnShutdown()
		{
			//TODO: Log

			Program.Stop();
			Program.Wait();
			Program.Finish();
		}
	}
}
