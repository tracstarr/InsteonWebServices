using System.Configuration;
using Mono.Unix;
using Mono.Unix.Native;

using Insteon.Daemon.Common;

namespace Insteon.Daemon
{
    class Program
    {
        static void Main(string[] args)
        {
            var hostedOn = ConfigurationManager.AppSettings["listenOn"];
			var insteonConnection = ConfigurationManager.AppSettings ["insteonConnection"];

            //Initialize app host
			var appHost = new InsteonAppListenerHost(insteonConnection);
            appHost.Init();
            appHost.Start(hostedOn);

			UnixSignal[] signals =
			{ 
			    new UnixSignal(Signum.SIGINT), 
			    new UnixSignal(Signum.SIGTERM), 
			};

            // Wait for a unix signal
            for (bool exit = false; !exit; )
            {
                int id = UnixSignal.WaitAny(signals);

                if (id >= 0 && id < signals.Length)
                {
                    if (signals[id].IsSet) exit = true;
                }
            }
        }

    }

}