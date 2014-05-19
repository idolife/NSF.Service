using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NSF.Share;
using NSF.Interface;
using NSF.Framework.Base;
using NSF.Framework.Svc;

namespace NSF.Game.Logic
{
    /// <summary>
    /// AgentHandler对象。
    /// </summary>
    public class AgentHandler : TcpHandler, IScript, IClientSvc
    {
        /// <summary>
        ///  协议实现对象。
        /// </summary>
        IClientImpl _Impl;

        /// <summary>
        /// 初始化并客户代理。
        /// </summary>
        public async Task ExecuteAsync(Object rtmParam, Object cnfParam)
        {
            Log.Debug("[Script][AgentHandler][Execute], Param={0}.", cnfParam);

            try
            {
                ///
                TcpClient rClient = rtmParam as TcpClient;
                JObject jParam = cnfParam as JObject;

                /// 载入客户端协议处理模块
                String rProtocolImpl = jParam.GetValue("ProtocolImpl").ToObject<String>();
                if (!rProtocolImpl.EndsWith(".dll"))
                    rProtocolImpl += ".dll";
                rProtocolImpl = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, rProtocolImpl);
                Assembly rAss = Assembly.LoadFile(rProtocolImpl);
                foreach (var i in rAss.ExportedTypes)
                {
                    if (i.GetInterfaces().Contains(typeof(IClientImpl)))
                    {
                        _Impl = (Activator.CreateInstance(i) as IClientImpl);
                        break;
                    }
                }

                /// 设置读写缓存区大小
                if (jParam.GetValue("ReadBufferSize") != null)
                {
                    TcpHandler.READ_BUFFER_SIZE =
                        jParam.GetValue("ReadBufferSize").ToObject<Int32>();
                }
                if (jParam.GetValue("WeadBufferSize") != null)
                {
                    TcpHandler.WRITE_BUFFER_SIZE =
                        jParam.GetValue("ReadBufferSize").ToObject<Int32>();
                }

                /// 触发连接就绪事件
                await _Impl.OnReady(this);

                /// 初始化底层数据
                /// （使本对象处于运行状态）
                Init(rClient);
            }
            catch(Exception e)
            {
                Log.Error("[Script][AgentHandler][Execute], {0}.", e);
            }
        }

        /// <summary>
        /// 处于数据包到达的逻辑。
        /// </summary>
        protected override async Task OnData(DataBlock chunk)
        {
            Log.Debug("[Script][AgentHandler][OnData], Length = {0}.", chunk.Length);

            /// 调用拆包/处理包逻辑
            await _Impl.OnData(chunk);
        }

        /// <summary>
        /// 处于异常逻辑。
        /// （连接会断开）
        /// </summary>
        protected override async Task OnException(Exception e)
        {
            Log.Debug("[Script][AgentHandler][OnException], Exception : {0}.", e);
            /// 触发事件
            await _Impl.OnException();
        }

        /// <summary>
        /// 外发数据。
        /// </summary>
        public async Task SendData(Byte[] buff, Int32 offset, Int32 length)
        {
            Log.Debug("[Script][AgentHandler][SendData], [LEN:{0}], Send buff. ", length);
            await Peer_.GetStream().WriteAsync(buff, offset, length);
        }

        /// <summary>
        /// 关闭底层连接。
        /// </summary>
        public void Close()
        {
            Log.Debug("[Script][AgentHandler][SendData], Positive close.");
            Peer_.Close();
        }

        /// <summary>
        /// 获取远端地址。
        /// </summary>
        public String RemoteIP 
        { 
            get { 
                return ((IPEndPoint)Peer_.Client.RemoteEndPoint).Address.ToString(); 
            } 
        }
    }
}
