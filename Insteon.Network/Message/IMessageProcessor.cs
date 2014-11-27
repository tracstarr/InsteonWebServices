using Insteon.Network.Enum;

namespace Insteon.Network.Message
{
    interface IMessageProcessor
    {
        bool ProcessMessage(byte[] data, int offset, out int count);
        bool ProcessEcho(byte[] data, int offset, out int count);
        void SetEchoStatus(EchoStatus status);
    }
}