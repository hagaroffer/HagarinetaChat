using ChatCommon;
using System;
using System.Net.Sockets;
using System.Text.Json;

namespace ChatClient
{
    public class ClientListener
    {
        //public List<string> _connectedUsersList;
        //private object _connectedUsersListLock;
        //private NetworkStream _stream;
        //private TcpClient _client;

        public ClientListener(List<string> connectedUsersList, object connectedUsersListLock,
            /*TcpClient client,*/ NetworkStream stream)
        {
            //_connectedUsersList = connectedUsersList;
            //_connectedUsersListLock = connectedUsersListLock;
            //_client = client;
            //_stream = stream;
        }

        public void ListenToIncomingMessages()
        {
            try
            {
                while (true)
                {
                    var message = ReadMessage();
                    ProcessMessage(message);
                }
            }
            catch (Exception e)
            {

            }
        }

        private ChatMessage ReadMessage()
        {
            return ChatMessageTranfer.ReadMessage(ChatClient._stream);            
        }

        private void ProcessMessage(ChatMessage message)
        {
            if (message.MessageType == MessageType.ConnectedUsers)
            {
                lock (ChatClient._connectedUsersListLock)
                {
                    var a = message.Message.Split(" ").ToList();
                    ChatClient._connectedUsersList = message.Message.Split(" ").ToList();
                }
                return;
            }

            string messageToShow = string.Empty;
            if (message.MessageType == MessageType.Private)
            {
                messageToShow += string.Format("Private message ");
            }
            else if (message.MessageType == MessageType.Broadcast)
            {
                messageToShow += string.Format("Broadcast message ");
            }

            messageToShow += string.Format("from {0}: {1}", message.SourceUsername, message.Message);

            if (message.File != null)
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), message.File.FileNameWithExtension);
                File.WriteAllBytes(filePath, message.File.Content);

                messageToShow += string.Format(", and a file: {0}", filePath);
            }

            Console.WriteLine(messageToShow);
        }
    }
}
