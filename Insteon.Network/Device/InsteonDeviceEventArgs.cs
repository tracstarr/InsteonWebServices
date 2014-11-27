using System;

namespace Insteon.Network.Device
{
    /// <summary>
    /// Provides notification when there is a change of status in an INSTEON device.
    /// </summary>
    public class InsteonDeviceEventArgs : EventArgs
    {
        internal InsteonDeviceEventArgs(InsteonDevice device)
        {
            Device = device;
        }

        /// <summary>
        /// Gets an object that represents the INSTEON device that has changed status.
        /// </summary>
        public InsteonDevice Device { get; private set; }
    }

    /// <summary>
    /// Represents the method that handles a status changed event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="data">An object that contains the event data.</param>
    public delegate void InsteonDeviceEventHandler(object sender, InsteonDeviceEventArgs data);
}