namespace Insteon.Network.Commands
{
    internal enum InsteonModemConfigurationFlags : byte
    {
        Reserved = 0x0,
        DisableDeadman = 0x10,
        DisableAutoLed = 0x20,
        EnterMonitorMode = 0x40,
        DisableAutoLinking = 0x80
    }
}