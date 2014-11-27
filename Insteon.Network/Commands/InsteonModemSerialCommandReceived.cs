namespace Insteon.Network.Commands
{
    internal enum InsteonModemSerialCommandReceived : byte
    {
        // receive only
        StandardMessage = 0x50,
        ExtendedMessage = 0x51,
        DeviceLinkingCompleted = 0x53,
        DeviceLinkRecord = 0x57,
        DeviceCleanup = 0x58,

        // receive after a sent message
        GetImInfo = 0x60
    }
}