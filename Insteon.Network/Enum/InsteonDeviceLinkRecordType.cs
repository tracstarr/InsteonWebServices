namespace Insteon.Network.Enum
{
    /// <summary>
    /// Represents the type of link record.
    /// </summary>
    public enum InsteonDeviceLinkRecordType
    {
        /// <summary>
        /// Indicates the link record is empty, or not in use.
        /// </summary>
        Empty = 0,

        /// <summary>
        /// Indicates the link record is a controller link, allowing messages to be sent to the linked device.
        /// </summary>
        Controller,

        /// <summary>
        /// Indicates the link record is a responder link, allowing messages to be received from the linked device.
        /// </summary>
        Responder
    }
}