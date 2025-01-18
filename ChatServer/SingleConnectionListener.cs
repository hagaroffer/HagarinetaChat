using ChatCommon;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;

namespace ChatServer
{
    public class SingleConnectionListener
    {
        private Dictionary<string, TcpClient> _clientsDictionary;
        private object _clientsDictionaryLock;
        private MessageSender _messageSender;
        private Queue<ChatMessage> _messagesQueue;
        private object _messagesQueueLock;
        private EventWaitHandle _eventWaitHandle;
        TcpClient _client;

        public SingleConnectionListener(Dictionary<string, TcpClient> clientsDictionary, object clientsDictionaryLock,
            Queue<ChatMessage> messagesQueue, object messagesQueueLock, EventWaitHandle eventWaitHandle, TcpClient client)
        {
            _clientsDictionary = clientsDictionary;
            _clientsDictionaryLock = clientsDictionaryLock;
            _messageSender = new MessageSender(_clientsDictionary, _clientsDictionaryLock);
            _messagesQueue = messagesQueue;
            _messagesQueueLock = messagesQueueLock;
            _eventWaitHandle = eventWaitHandle;
            _client = client;
        }

        public void HandleNewConnection(object o)
        {
            try
            {
                NetworkStream stream = _client.GetStream();

                var userName = CreateNewClient(stream);

                while (_client.Connected)
                {
                    try
                    {
                        ListenToNewCreatedConnection(stream, userName);
                    }
                    catch (IOException)
                    {
                        Console.WriteLine($"Client {userName} disconnected.");
                        HandleClientDisconnect(userName, _client, stream);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "ListenToNewCreatedConnection"));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "HandleNewConnection"));
            }
        }

        private void ListenToNewCreatedConnection(NetworkStream stream, string userName)
        {
            try
            {
                var message = ReadMessage(stream);

                if (message == null)
                {
                    throw new IOException("Message is null, client might have disconnected.");
                }

                message.SourceUsername = userName;

                if (message.MessageType == MessageType.Disconnect)
                {
                    Console.WriteLine($"User {userName} requested disconnect.");
                    HandleClientDisconnect(userName, _client, stream);
                    return;
                }

                lock (_messagesQueueLock)
                {
                    _messagesQueue.Enqueue(message);
                    _eventWaitHandle.Set();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "ListenToNewCreatedConnection"));
                throw;
            }
        }


        private void HandleClientDisconnect(string userName, TcpClient client, NetworkStream stream)
        {
            try
            {
                lock (_clientsDictionaryLock)
                {
                    if (_clientsDictionary.ContainsKey(userName))
                    {
                        _clientsDictionary.Remove(userName);
                        Console.WriteLine($"Client {userName} removed from the dictionary.");
                    }
                }

                SendConnectedClients();

                stream?.Close();
                client?.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "HandleClientDisconnect"));
            }
        }

        private string CreateNewClient(NetworkStream stream)
        {
            try
            {
                var message = ReadMessage(stream);
                if (message == null)
                {
                    throw new Exception("Received message is null, cannot proceed with client creation.");
                }

                var userName = message.Message.ToLower();

                if (IsUsernameUnique(userName))
                {
                    AddNewUserToDictionary(userName, _client);
                    SendConnectionBroadcastMessage(userName);
                    SendConnectedClients();

                    return userName;
                }
                else
                {
                    throw new Exception(string.Format("Username {0} already exists", userName));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "CreateNewClient"));
                throw;
            }
        }

        private ChatMessage ReadMessage(NetworkStream stream)
        {
            try
            {
                return ChatMessageTranfer.ReadMessage(stream);
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "ReadMessage"));
                return null;
            }
        }

        private bool IsUsernameUnique(string userName)
        {
            bool isUsernameAlreadyExists;
            lock (_clientsDictionaryLock)
            {
                isUsernameAlreadyExists = _clientsDictionary.ContainsKey(userName);
            }

            return !isUsernameAlreadyExists;
        }

        private void AddNewUserToDictionary(string userName, TcpClient client)
        {
            try
            {
                lock (_clientsDictionaryLock)
                {
                    _clientsDictionary.Add(userName, client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "AddNewUserToDictionary"));
                throw;
            }
        }

        private void SendConnectionBroadcastMessage(string userName)
        {
            try
            {
                ChatMessage broadcastConnectionMessage =
                        new ChatMessage(MessageType.Broadcast, string.Empty, string.Format("User {0} has joined", userName));
                broadcastConnectionMessage.SourceUsername = "Server";

                _messageSender.SendBroadcastMessage(broadcastConnectionMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "SendConnectionBroadcastMessage"));
            }
        }

        private void SendConnectedClients()
        {
            try
            {
                string usersString = string.Join(" ", _clientsDictionary.Keys.ToArray());
                ChatMessage broadcastConnectionMessage =
                        new ChatMessage(MessageType.ConnectedUsers, string.Empty, string.Format(usersString));
                broadcastConnectionMessage.SourceUsername = "Server";

                _messageSender.SendBroadcastMessage(broadcastConnectionMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "SendConnectedClients"));
            }
        }
    }
}
