using ChatClient;
using ChatCommon;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ChatClient
{
    public class ChatClient : IChatClient
    {
        private static readonly ChatClient _instance = new ChatClient();
        public static List<string> _connectedUsersList;
        public static object _connectedUsersListLock;

        public static NetworkStream _stream;
        private TcpClient _client;
        private string _myUsername = string.Empty;


        private ChatClient()
        {
            _connectedUsersListLock = new object();

            lock (_connectedUsersListLock)
            {
                _connectedUsersList = new List<string>();
            }
        }

        public static ChatClient Instance
        {
            get { return _instance; }
        }       

        public void StartClient(string myUsername)
        {
            _myUsername = myUsername;

            try
            {
                InitTcpListener();

                InitListenerThread();

                SendConnectionMessage();

            }
            catch (Exception ex)
            {
                if (_stream != null)
                {
                    _stream.Close();
                }
                if (_client != null)
                {
                    _client.Close();
                }
                return;
            }            
        }

        public void SendPrivateMessage(string destinationUsername, string message, string filePath)
        {
            try
            {
                var chatMessage = new ChatMessage(MessageType.Private, destinationUsername, message);

                HandleFileInMessage(chatMessage, filePath);

                SendMessage(chatMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(CreateExceptionMsg(ex, "SendPrivateMessage"));
            }
        }

        public void SendBroadcastMessage(string message, string filePath)
        {
            try
            {
                var chatMessage = new ChatMessage(MessageType.Broadcast, string.Empty, message);
                
                HandleFileInMessage(chatMessage, filePath);

                SendMessage(chatMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(CreateExceptionMsg(ex, "SendBroadcastMessage"));
            }
        }

        public List<string> GetConnectedClients()
        {
            try
            {
                lock (_connectedUsersListLock)
                {
                    return _connectedUsersList;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(CreateExceptionMsg(ex, "GetConnectedClients"));
                return null;
            }
        }


        private void InitTcpListener()
        {
            string ipAddress = Configurations.IpAddress;
            int port = int.Parse(Configurations.Port);
            
            try
            {                
                _client = new TcpClient();
                _client.Connect(IPAddress.Parse(ipAddress), port);
                _stream = _client.GetStream();
            }
            catch (Exception ex) 
            {
                Console.WriteLine(CreateExceptionMsg(ex, "InitTcpListener"));
                throw;
            }
        }

        private void InitListenerThread()
        {
            try
            {
                var clientListener = new ClientListener(_connectedUsersList, _connectedUsersListLock, _stream);
                Thread listenerThread = new Thread(clientListener.ListenToIncomingMessages);
                listenerThread.Start();
            }
            catch (Exception ex) 
            {
                Console.WriteLine(CreateExceptionMsg(ex, "InitTcpLisInitListenerThreadtener"));
                throw;
            }
        }

        private void SendConnectionMessage()
        {
            try
            {
                ChatMessage message = new ChatMessage(MessageType.Private, "", _myUsername);
                SendMessage(message);
            }
            catch (Exception ex) 
            {
                Console.WriteLine(CreateExceptionMsg(ex, "SendConnectionMessage"));
                throw;
            }
        }

        private void SendMessage(ChatMessage chatMessage)
        {
            var convertedMessageBuffer = ChatMessageTranfer.PrepareMessageToBeSent(chatMessage);
            ChatMessageTranfer.SendMessage(convertedMessageBuffer, _stream);
        }

        private async Task HandleFileInMessage(ChatMessage chatMessage, string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                long length = new FileInfo(filePath).Length;
                var fileContent = await File.ReadAllBytesAsync(filePath);
                var file = new ChatFile() { FileNameWithExtension = Path.GetFileName(filePath), Content = fileContent, FileLength = length };
                chatMessage.File = file;
            }
        }

        private string CreateExceptionMsg(Exception ex, string methodName)
        {
            string errorMsg = string.Format(@"Failure in {0}.{1}Error: {2}. {3}StackTrace: {4}", methodName, Environment.NewLine, ex.Message, Environment.NewLine, ex.StackTrace);
            return errorMsg;
        }

    }
}
