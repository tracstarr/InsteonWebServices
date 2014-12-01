using System.Collections.Generic;
using Insteon.Daemon.Common.Request;
using Insteon.Daemon.Common.Response;
using Insteon.Network.Device;
using Insteon.Network.Enum;
using Insteon.Network.Helpers;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface.ServiceModel;

namespace Insteon.Daemon.Common.Service
{

    public class InsteonService : IService
    {
        private readonly InsteonManager manager;
        private readonly SmartThingsSettings settings;
        
        public InsteonService(InsteonManager manager, SmartThingsSettings settings)
        {
            this.manager = manager;
            this.settings = settings;
        }
        
        public GetDevicesResponse Any(GetDevices request)
        {
            var result = new GetDevicesResponse() { Devices = new List<DeviceInfo>() };

            foreach (InsteonDevice device in manager.Network.Devices)
            {
                result.Devices.Add(new DeviceInfo(){Address = device.Address.ToString(), Category = device.Identity.GetDeviceCategoryName()});
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
            settings.AccessToken = request.AccessToken;
            settings.Location = request.Location;
            settings.ApplicationId = request.AppId;

            var cb = new SmartThingsCallbacks(settings);

            return !cb.Authorization() ? new ResponseStatus("404", "Couldn't connect to ST hub") : new ResponseStatus();
        }

        public ResponseStatus Any(SmartThingsSettingsResetRequest request)
        {
            settings.AccessToken = null;
            settings.Location = null;
            settings.ApplicationId = null;

            var cb = new SmartThingsCallbacks(settings);

            return !cb.AuthorizationRevoke() ? new ResponseStatus("404", "Couldn't connect to ST hub") : new ResponseStatus();
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