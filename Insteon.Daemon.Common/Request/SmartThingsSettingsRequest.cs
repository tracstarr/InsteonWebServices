using ServiceStack;

namespace Insteon.Daemon.Common.Request
{
    [Route("/configure/{location}/{url}/{authToken}")]
    public class SmartThingsSettingsRequest
    {
        public string Location { get; set; }
        public string Url { get; set; }
        public string AuthToken { get; set; }
    }
}