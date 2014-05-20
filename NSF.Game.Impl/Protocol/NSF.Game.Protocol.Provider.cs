using System;
using System.IO;
using ProtoBuf.Meta;
using Newtonsoft.Json;
using NSF.Share;
using NSF.Interface;

namespace NSF.Game.Logic
{
    /// <summary>
    /// 游戏协议提供者。
    /// </summary>
    public static class ProtocollProvide
    {
        /// <summary>
        /// 最大数据包长度。
        /// </summary>
        public static Int32 MAX_PACKAGE_SIZE = 1024 * 8;
        /// <summary>
        /// 超过这个最小数据体长度才会实行GZIP压缩。
        /// </summary>
        public static Int32 MIN_GZIP_DATA_SIZE = 1024;
        /// <summary>
        /// 数据包选项：使用GZIP压缩。
        /// </summary>
        public static UInt32 PACKAGE_USE_GZIP = 0x80000000;
        /// <summary>
        /// 数据包选项：使用动态加密。
        /// </summary>
        public static UInt32 PACKAGE_USE_CYPT = 0x40000000;

        /// <summary>
        /// 协议解包。
        /// </summary>
        public static GameReq DecodeMessage(IDataBlock chunk)
        {
            byte[] dataBuff = chunk.Buffer;
            Int32 dataOffset = chunk.ReadPosition;
            Int32 dataLength = chunk.Length;
            const Int32 INT32SIZE = sizeof(Int32);

            /// 不足包头的长度
            if (dataLength < INT32SIZE)
                return null;
            Int32 msgHeadSign = BitConverter.ToInt32(dataBuff, dataOffset);
            /// 低位4字节为完整包的长度
            Int32 msgFullLength = msgHeadSign & 0x0000FFFF;
            /// 非法数据包
            if (msgFullLength > MAX_PACKAGE_SIZE)
                throw new InvalidDataException("DecodeMessage");
            /// 不足完整包长度             
            if (dataLength < msgFullLength)
                return null;

            /// 包足够一个完整包
            dataLength -= INT32SIZE;
            dataOffset += INT32SIZE;

            /// 使用Protobuff解析获得消息包的对象
            /// 反序列化消息
            Int32 protoLength = msgFullLength - INT32SIZE;
            MemoryStream readStream = new MemoryStream(dataBuff, dataOffset, protoLength);
            GameReq msgObj = new GameReq();
            RuntimeTypeModel.Default.Deserialize(readStream, msgObj, typeof(GameReq));
            /// 移动数据块读指针
            chunk.ReadOffset(msgFullLength);

            return msgObj;
        }

        /// <summary>
        /// 协议打包。
        /// </summary>
        public static ArraySegment<Byte> EncodeMessage(Object msgCore)
        {
            /// 准备缓存
            const Int32 INT32SIZE = sizeof(Int32);
            Byte[] msgFullBuff = new Byte[MAX_PACKAGE_SIZE];
            MemoryStream msHeader = new MemoryStream(msgFullBuff, 0, INT32SIZE);

            /// 打包Json消息
            String msgJsonString = JsonConvert.SerializeObject(msgCore);
            /// 序列化消息对象
            MemoryStream msBody = new MemoryStream(msgFullBuff, INT32SIZE, MAX_PACKAGE_SIZE - INT32SIZE);
            GameAck msgObj = new GameAck { Json = msgJsonString };
            RuntimeTypeModel.Default.Serialize(msBody, msgObj);

            /// 序列化消息头
            BinaryWriter hdWriter = new BinaryWriter(msHeader);
            hdWriter.Write((Int32)msBody.Position + INT32SIZE);

            /// 返回打包后的缓存
            return new
                ArraySegment<Byte>(msgFullBuff, 0, (Int32)msBody.Position + INT32SIZE);
        }
    }
}
