using ServiceStack;

namespace Insteon.Daemon.Common.Request
{
    [Route("/connect", "GET", Summary = "Connect to the insteon network")]
    public class ConnectToInsteonNetworkRequest
    {
    }
}
