using ChatCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    public static class SharedResource
    {
        public static readonly object ClientsDictionaryLock = new object ();
        public static Dictionary<string, TcpClient> ClientsDictionary = new Dictionary<string, TcpClient>();
        public static readonly object MessagesQueueLock = new object();
        public static Queue<ChatMessage> MessagesQueue = new Queue<ChatMessage>();
        public static readonly EventWaitHandle EventWaitHandle = new AutoResetEvent(false);
    }
}
