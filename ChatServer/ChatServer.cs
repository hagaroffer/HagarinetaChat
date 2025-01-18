using ChatCommon;
using System.Net;
using System.Net.Sockets;

namespace ChatServer
{
    public class ChatServer
    {
        private TcpListener _tcpListener;

        public void InitServer()
        {
            try
            {
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
                Thread senderThread = new Thread(new QueueMessagesSender().HandleMessagesQueue);
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
                    _tcpListener
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

                lock (SharedResource.ClientsDictionaryLock)
                {
                    foreach (var client in SharedResource.ClientsDictionary.Values)
                    {
                        client.Close();
                    }
                    SharedResource.ClientsDictionary.Clear();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "CleanupServer"));
            }
        }
    }
}
