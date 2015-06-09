using Funq;
using Insteon.Daemon.Common.Service;
using Insteon.Network;
using ServiceStack;
using ServiceStack.Api.Swagger;
using ServiceStack.Text;

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

	    public override void Configure(Container container)
		{
            Plugins.RemoveAll(x => x is MetadataFeature); 
            //Plugins.Add(new RazorFormat());
	        JsConfig.EmitCamelCaseNames = true;
            Plugins.Add(new SwaggerFeature());
            var manager = new InsteonManager(insteonSource);
            container.Register(manager);
	        container.Register(new SmartThingsSettings());
            
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
