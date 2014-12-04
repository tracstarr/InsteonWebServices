using System;
using System.Threading;
using Insteon.Daemon.Common;


namespace Insteon.WinService
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
            try
            {
                ServiceStartup.Main();
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
                ServiceStartup.Stop();
            }
#else
            var appHost = ServiceStartup.GetAppHostListner();
            
			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[] 
			{ 
				new WinService(appHost, ServiceStartup.ListeningOn) 
			};
			ServiceBase.Run(ServicesToRun);
#endif

        }
    }
}
