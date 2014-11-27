namespace Insteon.Network.Commands
{
    internal enum InsteonModemSerialCommandSend : byte
    {
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