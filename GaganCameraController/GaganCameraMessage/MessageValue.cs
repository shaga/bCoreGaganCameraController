using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GaganCameraMessage
{
    public static class MessageValue
    {
        public const string PathStart = "MessageValue.Start";
        public const string PathStop = "MessageValue.Stop";
        public const string PathDisconnected = "MessageValue.Disconnected";

        public static string DataToString(this byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }

        public static byte[] StringToData(this string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }
    }
}
