
//using ChatServer;
using ChatServer;
using Microsoft.VisualBasic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

public class Program
{
    public static List<TcpClient> ClientsList;
    public static TcpClient Client;
    public static TcpListener TcpListener;

    static void Main(string[] args)
    {
        var tc = new ChatServer.ChatServer();
        tc.InitServer();
    }

    
}