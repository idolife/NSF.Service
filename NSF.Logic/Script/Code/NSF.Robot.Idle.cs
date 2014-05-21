using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NSF.Share;
using NSF.Core;
using NSF.Interface;
using NSF.Framework.Base;
using NSF.Framework.Svc;
using NSF.Framework.Rpc;

namespace NSF.Robot.Idle
{
    public class RobotIdle : TcpHandler, IClientSvc, IClientImpl, IScript
    {
        /// <summary>
        /// 机器人对象身份ID产生种子。
        /// </summary>
        static Int64 UUID_SEED = 1000000;
        /// <summary>
        /// 本机器人对象身份ID。
        /// </summary>
        Int64 _UUID;


        /// <summary>
        /// 默认构造函数。
        /// </summary>
        public RobotIdle()
        {
            _UUID = Interlocked.Increment(ref UUID_SEED);
        }

        public async Task ExecuteAsync(Object rtmParam, Object confParam)
        {
            Log.Debug("[Script][RobotIdle][Execute], Param={0}.", confParam);
            try
            {
                ///
                TcpClient rClient = rtmParam as TcpClient;
                JObject jParam = confParam as JObject;
 
                /// 初始化底层是本对象处于运行状态
                Init(rClient);

                /// 触发连接就绪事件（逻辑）
                await OnReady(this);
            }
            catch (Exception e)
            {
                Log.Error("[Script][RobotIdle][Execute], {0}.", e);
            }
        }

        ///--------------------------------------------------------------
        /// IClientSvc接口实现
        /// <summary>
        /// 本机器人了身份ID。
        /// </summary>
        public Int64 UUID
        {
            get { return _UUID; }
        }

        /// <summary>
        /// 本机器人的远端地址。
        /// </summary>
        public string RemoteIP
        {
            get { return Peer_.Client.RemoteEndPoint.ToString(); }
        }

        /// <summary>
        /// 外发数据包。
        /// </summary>
        public async Task SendData(byte[] buff, int offset, int length)
        {
            await Peer_.GetStream().WriteAsync(buff, offset, length);
            Log.Debug("[Script][RobotIdle][SendData], [{0, 8}|{1, 5}], Data sended.", _UUID, length - offset);
        }

        public void Close()
        {
            Peer_.Close();
            Log.Debug("[Script][RobotIdle][Close], [{0, 8}], Finished close.", _UUID);
        }
        ///--------------------------------------------------------------

        ///--------------------------------------------------------------
        /// IClientImpl接口实现
        /// <summary>
        /// 机器人连接就绪处理。
        /// </summary>
        public Task OnReady(IClientSvc cli)
        {
            Log.Debug("[Script][RobotIdle][OnReady], [{0, 8}], Robot ready.", _UUID);
            throw new NotImplementedException();
        }

        /// <summary>
        /// 机器人连接数据包到达。
        /// </summary>
        public Task OnData(IDataBlock chunk)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 机器人连接异常（断开连接）
        /// </summary>
        public Task OnException()
        {
            throw new NotImplementedException();
        }
        ///--------------------------------------------------------------
    }
}
