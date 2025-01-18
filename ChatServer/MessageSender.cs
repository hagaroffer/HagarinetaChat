using ChatCommon;
using System.Net.Sockets;

namespace ChatServer
{
    public class MessageSender
    {
        public MessageSender()
        {
        }

        public void SendBroadcastMessage(ChatMessage chatMessage)
        {
            var convertedMessageBuffer = ChatMessageTranfer.PrepareMessageToBeSent(chatMessage);
            Dictionary<string, TcpClient>.ValueCollection allClients;

            lock (SharedResource.ClientsDictionaryLock)
            {
                allClients = SharedResource.ClientsDictionary.Values;
            }

            Parallel.ForEach(allClients, (client) =>
            {
                var stream = client.GetStream();
                ChatMessageTranfer.SendMessage(convertedMessageBuffer, stream);
            });
        }

        public bool SendPrivateMessage(ChatMessage chatMessage)
        {
            TcpClient client;
            bool isDestinationUserExists = false;

            lock (SharedResource.ClientsDictionaryLock)
            {
                isDestinationUserExists = SharedResource.ClientsDictionary.TryGetValue(chatMessage.DestinationUsername.ToLower(), out client);
            }

            if (!isDestinationUserExists)
            {
                Console.WriteLine(string.Format("Username {0} doesn't exists", chatMessage.DestinationUsername));
                return false;
            }

            var stream = client.GetStream();
            var convertedMessageBuffer = ChatMessageTranfer.PrepareMessageToBeSent(chatMessage);
            ChatMessageTranfer.SendMessage(convertedMessageBuffer, stream);
            return true;
        }
    }
}
