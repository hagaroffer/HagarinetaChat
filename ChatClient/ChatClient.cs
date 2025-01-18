using ChatClient;
using ChatCommon;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ChatClient
{
    public class ChatClient : IChatClient
    {
        private static readonly ChatClient _instance = new ChatClient();

        private NetworkStream _stream;
        private TcpClient _client;
        private string _myUsername = string.Empty;


        private ChatClient()
        {
            RegisterAppExistEvents();
        }


        public static ChatClient Instance
        {
            get { return _instance; }
        }       

        public void StartClient(string myUsername)
        {
            if (!IsValidUsername(myUsername)) return;

            _myUsername = myUsername;

            try
            {
                InitTcpListener();

                InitListenerThread();

                SendConnectionMessage();
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "StartClient"));
                Disconnect();
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
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "SendPrivateMessage"));
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
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "SendBroadcastMessage"));
            }
        }

        public List<string> GetConnectedClients()
        {
            try
            {
                lock (SharedResource.ConnectedUsersListLock)
                {
                    return SharedResource.ConnectedUsersList;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "GetConnectedClients"));
                return null;
            }
        }

        public void Disconnect()
        {
            try
            {
                ChatMessage disconnectMessage = new ChatMessage(MessageType.Disconnect, string.Empty, _myUsername);
                SendMessage(disconnectMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "Disconnect"));
            }
            finally
            {
                _stream?.Close();
                _client?.Close();
                Console.WriteLine("Disconnected from server successfully.");
            }
        }

        private void InitTcpListener()
        {
            try
            {
                string ipAddress = Configurations.IpAddress;
                int port = int.Parse(Configurations.Port);

                _client = new TcpClient();
                _client.Connect(IPAddress.Parse(ipAddress), port);
                _stream = _client.GetStream();

                Task.Run(async () =>
                {
                    while (_client.Connected)
                    {
                        try
                        {
                            if (_stream == null)
                            {
                                throw new Exception("Disconnected from server.");
                            }

                            await Task.Delay(1000);
                        }
                        catch
                        {
                            Console.WriteLine("Lost connection to the server.");
                            Disconnect();
                            break;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "InitTcpListener"));
                throw;
            }
        }

        private void InitListenerThread()
        {
            try
            {
                var clientListener = new ClientListener(_stream);
                Thread listenerThread = new Thread(clientListener.ListenToIncomingMessages);
                listenerThread.Start();
            }
            catch (Exception ex) 
            {
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "InitTcpLisInitListenerThreadtener"));
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
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "SendConnectionMessage"));
                throw;
            }
        }

        private void SendMessage(ChatMessage chatMessage)
        {
            var convertedMessageBuffer = ChatMessageTranfer.PrepareMessageToBeSent(chatMessage);
            ChatMessageTranfer.SendMessage(convertedMessageBuffer, _stream);
        }

        private void HandleFileInMessage(ChatMessage chatMessage, string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                var file = new ChatFile() { FileNameWithExtension = Path.GetFileName(filePath), Content = File.ReadAllBytes(filePath)};
                chatMessage.File = file;
            }
        }

        private void RegisterAppExistEvents()
        {
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                Disconnect();
            };

            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("Application is exiting...");
                Disconnect();

                e.Cancel = true;
            };
        }

        private bool IsValidUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return false;
            }

            if (username.Length < 3 || username.Length > 30)
            {
                return false;
            }

            // Regular expression to check if username contains only letters, digits, and underscores
            string pattern = @"^[a-zA-Z0-9_]+$";
            if (!Regex.IsMatch(username, pattern))
            {
                return false;
            }

            if (username.Trim() != username)
            {
                return false;
            }

            string[] reservedWords = { "admin", "root", "system" };
            if (Array.Exists(reservedWords, word => word.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            return true;
        }

    }
}
