using ChatCommon;
using System.Net.Sockets;

namespace ChatServer
{
    public class QueueMessagesSender
    {
        private Queue<ChatMessage> _messagesQueue;
        private object _messagesQueueLock;
        private EventWaitHandle _eventWaitHandle;
        private Dictionary<string, TcpClient> _clientsDictionary;
        private object _clientsDictionaryLock;

        public QueueMessagesSender(Queue<ChatMessage> messagesQueue, object messagesQueueLock, EventWaitHandle eventWaitHandle, Dictionary<string, TcpClient> clientsDictionary, object clientsDictionaryLock) 
        {
            _messagesQueue = messagesQueue;
            _messagesQueueLock = messagesQueueLock;
            _eventWaitHandle = eventWaitHandle;
            _clientsDictionary = clientsDictionary;
            _clientsDictionaryLock = clientsDictionaryLock;
        }

        public void HandleMessagesQueue()
        {
            try
            {
                while (true)
                {
                    _eventWaitHandle.WaitOne();
                    bool dequeueSucceed = false;
                    ChatMessage message;

                    lock (_messagesQueueLock)
                    {
                        dequeueSucceed = _messagesQueue.TryDequeue(out message);
                    }

                    if (dequeueSucceed)
                    {
                        SendMessage(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonCommands.CreateExceptionMsg(ex, "HandleMessagesQueue"));
                return;
            }
        }

        private void SendMessage(ChatMessage message)
        {
            var messageSender = new MessageSender(_clientsDictionary, _clientsDictionaryLock);

            if (message.MessageType.Equals(MessageType.Private))
            {
                messageSender.SendPrivateMessage(message);
            }
            else
            {
                messageSender.SendBroadcastMessage(message);
            }
        }
    }
}
