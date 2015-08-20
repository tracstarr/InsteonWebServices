namespace Insteon.Network.Device
{
    public enum InsteonDeviceStatus
    {
        Unknown ,
        On,
        Off,
        FastOn,
        FastOff,
        Brighten,
        Dim,
        LowBattery,
        LightDetected,               // motion sensor
        DryDetected,
        WetDetected,
        Heartbeat,
        SensorTriggerOn,
        SensorTriggerOff
    }
}