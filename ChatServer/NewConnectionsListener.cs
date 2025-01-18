using ChatCommon;
using System.Net.Sockets;

namespace ChatServer
{
    public class NewConnectionsListener
    {
        private TcpListener _tcpListener;

        public NewConnectionsListener(TcpListener tcpListener)
        {
            _tcpListener = tcpListener;
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
                var singleConnectionListener = new SingleConnectionListener(SharedResource.EventWaitHandle, client);
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

