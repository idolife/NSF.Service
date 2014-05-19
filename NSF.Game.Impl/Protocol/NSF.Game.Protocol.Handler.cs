using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NSF.Core;
using NSF.Share;
using NSF.Interface;

namespace NSF.Game.Logic
{

    /// <summary>
    /// 游戏客户端逻辑处理对象。
    /// </summary>
    public class ProtocolHandler : IClientImpl
    {
        /// <summary>
        /// 参数连接UUID的种子对象。
        /// </summary>
        static Int64 _UUID_SEED = 10000000;
        /// <summary>
        /// 连接对象的唯一ID。
        /// </summary>
        Int64 _UUID;
        /// <summary>
        /// 关联的连接对象。
        /// </summary>
        IClientSvc _Client;

        /// <summary>
        /// 处理连接就绪。
        /// </summary>
        public Task OnReady(IClientSvc cli)
        {
            /// 保存关联的连接。
            _Client = cli;
            /// 创建唯一ID
            _UUID = Interlocked.Increment(ref _UUID_SEED);

            ///
            Log.Debug("[Agent][ProtocolHandler][OnReady], [UUID:{0, 8}, Remote:{1}]", _UUID, _Client.RemoteIP);
            
            ///
            return null;
        }

        /// <summary>
        /// 处理数据包到达。
        /// </summary>
        public async Task OnData(IDataBlock chunk)
        {
            /// 循环处理这个数据包的所有消息
            while (true)
            {
                /// 解包
                Object msg = ProtocollProvide.DecodeMessage(chunk);
                /// 数据包不完整
                if (msg == null)
                    break;

                /// 处理协议消息
                await OnMessage(msg);
            }
        }

        /// <summary>
        /// 处理连接异常。
        /// </summary>
        public Task OnException()
        {
            ///
            Log.Debug("[Agent][ProtocolHandler][OnException], [UUID:{0, 8}, Remote:{1}]", _UUID, _Client.RemoteIP);
            ///
            return null;
        }

        /// <summary>
        /// 处理消息协议。
        /// </summary>
        protected Task OnMessage(Object msg)
        {
            throw new NotImplementedException("OnMessage");
        }
    }
}
