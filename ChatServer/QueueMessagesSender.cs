using ChatCommon;
using System.Net.Sockets;

namespace ChatServer
{
    public class QueueMessagesSender
    {
        public QueueMessagesSender() 
        {
        }

        public void HandleMessagesQueue()
        {
            try
            {
                while (true)
                {
                    SharedResource.EventWaitHandle.WaitOne();
                    bool dequeueSucceed = false;
                    ChatMessage message;

                    lock (SharedResource.MessagesQueueLock)
                    {
                        dequeueSucceed = SharedResource.MessagesQueue.TryDequeue(out message);
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
            var messageSender = new MessageSender();

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
