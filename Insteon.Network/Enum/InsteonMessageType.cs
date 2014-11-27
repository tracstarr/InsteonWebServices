namespace Insteon.Network.Enum
{
    // Identfies the type of INSTEON message.
    internal enum InsteonMessageType
    {
        Other = 0,
        Ack,
        DeviceLink,
        DeviceLinkCleanup,
        DeviceLinkRecord,
        FastOffBroadcast,
        FastOffCleanup,
        FastOnBroadcast,
        FastOnCleanup,
        GetInsteonModemInfo,
        IncrementBeginBroadcast,
        IncrementEndBroadcast,
        OffBroadcast,
        OffCleanup,
        OnBroadcast,
        OnCleanup,
        SetButtonPressed,
        SuccessBroadcast
    }
}