using Insteon.Daemon.Common.Settings;
using Insteon.Network.Device;
using RestSharp;

namespace Insteon.Daemon.Common.Service
{
    internal class SmartThingsCallbacks
    {
        const string smartthingsUrl = "https://graph.api.smartthings.com";

        private readonly SmartThingsSettings settings;
        private readonly RestClient client;
        private readonly string rootPath;

        public SmartThingsCallbacks(SmartThingsSettings settings)
        {
            this.settings = settings;
            client = new RestClient(smartthingsUrl);

            rootPath = string.Format("/api/smartapps/installations/{0}/", settings.ApplicationId);
        }

        public bool Authorization()
        {
            string path = string.Format("{0}link", rootPath);
            var request = new RestRequest(path, Method.GET) {RequestFormat = DataFormat.Json};
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
            string path = string.Format("{0}device/{1}/{2}", rootPath, device.Address, status);
            var request = new RestRequest(path, Method.PUT) { RequestFormat = DataFormat.Json };
            request.AddQueryParameter("access_token", settings.AccessToken);

            var response = client.Execute(request);
            return response.Content.Contains("ok");  
        }
    }
}
