using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient
{
    public interface IChatClient
    {
        void StartClient(string myUsername);
        void SendPrivateMessage(string destinationUsername, string message, string filePath);
        void SendBroadcastMessage(string message, string filePath);
        List<string> GetConnectedClients();
    }
}
