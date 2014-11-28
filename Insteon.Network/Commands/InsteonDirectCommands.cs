namespace Insteon.Network.Commands
{
    /// <summary>
    /// Represents the set of Standard commands that can be sent to an INSTEON device.
    /// </summary>
    internal enum InsteonDirectCommands
    {
        /// <summary>
        /// For devices after 2/1/7 to aid in looking up product type as devcat and subcat are no longer viable. Responds with ES 0x0300
        /// </summary>
        ProductDataRequest = 0x03,

        /// <summary>
        /// Commands a device to enter linking mode.
        /// </summary>
        EnterLinkingMode = 0x09,

        /// <summary>
        /// Commands a device to enter unlinking mode.
        /// </summary>
        EnterUnlinkingMode = 0x0A,

        /// <summary>
        /// Queries a device for its INSTEON identity (DevCat, SubCat, FirmwareVersion).
        /// </summary>
        IDRequest = 0x10,

        /// <summary>
        /// Commands a device to turn on. If the target device is a dimmer, a second parameter determines the brightness level from 0 (least brightness) to 255 (full brightness).
        /// </summary>
        On = 0x11,

        /// <summary>
        /// Commands a device to turn on immediately, causing dimmer devices to bypass their slow ramp up behavior.
        /// </summary>
        FastOn = 0x12,

        /// <summary>
        /// Commands a device to turn off.
        /// </summary>
        Off = 0x13,

        /// <summary>
        /// Commands a device to turn off immediately, causing dimmer devices to bypass their slow ramp down behavior.
        /// </summary>
        FastOff = 0x14,

        /// <summary>
        /// Commands a dimmer device to brighten incrementally by one step, where each step is a small percentage of the full on level for the device.
        /// </summary>
        Brighten = 0x15,

        /// <summary>
        /// Commands a dimmer device to dim incrementally by one step, where each step is a small percentage of the full on level for the device.
        /// </summary>
        Dim = 0x16,

        /// <summary>
        /// Commands a dimmer device to slowly begin dimming. Dimming will continue until a stop dimming command is received or a limit is reached.
        /// A parameter value must be specified to determine the direction (0=dim, 1=brighten).
        /// </summary>
        StartDimming = 0x17,

        /// <summary>
        /// Commands a dimmer device to stop the dimming process initiated by a previous start dimming command.
        /// </summary>
        StopDimming = 0x18,

        /// <summary>
        /// Gets the on-level for the device.
        /// </summary>
        StatusRequest = 0x19
    }

}