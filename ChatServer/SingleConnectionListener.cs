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

                var userName = CreateNewClient(_client, stream);
                if (string.IsNullOrEmpty(userName))
                {
                    //??????????????????????
                    //return false;
                }

                while (true)
                {
                    ListenToNewCreatedConnection(_client, stream, userName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(CreateExceptionMsg(ex, "HandleNewConnection"));
                return;
            }
        }

        private void ListenToNewCreatedConnection(TcpClient client, NetworkStream stream, string userName)
        {
            var message = ReadMessage(stream);
            message.SourceUsername = userName;

            if (message.MessageType == MessageType.Disconnect)
            {
                HandleClientDisconnect(userName, client, stream);
                return;
            }

            lock (_messagesQueueLock)
            {
                _messagesQueue.Enqueue(message);
                _eventWaitHandle.Set();
            }
        }

        private void HandleClientDisconnect(string userName, TcpClient client, NetworkStream stream)
        {
            try
            {
                lock (_clientsDictionaryLock)
                {
                    _clientsDictionary.Remove(userName);
                }

                stream?.Close();
                _client?.Close();
            }
            catch (Exception ex)
            {
            }
        }

        private string CreateNewClient(TcpClient client, NetworkStream stream)
        {
            try
            {
                var message = ReadMessage(stream);
                if (message == null)
                {
                    return string.Empty;
                }

                var userName = message.Message.ToLower();

                if (IsUsernameUnique(userName))
                {
                    AddNewUserToDictionary(userName, client);
                    SendConnectionBroadcastMessage(userName);
                    SendConnectedClients();

                    return userName;
                }
                else
                {
                    //LOG!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    throw new Exception(string.Format("Username {0} already exists", userName));
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(CreateExceptionMsg(ex, "CreateNewClient"));
                throw;
            }
        }

        private ChatMessage ReadMessage(NetworkStream stream)
        {
            return ChatMessageTranfer.ReadMessage(stream).Result;
        }

        private bool IsUsernameUnique(string userName)
        {
            var isUsernameAlreadyExists = false;

            lock (_clientsDictionaryLock)
            {
                isUsernameAlreadyExists = _clientsDictionary.ContainsKey(userName);
            }

            if (isUsernameAlreadyExists) return false;
            return true;
        }

        private void AddNewUserToDictionary(string userName, TcpClient client)
        {
            lock (_clientsDictionaryLock)
            {
                _clientsDictionary.Add(userName, client);
            }
        }

        private void SendConnectionBroadcastMessage(string userName)
        {
            ChatMessage broadcastConnectionMessage =
                    new ChatMessage(MessageType.Broadcast, string.Empty, string.Format("User {0} has joined", userName));
            broadcastConnectionMessage.SourceUsername = "Server";

            _messageSender.SendBroadcastMessage(broadcastConnectionMessage);
        }

        private void SendConnectedClients()
        {
            string usersString = string.Join(" ", _clientsDictionary.Keys.ToArray());
            ChatMessage broadcastConnectionMessage =
                    new ChatMessage(MessageType.ConnectedUsers, string.Empty, string.Format(usersString));
            broadcastConnectionMessage.SourceUsername = "Server";

            _messageSender.SendBroadcastMessage(broadcastConnectionMessage);
        }

        //private void ListenToClientMessages(TcpClient client, NetworkStream stream, string userName)
        //{
        //    var message = ReadMessage(client, stream);
        //    message.SourceUsername = userName;

        //    lock (_messagesQueueLock)
        //    {
        //        _messagesQueue.Enqueue(message);
        //        _eventWaitHandle.Set();
        //    }
        //}

        private string CreateExceptionMsg(Exception ex, string methodName)
        {
            string errorMsg = string.Format(@"Failure in {0}.{1}Error: {2}. {3}StackTrace: {4}", methodName, Environment.NewLine, ex.Message, Environment.NewLine, ex.StackTrace);
            return errorMsg;
        }
    }
}
