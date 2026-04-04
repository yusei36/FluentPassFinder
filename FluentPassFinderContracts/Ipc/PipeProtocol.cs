using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace FluentPassFinder.Contracts.Public.Ipc
{
    public static class PipeProtocol
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public static void WriteMessage<T>(PipeStream stream, T message)
        {
            var json = JsonConvert.SerializeObject(message, SerializerSettings);
            var data = Encoding.UTF8.GetBytes(json);
            var lengthBytes = BitConverter.GetBytes(data.Length);
            stream.Write(lengthBytes, 0, 4);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        public static T ReadMessage<T>(PipeStream stream)
        {
            var lengthBytes = new byte[4];
            if (ReadExact(stream, lengthBytes, 4) != 4)
                return default(T);

            var length = BitConverter.ToInt32(lengthBytes, 0);
            var data = new byte[length];
            ReadExact(stream, data, length);

            var json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(json, SerializerSettings);
        }

        private static int ReadExact(PipeStream stream, byte[] buffer, int count)
        {
            var offset = 0;
            while (offset < count)
            {
                var read = stream.Read(buffer, offset, count - offset);
                if (read == 0)
                    return offset;
                offset += read;
            }
            return offset;
        }
    }
}
