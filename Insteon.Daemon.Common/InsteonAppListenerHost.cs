using Funq;
using Insteon.Daemon.Common.Service;
using Insteon.Daemon.Common.Settings;
using Insteon.Network;
using ServiceStack;
using ServiceStack.Api.Swagger;
using ServiceStack.Logging;
using ServiceStack.Text;
using SettingsProviderNet;

namespace Insteon.Daemon.Common
{
    public class InsteonAppListenerHost
        : AppHostHttpListenerBase
    {
        private readonly string insteonSource;
        private readonly ILog logger = LogManager.GetLogger(typeof(InsteonAppListenerHost));

        public InsteonAppListenerHost(string insteonSource)
            : base("Insteon HttpListener", typeof(InsteonService).Assembly)
        {
            this.insteonSource = insteonSource;
        }

        public override void Configure(Container container)
        {
            JsConfig.EmitCamelCaseNames = true;
            Plugins.Add(new SwaggerFeature());
            var manager = new InsteonManager(insteonSource);
            container.Register(manager);
            var settingsProvider = new SettingsProvider(new RoamingAppDataStorage("Insteon"));
            var mySettings = settingsProvider.GetSettings<SmartThingsSettings>();

            container.Register(mySettings);

            manager.Network.Devices.DeviceStatusChanged += (sender, data) =>
            {
                logger.DebugFormat("{0}:{1}", data.Device.Address, data.DeviceStatus);
                var settings = container.Resolve<SmartThingsSettings>();
                var cb = new SmartThingsCallbacks(settings);
                cb.PushDeviceStatusUpdate(data.Device, data.DeviceStatus);
            };

            this.GlobalResponseFilters.Add((req, res, dto) =>
            {

                res.AddHeader("X-SmartThings", ServiceName);

            });

            manager.Connect();

        }

        public override void Stop()
        {
            var network = Container.Resolve<InsteonNetwork>();

            if (network != null && network.IsConnected)
            {
                network.Close();
            }

            base.Stop();
        }
    }
}
