using System.Collections.Generic;
using System.Threading.Tasks;
using BinarySerializer;
using MiscUtil.IO;
using Shared;
using Shared.ResultObjects;

namespace WebCommander.Helpers
{
    public static class ResultObjectHelper
    {
        public static async Task<T?> DeserializeResult<T>(byte[]? data, T? defaultValue = default)
        {
            if (data == null || data.Length == 0)
                return defaultValue;

            try 
            {
                return await data.BinaryDeserializeAsync<T>();
            }
            catch
            {
                return defaultValue;
            }
        }

        public static Task<ListDirectoryResult?> DeserializeListDirectoryResults(byte[]? data)
            => DeserializeResult<ListDirectoryResult?>(data, null);

        public static Task<List<ListProcessResult>> DeserializeListProcessResults(byte[]? data)
            => DeserializeResult(data, new List<ListProcessResult>())!;

        public static Task<List<Job>> DeserializeJobResults(byte[]? data)
            => DeserializeResult(data, new List<Job>())!;

        public static Task<List<LinkInfo>> DeserializeLinkInfoResults(byte[]? data)
            => DeserializeResult(data, new List<LinkInfo>())!;

        public static Task<List<ReversePortForwarResult>> DeserializeReversePortForwardResults(byte[]? data)
            => DeserializeResult(data, new List<ReversePortForwarResult>())!;

        public static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
