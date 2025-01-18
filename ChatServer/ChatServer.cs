using ChatCommon;
using System.Net;
using System.Net.Sockets;

namespace ChatServer
{
    public class ChatServer
    {
        private TcpListener _tcpListener;
        private Dictionary<string, TcpClient> _clientsDictionary;
        private object _clientsDictionaryLock;
        private Queue<ChatMessage> _messagesQueue;
        private object _messagesQueueLock;
        private EventWaitHandle _eventWaitHandle;

        public void InitServer()
        {
            try
            {
                InitVariables();
                InitTcpListener();
                InitQueueMessagesSenderThread();

                Console.WriteLine("Chat server is running");
                ListenToNewConnectionRequests();
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "InitServer"));
                CleanupServer();
            }
        }

        private void InitVariables()
        {
            try
            {
                _clientsDictionary = new Dictionary<string, TcpClient>();
                _clientsDictionaryLock = new object();
                _messagesQueue = new Queue<ChatMessage>();
                _messagesQueueLock = new object();
                _eventWaitHandle = new AutoResetEvent(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "InitVariables"));
                throw;
            }
        }

        private void InitTcpListener()
        {
            try
            {
                string ipAddress = Configurations.IpAddress;
                int port = int.Parse(Configurations.Port);

                _tcpListener = new TcpListener(IPAddress.Parse(ipAddress), port);
                _tcpListener.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "InitTcpListener"));
                throw;
            }
        }

        private void InitQueueMessagesSenderThread()
        {
            try
            {
                Thread senderThread = new Thread(new QueueMessagesSender(_messagesQueue, _messagesQueueLock, _eventWaitHandle, _clientsDictionary, _clientsDictionaryLock).HandleMessagesQueue);
                senderThread.IsBackground = true;
                senderThread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "InitQueueMessagesSenderThread"));
                throw;
            }
        }

        private void ListenToNewConnectionRequests()
        {
            try
            {
                var connectionsListener = new NewConnectionsListener(
                    _tcpListener,
                    _clientsDictionary,
                    _clientsDictionaryLock,
                    _messagesQueue,
                    _messagesQueueLock,
                    _eventWaitHandle
                );
                connectionsListener.ListenToConnectionRequests();
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "ListenToNewConnectionRequests"));
                throw;
            }
        }

        private void CleanupServer()
        {
            try
            {
                Console.WriteLine("Cleaning up server resources...");
                if (_tcpListener != null)
                {
                    _tcpListener.Stop();
                    _tcpListener = null;
                }

                lock (_clientsDictionaryLock)
                {
                    foreach (var client in _clientsDictionary.Values)
                    {
                        client.Close();
                    }
                    _clientsDictionary.Clear();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "CleanupServer"));
            }
        }
    }
}
