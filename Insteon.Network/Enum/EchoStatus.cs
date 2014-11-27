namespace Insteon.Network.Enum
{
    // Represents the status of the serial message echoed from the controller.
    internal enum EchoStatus
    {
        None = 0, // No response
        Unknown = 1, // Unknown acknowledgment response (i.e. not a 0x06 or a 0x15)
        ACK = 0x06, // Acknowledge (OK)
        NAK = 0x15 // Negative Acknowledge (ERROR)
    }
}