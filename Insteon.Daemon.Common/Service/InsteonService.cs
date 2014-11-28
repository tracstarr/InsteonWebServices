using System.Collections.Generic;
using Insteon.Daemon.Common.Request;
using Insteon.Daemon.Common.Response;
using Insteon.Network.Device;
using Insteon.Network.Enum;
using ServiceStack;

namespace Insteon.Daemon.Common.Service
{

    public class InsteonService : ServiceStack.Service
    {
        private readonly InsteonManager manager;
        private readonly SmartThingsSettings settings;

        public InsteonService(InsteonManager manager, SmartThingsSettings settings)
        {
            this.manager = manager;
            this.settings = settings;
        }

        //public async Task<GetDevicesResponse> Any(GetDevices request)
        public GetDevicesResponse Any(GetDevices request)
        {
            var result = new GetDevicesResponse(){Result = new List<string>()};

            foreach (InsteonDevice device in manager.Network.Devices)
            {
                result.Result.Add(device.Address.ToString());
            }


            return result;
        }

        public object Any(GetStatus request)
        {
            return new
            {
                Address = manager.Connection.Address.ToString(),
                manager.Connection.Name,
                manager.Connection.Type,
                manager.Network.IsConnected,
                manager.Network.Devices.Count
            };

        }

        public ResponseStatus Any(SmartThingsSettingsRequest request)
        {
            // always reset values
            settings.AuthenticationToken = request.AuthToken;
            settings.Location = request.Location;
            settings.Url = string.Format("https://graph.api.smartthings.com/api/smartapps/installations/${0}/verify", request.Url);

            // some sort of callback to above url for test purpose?
            var client = new JsonServiceClient("https://graph.api.smartthings.com");
            var result = client.Put(string.Format("api/smartapps/installations/${0}/authReply?access_token={1}", request.Url, settings.AuthenticationToken));


            return new ResponseStatus();
        }

        public ResponseStatus Any(EnterLinkModeRequest request)
        {
            if (request.Start && manager.Network.Controller.IsInLinkingMode)
            {
                return new ResponseStatus();
            }
            if (request.Start && !manager.Network.Controller.IsInLinkingMode)
            {
                manager.Network.Controller.EnterLinkMode(InsteonLinkMode.Controller, 0x01);
                return new ResponseStatus();
            }
            if (!request.Start && manager.Network.Controller.IsInLinkingMode)
            {
                manager.Network.Controller.CancelLinkMode();
                return new ResponseStatus();
            }

            return new ResponseStatus("404", "Cannot change to provided linking mode.");
        }


    }
}