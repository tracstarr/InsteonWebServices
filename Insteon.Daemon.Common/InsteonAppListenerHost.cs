using System;
using Insteon.Daemon.Common.Service;
using Insteon.Network;
using ServiceStack;

namespace Insteon.Daemon.Common
{
	public class InsteonAppListenerHost
		: AppHostHttpListenerBase
	{
	    private readonly string insteonSource;
	    private readonly Uri smartAppUri;

	    public InsteonAppListenerHost(string insteonSource, Uri smartAppUri)
			: base("Insteon HttpListener", typeof(InsteonService).Assembly)
		{
		    this.insteonSource = insteonSource;
		    this.smartAppUri = smartAppUri;
		}

	    public override void Configure(Funq.Container container)
		{
            // will throw if cannot make connection
            var manager = new InsteonManager(insteonSource, smartAppUri);
            container.Register(manager);
	        container.Register(new SmartThingsSettings());
		}

	    public override ServiceStackHost Start(string urlBase)
	    {
	        var manager = Container.Resolve<InsteonManager>();
	        var connected = manager.Connect();

	        if (!connected)
	        {
	            throw new Exception("Could not connect to Insteon Controller");
	        }
            
	        return base.Start(urlBase);
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
