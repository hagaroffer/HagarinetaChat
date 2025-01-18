using ChatCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient
{
    public static class SharedResource
    {
        public static readonly object ConnectedUsersListLock = new object();
        public static List<string> ConnectedUsersList = new List<string>();
    }
}
