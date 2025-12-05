using System.Collections.Generic;
using System.Threading.Tasks;
using BinarySerializer;
using MiscUtil.IO;
using WebCommander.Models.ResultObjects;

namespace WebCommander.Helpers
{
    public static class ResultObjectHelper
    {
        public static async Task<ListDirectoryResult?> DeserializeListDirectoryResults(byte[] data)
        {
            if (data == null || data.Length == 0)
                return null;

            try 
            {
                return await data.BinaryDeserializeAsync<ListDirectoryResult>();
            }
            catch
            {
                return null;
            }
        }

        public static async Task<List<ListProcessResult>> DeserializeListProcessResults(byte[] data)
        {
            if (data == null || data.Length == 0)
                return new List<ListProcessResult>();

            try 
            {
                return await data.BinaryDeserializeAsync<List<ListProcessResult>>();
            }
            catch
            {
                return new List<ListProcessResult>();
            }
        }

        public static async Task<List<Job>> DeserializeJobResults(byte[] data)
        {
            if (data == null || data.Length == 0)
                return new List<Job>();

            try 
            {
                return await data.BinaryDeserializeAsync<List<Job>>();
            }
            catch
            {
                return new List<Job>();
            }
        }

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
