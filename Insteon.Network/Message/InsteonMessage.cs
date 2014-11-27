using System.Collections.Generic;
using System.Text;
using Insteon.Network.Enum;
using Insteon.Network.Helpers;

namespace Insteon.Network.Message
{
    // Represents a structured view of an INSTEON message, as produced by the message processor.
    internal class InsteonMessage
    {
        public InsteonMessage(int messageId, InsteonMessageType messageType, Dictionary<PropertyKey, int> properties)
        {
            MessageId = messageId;
            MessageType = messageType;
            Properties = properties;
            Key = ToString("Key");
        }

        public string Key { get; private set; }
        public int MessageId { get; private set; }
        public InsteonMessageType MessageType { get; private set; }
        public Dictionary<PropertyKey, int> Properties { get; private set; }

        public override string ToString()
        {
            return MessageType.ToString();
        }

        public string ToString(string format)
        {
            if (format == "Log")
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("  MessageId={0:X2}", MessageId);
                sb.AppendLine();
                sb.AppendFormat("  MessageType={0}", MessageType);
                sb.Append(Utilities.FormatProperties(Properties, true, false));
                return sb.ToString();
            }
            if (format == "Key")
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("MessageId={0:X2} ", MessageId);
                sb.Append(Utilities.FormatProperties(Properties, false, true));
                return sb.ToString();
            }
            return ToString();
        }
    }
}