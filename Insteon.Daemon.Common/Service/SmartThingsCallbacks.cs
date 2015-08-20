using Insteon.Daemon.Common.Settings;
using Insteon.Network.Device;
using RestSharp;
using ServiceStack.Logging;

namespace Insteon.Daemon.Common.Service
{
    internal class SmartThingsCallbacks
    {
        const string smartthingsUrl = "https://graph.api.smartthings.com";

        private readonly SmartThingsSettings settings;
        private readonly RestClient client;
        private readonly string rootPath;

        private readonly ILog logger = LogManager.GetLogger(typeof(SmartThingsCallbacks));
        public SmartThingsCallbacks(SmartThingsSettings settings)
        {
            this.settings = settings;
            client = new RestClient(smartthingsUrl);

            rootPath = string.Format("/api/smartapps/installations/{0}/", settings.ApplicationId);
        }

        public bool Authorization()
        {
            string path = string.Format("{0}link", rootPath);
            var request = new RestRequest(path, Method.GET) { RequestFormat = DataFormat.Json };
            request.AddQueryParameter("access_token", settings.AccessToken);

            var response = client.Execute(request);
            return response.Content.Contains("ok");
        }

        public bool AuthorizationRevoke()
        {
            string path = string.Format("{0}revoke", rootPath);
            var request = new RestRequest(path, Method.GET) { RequestFormat = DataFormat.Json };
            request.AddQueryParameter("access_token", settings.AccessToken);

            var response = client.Execute(request);
            return response.Content.Contains("ok");
        }

        public bool PushDeviceStatusUpdate(InsteonDevice device, InsteonDeviceStatus status)
        {
            //note: because the parent caller of this event is an InsteonDevice which is processing a current message received event, it's not been "reset" and we cannot
            //make a call to GetOnLevel for dimmable devices as no response will be handled. We must "ask" ST to make another rest call to obtain current state. All we can 
            //do here is tell it there is an update.

            string path = string.Format("{0}deviceupdate/{1}/{2}", rootPath, device.Address, status);
            var request = new RestRequest(path, Method.PUT) { RequestFormat = DataFormat.Json };

            request.AddQueryParameter("access_token", settings.AccessToken);

            var response = client.Execute(request);

            logger.InfoFormat("Content Returend from ST: {0}", response.Content);
            return response.Content.Contains("ok");
        }
    }
}
