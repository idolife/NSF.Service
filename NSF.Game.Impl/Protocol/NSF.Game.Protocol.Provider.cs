using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSF.Share;
using NSF.Interface;

namespace NSF.Game.Logic
{
    /// <summary>
    /// 游戏协议提供者。
    /// </summary>
    public static class ProtocollProvide
    {
        //public static Type GetTypeById(Int32 msgId)
        //{
        //    throw new NotImplementedException();
        //}

        //public static Int32 GetIdByType(Type msgType)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// 协议解包。
        /// </summary>
        public static GameReq DecodeMessage(IDataBlock chunk)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 协议打包。
        /// </summary>
        public static Byte[] EncodeMessage(Object msgObj)
        {
            throw new NotImplementedException();
        }
    }
}
