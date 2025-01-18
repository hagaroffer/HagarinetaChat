using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ChatCommon
{
    public static class ChatMessageTranfer
    {
        public static ChatMessage ReadMessage(NetworkStream stream)
        {
            try
            {
                byte[] data = new byte[1000000];
                int bytesRead = stream.Read(data, 0, 1000000);

                using (MemoryStream ms = new MemoryStream(data, 0, bytesRead))
                {
                    var message = JsonSerializer.Deserialize<ChatMessage>(ms);
                    return message;
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static byte[] PrepareMessageToBeSent(ChatMessage message)
        {
            var serializedMessage = JsonSerializer.Serialize(message);
            byte[] myWriteBuffer = Encoding.ASCII.GetBytes(serializedMessage);
            return myWriteBuffer;
        }

        public static void SendMessage(byte[] convertedMessageBuffer, NetworkStream stream)
        {
            stream.Write(convertedMessageBuffer, 0, convertedMessageBuffer.Length);
        }
    }
}
