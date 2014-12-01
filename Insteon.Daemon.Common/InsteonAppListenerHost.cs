using System;
using Insteon.Daemon.Common.Service;
using Insteon.Network;
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
            // will throw if cannot make connection
            var manager = new InsteonManager(insteonSource);
            container.Register(manager);
	        container.Register(new SmartThingsSettings());
		}

	    public override void Start(string urlBase)
	    {
	        var manager = Container.Resolve<InsteonManager>();
	        var connected = manager.Connect();

	        if (!connected)
	        {
	            throw new Exception("Could not connect to Insteon Controller");
	        }
            
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
