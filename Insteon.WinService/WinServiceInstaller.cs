using System.ComponentModel;
using System.Configuration.Install;

namespace Insteon.Daemon
{
	[RunInstaller(true)]
	public partial class WinServiceInstaller : Installer
	{
		public WinServiceInstaller()
		{
			InitializeComponent();
		}		
	}
}
