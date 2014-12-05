using System.ComponentModel;
using ServiceStack.ServiceHost;

namespace Insteon.Daemon.Common.Request
{
    [Description("Connect to the insteon network")]
    [Route("/connect")]
    public class ConnectToInsteonNetworkRequest
    {
    }
}
