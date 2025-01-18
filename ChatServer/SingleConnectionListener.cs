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
        NetworkStream _stream;

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
                _stream = _client.GetStream();

                string username = CreateNewClient();

                while (_client.Connected)
                {
                    try
                    {
                        ListenToNewCreatedConnection(username);
                    }
                    catch (IOException)
                    {
                        Console.WriteLine($"Client {username} disconnected.");
                        HandleClientDisconnect(username);
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

        private void ListenToNewCreatedConnection(string username)
        {
            try
            {
                var message = ReadMessage();

                if (message == null)
                {
                    throw new IOException("Message is null, client might have disconnected.");
                }

                message.SourceUsername = username;

                if (message.MessageType == MessageType.Disconnect)
                {
                    Console.WriteLine($"User {username} requested disconnect.");
                    HandleClientDisconnect(username);
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


        private void HandleClientDisconnect(string username)
        {
            try
            {
                lock (_clientsDictionaryLock)
                {
                    if (_clientsDictionary.ContainsKey(username))
                    {
                        _clientsDictionary.Remove(username);
                        Console.WriteLine($"Client {username} removed from the dictionary.");
                    }
                }

                SendConnectedClients();

                _stream?.Close();
                _client?.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "HandleClientDisconnect"));
            }
        }

        private string CreateNewClient()
        {
            try
            {
                var message = ReadMessage();
                if (message == null)
                {
                    throw new Exception("Received message is null, cannot proceed with client creation.");
                }

                var username = message.Message.ToLower();

                if (IsUsernameUnique(username))
                {
                    AddNewUserToDictionary(username);
                    SendConnectionBroadcastMessage(username);
                    SendConnectedClients();

                    return username;
                }
                else
                {
                    throw new Exception(string.Format("Username {0} already exists", username));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "CreateNewClient"));
                throw;
            }
        }

        private ChatMessage ReadMessage()
        {
            try
            {
                return ChatMessageTranfer.ReadMessage(_stream);
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "ReadMessage"));
                return null;
            }
        }

        private bool IsUsernameUnique(string username)
        {
            bool isUsernameAlreadyExists;
            lock (_clientsDictionaryLock)
            {
                isUsernameAlreadyExists = _clientsDictionary.ContainsKey(username);
            }

            return !isUsernameAlreadyExists;
        }

        private void AddNewUserToDictionary(string username)
        {
            try
            {
                lock (_clientsDictionaryLock)
                {
                    _clientsDictionary.Add(username, _client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "AddNewUserToDictionary"));
                throw;
            }
        }

        private void SendConnectionBroadcastMessage(string username)
        {
            try
            {
                ChatMessage broadcastConnectionMessage =
                        new ChatMessage(MessageType.Broadcast, string.Empty, string.Format("User {0} has joined", username));
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
