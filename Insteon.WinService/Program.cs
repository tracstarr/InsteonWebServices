using System;
using System.Configuration;
using System.Threading;
using Insteon.Daemon.Common;


namespace Insteon.WinService
{
	static class Program
	{
		static void Main()
		{

            

            var url = ConfigurationManager.AppSettings["SmartAppUrl"];
            var hostedOn = ConfigurationManager.AppSettings["listenOn"];
			var insteonConnection = ConfigurationManager.AppSettings ["insteonConnection"];

			var appHost = new InsteonAppListenerHost(insteonConnection, new Uri(url));
            
#if DEBUG
         //   LogManager.LogFactory = new ConsoleLogFactory();
			Console.WriteLine("Running Insteon.WinService in Console mode");
			try
			{
				appHost.Init();
				appHost.Start(hostedOn);

				Console.WriteLine("Press <CTRL>+C to stop.");
				Thread.Sleep(Timeout.Infinite);
			}
			catch (Exception ex)
			{
				Console.WriteLine("ERROR: {0}: {1}", ex.GetType().Name, ex.Message);
				throw;
			}
			finally
			{
				appHost.Stop();
			}

			Console.WriteLine("Insteon.WinService has finished");

#else
            LogManager.LogFactory = new ConsoleLogFactory(false);
			//When in RELEASE mode it will run as a Windows Service with the code below

			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[] 
			{ 
				new WinService(appHost, ListeningOn) 
			};
			ServiceBase.Run(ServicesToRun);
#endif

        }
	}
}
