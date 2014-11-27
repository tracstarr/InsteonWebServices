namespace Insteon.Network.Device
{
    /// <summary>
    /// Provides notification when an INSTEON device has reported a change in status.
    /// </summary>
    public class InsteonDeviceStatusChangedEventArgs
    {
        internal InsteonDeviceStatusChangedEventArgs(InsteonDevice device, InsteonDeviceStatus status)
        {
            Device = device;
            DeviceStatus = status;
        }

        /// <summary>
        /// Gets an object that represents the INSTEON device that has changed status.
        /// </summary>
        public InsteonDevice Device { get; private set; }

        /// <summary>
        /// Gets a value that indicates the status of the INSTEON device.
        /// </summary>
        public InsteonDeviceStatus DeviceStatus { get; private set; }
    }

    /// <summary>
    /// Represents the method that handles a status changed event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="data">An object that contains the event data.</param>
    public delegate void InsteonDeviceStatusChangedEventHandler(object sender, InsteonDeviceStatusChangedEventArgs data);
}