using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NSF.Share
{
    /// <summary>
    /// 工具类，提供一些辅助的功能的函数。
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// 常规字符串转换为Base64字符串。
        /// </summary>
        public static String ToBase64String(String normalString)
        {
            Byte[] base64 = System.Text.Encoding.UTF8.GetBytes(normalString);
            return Convert.ToBase64String(base64);
        }

        /// <summary>
        /// Base64字符串转换为常规字符串。
        /// </summary>
        public static String FromBase64String(String base64String)
        {
            Byte[] base64 = Convert.FromBase64String(base64String);
            return System.Text.Encoding.UTF8.GetString(base64);
        }

        /// <summary>
        /// 使用GZip算法压缩字符串。
        /// </summary>
        public static Byte[] GZipCompressString(String toCompress, Encoding useEncoding)
        {
            /// 压缩（字符串按照指定编码转为字节流）
            MemoryStream cStream = new MemoryStream();
            Byte[] toBytes = useEncoding.GetBytes(toCompress);
            GZipStream zStream = new GZipStream(cStream, CompressionMode.Compress, true);
            zStream.Write(toBytes, 0, toBytes.Length);
            zStream.Close();

            /// 转为字节流
            return cStream.ToArray();
        }

        /// <summary>
        /// 使用GZip算法解压字符串。
        /// </summary>
        public static String GZipDecompressString(Byte[] toDecompress, Encoding useEncoding)
        {
            /// 解压
            MemoryStream cStream = new MemoryStream(toDecompress);
            GZipStream zStream = new GZipStream(cStream, CompressionMode.Decompress, true);
            MemoryStream dStream = new MemoryStream();
            zStream.CopyTo(dStream);

            /// 转为指定编码的字符串。
            return
                useEncoding.GetString(dStream.GetBuffer(), 0, (int)dStream.Length);
        }

        /// <summary>
        /// 本地系统的时区。
        /// </summary>
        public static int UtcOffset = (int)TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalHours;

        /// <summary>
        /// DateTime时间格式转换为Unix的时间戳。
        /// （时间调整到UTC0）
        /// </summary>		
        public static int DateTimeToTimestamp(DateTime dtSrc)
        {

            /// 从1970-1-1 0:0:0开始经过的秒数为基准
            return
                (int)(dtSrc.AddHours(-UtcOffset) - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }

        /// <summary>
        /// Unix的时间戳到DateTime时间格式的转换。
        /// （时间调整到UTC0）
        /// </summary>
        public static DateTime TimestampToDateTime(int totalSeconds)
        {
            /// 从1970-1-1 0:0:0开始经过的秒数为基准
            return
                new DateTime(1970, 1, 1, 0, 0, 0).AddHours(UtcOffset).AddSeconds(totalSeconds);
        }

        /// <summary>
        /// 判断2个时间戳是否在同一天。
        /// </summary>
        public static Boolean IsSampeDay(int timestamp1, int timestamp2)
        {
            if (timestamp1 == timestamp2)
                return true;

            DateTime dt1 = TimestampToDateTime(timestamp1);
            DateTime dt2 = TimestampToDateTime(timestamp2);

            return
                ((dt1.Year == dt2.Year) &&
                 (dt1.Month == dt2.Month) &&
                 (dt1.Day == dt2.Day));
        }

        /// <summary>
        /// 计算两个日期相隔的天数。
        /// </summary>
        public static Int32 CalculateDayOffset(DateTime dt1, DateTime dt2)
        {
            if (dt1.DayOfYear > dt2.DayOfYear)
            {
                return (dt1.DayOfYear - dt2.DayOfYear);
            }
            else
            {
                return (dt2.DayOfYear - dt1.DayOfYear);
            }
        }

        /// <summary>
        /// Camel格式的Json序列化过滤器。
        /// </summary>
        private static JsonSerializerSettings JsonSetting = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        /// <summary>
        /// 序列对象到Json字符串。
        /// </summary>
        public static String SerializeToJsonString(object value)
        {
            return
                JsonConvert.SerializeObject(value, Newtonsoft.Json.Formatting.None, JsonSetting);
        }
    }
}
