using System;
using System.Configuration;
using System.Reflection;
#if MONO
using Mono.Unix;
using Mono.Unix.Native;
#endif
using Insteon.Daemon.Common;

namespace Insteon.Daemon
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = ConfigurationManager.AppSettings["SmartAppUrl"];
            var hostedOn = ConfigurationManager.AppSettings["listenOn"];

            //Initialize app host
            var appHost = new InsteonAppListenerHost("serial: COM5", new Uri(url));
            appHost.Init();
            appHost.Start(hostedOn);
#if MONO
            UnixSignal[] signals = new UnixSignal[] { 
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
#endif
        }

    }

}