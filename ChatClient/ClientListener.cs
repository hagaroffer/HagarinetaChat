﻿using ChatCommon;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;

namespace ChatClient
{
    public class ClientListener
    {
        //public List<string> _connectedUsersList;
        //private object _connectedUsersListLock;
        //private NetworkStream _stream;
        //private TcpClient _client;


        public ClientListener(List<string> connectedUsersList, object connectedUsersListLock,
            /*TcpClient client,*/ NetworkStream stream)
        {
            //_connectedUsersList = connectedUsersList;
            //_connectedUsersListLock = connectedUsersListLock;
            //_client = client;
            //_stream = stream;
        }

        public void ListenToIncomingMessages()
        {
            try
            {
                while (true)
                {
                    if (ChatClient._stream == null || !ChatClient._stream.CanRead)
                    {
                        Console.WriteLine("Disconnected from server.");
                        break;
                    }

                    var message = ReadMessage();
                    if (message == null)
                    {
                        Console.WriteLine("Server disconnected.");
                        break;
                    }
                    ProcessMessage(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Disconnected due to an error: " + ex.Message);
            }
            finally
            {
                ChatClient._stream?.Close();
                ChatClient.Instance.Disconnect();
            }
        }

        private ChatMessage ReadMessage()
        {
            try
            {
                return ChatMessageTranfer.ReadMessage(ChatClient._stream);
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "ReadMessage"));
                return null;
            }
        }

        private void ProcessMessage(ChatMessage message)
        {
            try
            {
                if (message == null)
                {
                    return;
                }

                if (message.MessageType == MessageType.ConnectedUsers)
                {
                    lock (ChatClient._connectedUsersListLock)
                    {
                        ChatClient._connectedUsersList = message.Message.Split(" ").ToList();
                    }
                    return;
                }

                string messageToShow = string.Empty;
                if (message.MessageType == MessageType.Private)
                {
                    messageToShow += string.Format("Private message ");
                }
                else if (message.MessageType == MessageType.Broadcast)
                {
                    messageToShow += string.Format("Broadcast message ");
                }

                messageToShow += string.Format("from {0}: {1}", message.SourceUsername, message.Message);

                if (message.File != null)
                {
                    string filePath = CreateNewFilePath(message.File.FileNameWithExtension);
                    File.WriteAllBytes(filePath, message.File.Content);
                    messageToShow += string.Format(", and a file: {0}", filePath);
                }

                Console.WriteLine(messageToShow);
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "ProcessMessage"));
                return;
            }
        }

        private string CreateNewFilePath(string originalfileName)
        {
            string randomString = new Random().Next().ToString();
            var dotIndex = originalfileName.IndexOf('.');
            var fileName = originalfileName.Substring(0, dotIndex);
            var fileExtension = originalfileName.Substring(dotIndex);
            string path = string.Concat(fileName, "_", randomString, fileExtension);
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), path);
            return filePath;
        }
    }
}
