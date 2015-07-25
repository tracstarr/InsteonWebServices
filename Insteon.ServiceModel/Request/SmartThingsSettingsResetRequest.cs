using ServiceStack;

namespace Insteon.ServiceModel.Request
{
    [Route("/configure/reset", "GET", Summary = "Reset SmartThings configuration.")]
    public class SmartThingsSettingsResetRequest
    { }
}