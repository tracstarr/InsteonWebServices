using System.ComponentModel;
using ServiceStack;

namespace Insteon.Daemon.Common.Request
{
    [Description("Get all current Insteon controller links.")]
    [Route("/devices")]
    public class GetDevices { }
}