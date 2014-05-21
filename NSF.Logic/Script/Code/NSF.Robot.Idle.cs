using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Reflection;
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
        /// 
        /// </summary>
        public long UUID
        {
            get { throw new NotImplementedException(); }
        }

        public string RemoteIP
        {
            get { throw new NotImplementedException(); }
        }

        public Task SendData(byte[] buff, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }
        ///--------------------------------------------------------------

        ///--------------------------------------------------------------
        /// IClientImpl接口实现
        /// <summary>
        /// 机器人连接就像处理。
        /// </summary>
        public Task OnReady(IClientSvc cli)
        {
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
        /// <returns></returns>
        public Task OnException()
        {
            throw new NotImplementedException();
        }
        ///--------------------------------------------------------------
    }
}
