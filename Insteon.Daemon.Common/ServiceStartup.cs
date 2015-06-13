using System;
using System.Configuration;
using ServiceStack;
using ServiceStack.Logging;

namespace Insteon.Daemon.Common
{
    public static class ServiceStartup
    {
        static InsteonAppListenerHost appHost;

        public static string ListeningOn
        {
            get
            {
                return ConfigurationManager.AppSettings["listenOn"];
            }
        }

        public static void Main()
        {
            LogManager.LogFactory = new ConsoleLogFactory();
            var logger = LogManager.GetLogger(typeof(ServiceStartup));

            try
            {
                var insteonConnection = ConfigurationManager.AppSettings["insteonConnection"];

                appHost = new InsteonAppListenerHost(insteonConnection);

                appHost.Init();
                appHost.Start(ListeningOn);
                logger.InfoFormat("Listening On: {0}", ListeningOn);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("{0}: {1}", ex.GetType().Name, ex.Message);
                throw;
            }
        }

        public static void Stop()
        {
            if (appHost != null)
            {
                appHost.Stop();
            }

        }

        public static AppHostHttpListenerBase GetAppHostListner()
        {
            LogManager.LogFactory = new ConsoleLogFactory();
            var insteonConnection = ConfigurationManager.AppSettings["insteonConnection"];
            return new InsteonAppListenerHost(insteonConnection);
        }

    }
}
