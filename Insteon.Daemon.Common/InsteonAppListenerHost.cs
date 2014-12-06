using Insteon.Daemon.Common.Service;
using Insteon.Network;
using ServiceStack.Razor;
using ServiceStack.WebHost.Endpoints;

namespace Insteon.Daemon.Common
{
	public class InsteonAppListenerHost
		: AppHostHttpListenerBase
	{
	    private readonly string insteonSource;
	    
	    public InsteonAppListenerHost(string insteonSource)
			: base("Insteon HttpListener", typeof(InsteonService).Assembly)
		{
		    this.insteonSource = insteonSource;
		}

	    public override void Configure(Funq.Container container)
		{
            Plugins.Add(new RazorFormat());
            var manager = new InsteonManager(insteonSource);
            container.Register(manager);
	        container.Register(new SmartThingsSettings());

            SetConfig(new EndpointHostConfig()
            {
                CustomHttpHandlers ={ { System.Net.HttpStatusCode.NotFound, new RazorHandler("/notfound")}}
            });
		}

	    public override void Start(string urlBase)
	    {
	       base.Start(urlBase);
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
