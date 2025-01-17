using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatCommon
{
    public class ChatMessage
    {
        public MessageType MessageType { get; set; }
        public string SourceUsername { get; set; }
        public string DestinationUsername { get; set; }
        public string Message { get; set; }
        public ChatFile File { get; set; }

        public ChatMessage(MessageType messageType, string destinationUsername, string message)
        {
            MessageType = messageType;
            DestinationUsername = destinationUsername;
            Message = message;
        }
    }

    public enum MessageType
    {
        Private = 0,
        Broadcast = 1,
        ConnectedUsers = 2
    }

    public class ChatFile
    {
        public string FileNameWithExtension { get; set; }

        public byte[] Content { get; set; }

        public long FileLength { get; set; }

    }
}
