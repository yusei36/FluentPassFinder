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

        /// <summary>
        /// Serialize <paramref name="message"/> (using its runtime type) and write it
        /// length-prefixed to <paramref name="stream"/>.
        /// </summary>
        public static void WriteMessage(PipeStream stream, PipeEnvelope message)
        {
            var json = JsonConvert.SerializeObject(message, message.GetType(), SerializerSettings);
            var data = Encoding.UTF8.GetBytes(json);
            var lengthBytes = BitConverter.GetBytes(data.Length);
            stream.Write(lengthBytes, 0, 4);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        /// <summary>Read one length-prefixed message and return the raw JSON string.</summary>
        public static string ReadJson(PipeStream stream)
        {
            var lengthBytes = new byte[4];
            if (ReadExact(stream, lengthBytes, 4) != 4)
                return null;

            var length = BitConverter.ToInt32(lengthBytes, 0);
            var data = new byte[length];
            ReadExact(stream, data, length);
            return Encoding.UTF8.GetString(data);
        }

        /// <summary>Deserialize a previously read JSON string to a concrete envelope type.</summary>
        public static T Deserialize<T>(string json) where T : PipeEnvelope
        {
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
