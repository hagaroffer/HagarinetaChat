using ChatCommon;
using System.Net.Sockets;

namespace ChatServer
{
    public class NewConnectionsListener
    {
        private TcpListener _tcpListener;
        private Dictionary<string, TcpClient> _clientsDictionary;
        private object _clientsDictionaryLock;
        private Queue<ChatMessage> _messagesQueue;
        private object _messagesQueueLock;
        private EventWaitHandle _eventWaitHandle;


        public NewConnectionsListener(TcpListener tcpListener, Dictionary<string, TcpClient> clientsDictionary, object clientsDictionaryLock,
            Queue<ChatMessage> messagesQueue, object messagesQueueLock, EventWaitHandle eventWaitHandle)
        {
            _tcpListener = tcpListener;
            _clientsDictionary = clientsDictionary;
            _clientsDictionaryLock = clientsDictionaryLock;
            _messagesQueue = messagesQueue;
            _messagesQueueLock = messagesQueueLock;
            _eventWaitHandle = eventWaitHandle;
        }

        public void ListenToConnectionRequests()
        {
            try
            {
                while (true)
                {
                    WaitForNewConnectionRequest();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ListenToConnectionRequests: {ex.Message}");
            }
        }

        private void WaitForNewConnectionRequest()
        {
            if (_tcpListener.Pending())
            {
                var client = _tcpListener.AcceptTcpClient();
                InitSingleConnectionListenerThread(client);
            }
        }

        private void InitSingleConnectionListenerThread(TcpClient client)
        {
            try
            {
                var singleConnectionListener = new SingleConnectionListener(_clientsDictionary, _clientsDictionaryLock, _messagesQueue, _messagesQueueLock, _eventWaitHandle, client);
                ThreadPool.QueueUserWorkItem(new WaitCallback(singleConnectionListener.HandleNewConnection));
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "InitSingleConnectionListenerThread"));
                throw;
            }
        }
    }
}

