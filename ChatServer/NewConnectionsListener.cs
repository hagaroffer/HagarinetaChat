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
                return;
            }
        }

        private async Task WaitForNewConnectionRequest()
        {
            if (_tcpListener.Pending())
            {
                try
                {
                    var client = await _tcpListener.AcceptTcpClientAsync();

                    InitSingleConnectionListenerThread(client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(CreateExceptionMsg(ex, "WaitForNewConnectionRequest"));
                    throw;
                }
            }
        }

        private void InitSingleConnectionListenerThread(TcpClient client)
        {
            try
            {
                var a = new SingleConnectionListener(_clientsDictionary, _clientsDictionaryLock, _messagesQueue, _messagesQueueLock, _eventWaitHandle, client);
                ThreadPool.QueueUserWorkItem(new WaitCallback(a.HandleNewConnection));
            }
            catch (Exception ex)
            {
                Console.WriteLine(CreateExceptionMsg(ex, "InitSingleConnectionListenerThread"));
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

