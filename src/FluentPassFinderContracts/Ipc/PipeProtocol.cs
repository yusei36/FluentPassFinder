// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO.Pipes;
using System.Text;

namespace FluentPassFinder.Contracts.Public.Ipc
{
    public static class PipeProtocol
    {
        // Bounds the length prefix so a hostile/corrupt peer cannot force a huge allocation.
        private const int MaxFrameBytes = 32 * 1024 * 1024;

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            // Replace default-initialized collections instead of appending to them,
            // otherwise default entries (e.g. ExcludeFields) duplicate on every pipe round-trip.
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            Converters = { new StringEnumConverter() }
        };

        public static void WriteRequest(PipeStream stream, PipeRequest message)
        {
            WriteRaw(stream, JsonConvert.SerializeObject(message, message.GetType(), SerializerSettings));
        }

        public static void WriteResponse(PipeStream stream, PipeResponse message)
        {
            WriteRaw(stream, JsonConvert.SerializeObject(message, message.GetType(), SerializerSettings));
        }

        /// <summary>Read one length-prefixed frame and deserialize to the correct concrete request type.</summary>
        public static PipeRequest ReadRequest(PipeStream stream)
        {
            var json = ReadJson(stream);
            if (json == null) return null;
            return JsonConvert.DeserializeObject<PipeRequest>(json, SerializerSettings);
        }

        /// <summary>Read one length-prefixed frame and deserialize to the expected response type.</summary>
        public static T ReadResponse<T>(PipeStream stream) where T : PipeResponse
        {
            var json = ReadJson(stream);
            if (json == null) return null;
            return JsonConvert.DeserializeObject<T>(json, SerializerSettings);
        }

        private static void WriteRaw(PipeStream stream, string json)
        {
            var data = Encoding.UTF8.GetBytes(json);
            var lengthBytes = System.BitConverter.GetBytes(data.Length);
            stream.Write(lengthBytes, 0, 4);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        private static string ReadJson(PipeStream stream)
        {
            var lengthBytes = new byte[4];
            if (ReadExact(stream, lengthBytes, 4) != 4)
                return null;

            var length = System.BitConverter.ToInt32(lengthBytes, 0);
            if (length < 0 || length > MaxFrameBytes)
                return null;

            var data = new byte[length];
            if (ReadExact(stream, data, length) != length)
                return null;
            return Encoding.UTF8.GetString(data);
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
