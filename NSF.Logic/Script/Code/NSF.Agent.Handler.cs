using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NSF.Share;
using NSF.Core;
using NSF.Interface;
using NSF.Framework.Base;
using NSF.Framework.Svc;

namespace NSF.Game.Logic
{
    /// <summary>
    /// AgentHandler对象。
    /// </summary>
    public class AgentHandler : TcpHandler, IClientSvc, IScript
    {
        /// <summary>
        ///  协议实现对象。
        /// </summary>
        IClientImpl _Impl;
        IModule _Mgr;

        /// <summary>
        /// 初始化并客户代理。
        /// </summary>
        public async Task ExecuteAsync(Object rtmParam, Object cnfParam)
        {
            Log.Debug("[AgentHandler][Execute], Param={0}.", cnfParam);

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

                /// 初始化底层数据
                /// （使本对象处于运行状态）
                Init(rClient);

                /// 触发连接就绪事件
                await _Impl.OnReady(this);

                /// 获取客户管理模块并加入管理
                _Mgr = MgrModule.Instance.Find("NSF.Agent.Mgr");
                _Mgr.Command("Join", this);
            }
            catch(Exception e)
            {
                Log.Error("[AgentHandler][Execute], {0}.", e);
            }
        }

        /// <summary>
        /// 处理数据包到达的逻辑。
        /// </summary>
        protected override async Task OnData(DataBlock chunk)
        {
            Log.Debug("[AgentHandler][OnData], Length = {0}.", chunk.Length);

            /// 调用拆包/处理包逻辑
            await _Impl.OnData(chunk);
        }

        /// <summary>
        /// 处于异常逻辑。
        /// （连接会断开）
        /// </summary>
        protected override async Task OnException(Exception e)
        {
            Log.Debug("[AgentHandler][OnException], Exception : {0}.", e);
            /// 离开管理
            _Mgr.Command("Leave", this);
            /// 触发事件
            await _Impl.OnException();
        }

        /// <summary>
        /// 外发数据。
        /// </summary>
        public async Task SendData(Byte[] buff, Int32 offset, Int32 length)
        {
            Log.Debug("[AgentHandler][SendData], [LEN:{0}], Send buff. ", length);
            await Peer_.GetStream().WriteAsync(buff, offset, length);
        }

        /// <summary>
        /// 关闭底层连接。
        /// </summary>
        public void Close()
        {
            Log.Debug("[AgentHandler][SendData], Positive close.");
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

        public Int64 UUID { get { return _Impl.UUID; } }
    }
}
