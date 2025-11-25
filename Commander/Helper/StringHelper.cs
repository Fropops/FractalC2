using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Helper
{
    internal static class StringHelper
    {
        public static string FileLengthToString(long lengthInBytes)
        {
            double lengthInKb = lengthInBytes / 1024.0;
            double lengthInMb = lengthInKb / 1024.0;
            double lengthInGb = lengthInMb / 1024.0;

            string lengthString = lengthInBytes < 1024
            ? $"{lengthInBytes} bytes"
            : lengthInKb < 1024
                ? $"{lengthInKb:F1} KB"
                : lengthInMb < 1024
                    ? $"{lengthInMb:F1} MB"
                    : $"{lengthInGb:F1} GB";

            return lengthString;
        }

        public static string FormatElapsedTime(double seconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);

            string formattedTime = "";

            if (timeSpan.Days > 0)
            {
                formattedTime += $"{timeSpan.Days}d ";
            }

            if (timeSpan.Hours > 0 || timeSpan.Days > 0)
            {
                formattedTime += $"{timeSpan.Hours}h ";
            }

            if (timeSpan.Minutes > 0 || timeSpan.Hours > 0 || timeSpan.Days > 0)
            {
                formattedTime += $"{timeSpan.Minutes}m ";
            }

            formattedTime += $"{timeSpan.Seconds:00}.{timeSpan.Milliseconds / 10:00}s";

            return formattedTime.Trim(); // Trim any leading/trailing white spaces
        }

        public static string IpAsString(byte[] ipAddressBytes)
        {
            if (ipAddressBytes == null)
                return string.Empty;
            return string.Join(".", ipAddressBytes);
        }

    }
}
