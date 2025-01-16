using ChatCommon;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ChatServer
{
    public class MessageSender
    {
        private Dictionary<string, TcpClient> _clientsDictionary;
        private Object _clientsDictionaryLock;

        public MessageSender(Dictionary<string, TcpClient> clientsDictionary, Object clientsDictionaryLock)
        {
            _clientsDictionary = clientsDictionary;
            _clientsDictionaryLock = clientsDictionaryLock;
        }

        public void SendBroadcastMessage(ChatMessage chatMessage)
        {
            var convertedMessageBuffer = ChatMessageTranfer.PrepareMessageToBeSent(chatMessage);
            Dictionary<string, TcpClient>.ValueCollection allClients;

            lock (_clientsDictionaryLock)
            {
                allClients = _clientsDictionary.Values;
            }

            Parallel.ForEach(allClients, (client) =>
            {
                var stream = client.GetStream();
                ChatMessageTranfer.SendMessage(convertedMessageBuffer, stream);
                //stream.Write(convertedMessageBuffer, 0, convertedMessageBuffer.Length);
            });
        }

        public bool SendPrivateMessage(ChatMessage chatMessage)
        {
            TcpClient client;
            bool isDestinationUserExists = false;

            lock (_clientsDictionaryLock)
            {
                isDestinationUserExists = _clientsDictionary.TryGetValue(chatMessage.DestinationUsername.ToLower(), out client);
            }

            if (!isDestinationUserExists)
            {
                Console.WriteLine(string.Format("Username {0} doesn't exists", chatMessage.DestinationUsername));
                return false;
                //Send error message to sender?
            }

            var stream = client.GetStream();
            var convertedMessageBuffer = ChatMessageTranfer.PrepareMessageToBeSent(chatMessage);
            ChatMessageTranfer.SendMessage(convertedMessageBuffer, stream);
            return true;
        }
    }
}
