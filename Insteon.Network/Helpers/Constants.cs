namespace Insteon.Network.Helpers
{
    internal static class Constants
    {
        // note: all times are in milliseconds
        public const int deviceAckTimeout = 4000; // timeout to receive expected ACK reply from a device
        public const int deviceCommandRetries = 3; // number of times to retry sending a device command (if no ACK was received from the device within deviceAckTimeout time)
        public const int deviceCommandWaitTime = 200; // minimum time between to wait between sending two different device commands
        public const int duplicateInterval = 1000; // interval within which to treat multiple matching messages received from a device as duplicates, where matching is defined as all message bytes are the same except for the hop-count bits
        public const int echoTimeout = 1000; // time to wait for echo response after sending data
        public const int negotiateRetries = 5; // number of times to retry negotiating with a serial device
        public const int openTimeout = 1000; // time to wait for initial response after opening port
        public const int readDataRetries = 5; // number of times to retry reading data from the serial port
        public const int readDataRetryTime = 5; // time to wait between retries when reading data from the serial port
        public const int readDataTimeout = 1000; // maximum amount of time to wait for additional data when reading data from the serial port
        public const int sendMessageRetries = 5; // number of times to retry sending a message
        public const int sendMessageWaitTime = 100; // amount of time to wait after failing to send a message before retrying, note time is multiplied on each retry 1x, 2x, 3x, ... up to sendMessageRetries times
        public const int sendReceiveTimeout = 2000; // timeout to receive expected reply from a blocking send/receive message such as a Controller.GetLinks or a Device.GetOnLevel
        public const int webRequestTimeout = 5000; // timeout for web requests to smartlinc.smarthome.com and for accessing SmartLinc devices over the local network
        public const byte MessageStartByte = 0x02; // todo: is this only for serial commands?
        public const byte MessageEndByte = 0x06;// todo: is this only for serial commands?
        public const byte ResponderCmd = 0x01;
        public const byte ControllerCmd = 0x02;
        public const byte SuccessCmd = 0x06;
    }
}