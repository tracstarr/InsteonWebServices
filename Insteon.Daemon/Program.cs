using Mono.Unix;
using Mono.Unix.Native;
using Insteon.Daemon.Common;

namespace Insteon.Daemon
{
    class Program
    {
        static void Main()
        {
            ServiceStartup.Main();

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