using ServiceStack.ServiceHost;

namespace Insteon.Daemon.Common.Request
{
    [Route("/configure/reset", "GET", Summary = "Reset SmartThings configuration.")]
    public class SmartThingsSettingsResetRequest
    { }
}