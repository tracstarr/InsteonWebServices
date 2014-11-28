namespace Insteon.Network.Commands
{
    internal enum InsteonModemSerialCommand : byte
    {
        // receive only
        StandardMessage = 0x50,
        ExtendedMessage = 0x51,
        X10Received = 0x52,
        DeviceLinkingCompleted = 0x53,
        ButtonEventReport = 0x54,
        UserResetDetected = 0x55,
        AllLinkCleanupFailure = 0x56,
        DeviceLinkRecord = 0x57,
        DeviceCleanup = 0x58,

        // send and receive
        GetImInfo = 0x60,
        SendAllLinkCommand = 0x61,
        StandardOrExtendedMessage = 0x62,
        GetNextDeviceLinkRecord = 0x6A,
        SetConfiguration = 0x6B,
        StartAllLink = 0x64,
        CancelAllLink = 0x65,
        GetFirstAllLinkRecord = 0x69

    }
}