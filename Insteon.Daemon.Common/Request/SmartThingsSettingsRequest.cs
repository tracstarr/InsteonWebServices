using ServiceStack.ServiceHost;

namespace Insteon.Daemon.Common.Request
{
    [Route("/configure/{location}/{AppId}/{AccessToken}")]
    public class SmartThingsSettingsRequest
    {
        public string Location { get; set; }
        public string AppId { get; set; }
        public string AccessToken { get; set; }
    }
}