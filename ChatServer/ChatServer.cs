using ChatCommon;
using System.IO;
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

                InitNewConnectionsListenerThread();

                Console.WriteLine("Chat server is running");
            }
            catch (Exception ex)
            {
                return;
            }
        }

        private void InitVariables()
        {
            _clientsDictionary = new Dictionary<string, TcpClient>();
            _clientsDictionaryLock = new object();
            _messagesQueue = new Queue<ChatMessage>();
            _messagesQueueLock = new object();
            _eventWaitHandle = new AutoResetEvent(false);
        }

        private void InitTcpListener()
        {
            try
            {
                string ipAddress = Configurations.IpAddress;
                int port = int.Parse(Configurations.Port);

                _clientsDictionary = new Dictionary<string, TcpClient>();

                _tcpListener = new TcpListener(IPAddress.Parse(ipAddress), port);
                _tcpListener.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(CreateExceptionMsg(ex, "InitTcpListener"));
                if (_tcpListener != null)
                {
                    _tcpListener.Stop();
                }
                throw;
            }           
        }

        private void InitQueueMessagesSenderThread()
        {
            try
            {
                Thread senderThread = new Thread(new QueueMessagesSender(_messagesQueue, _messagesQueueLock, _eventWaitHandle, _clientsDictionary, _clientsDictionaryLock).HandleMessagesQueue);
                senderThread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(CreateExceptionMsg(ex, "InitQueueMessagesSenderThread"));
                throw;
            }
        }

        private void InitNewConnectionsListenerThread()
        {
            //var a = new NewConnectionsListener(_tcpListener, _clientsDictionary, _clientsDictionaryLock, _messagesQueue, _messagesQueueLock, _eventWaitHandle);
            //ThreadPool.QueueUserWorkItem(new WaitCallback(a.ListenToConnectionRequests));

            try
            {

                Thread newConnectionsListenerThread = new Thread(new NewConnectionsListener(_tcpListener, _clientsDictionary, _clientsDictionaryLock, _messagesQueue, _messagesQueueLock, _eventWaitHandle).ListenToConnectionRequests);
                newConnectionsListenerThread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(CreateExceptionMsg(ex, "InitNewConnectionsListenerThread"));
                throw;
            }
        }

        private string CreateExceptionMsg(Exception ex, string methodName)
        {
            string errorMsg = string.Format(@"Failure in {0}.{1}Error: {2}. {3}StackTrace: {4}", methodName, Environment.NewLine, ex.Message, Environment.NewLine, ex.StackTrace);
            return errorMsg;
        }
    }
}
