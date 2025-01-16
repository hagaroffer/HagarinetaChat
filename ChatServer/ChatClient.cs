using System.Net.Sockets;

namespace ChatServer
{
    public class ChatClient
    {
        public string UserName { get; set; }
        public TcpClient TcpClient { get; set; }

        public ChatClient(string userName, TcpClient tcpClient) 
        {
            UserName = userName;
            TcpClient = tcpClient;
        }
    }
}
