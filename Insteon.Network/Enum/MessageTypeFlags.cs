namespace Insteon.Network.Enum
{
    internal enum MessageTypeFlags: byte
    {
        Ack = 0x20,
        AllLink = 0x40,
        Broadcast = 0x80,
        Extended = 0x10,
        HopsLeft = 0x0C,
        MaxHops = 0x03
    }
}