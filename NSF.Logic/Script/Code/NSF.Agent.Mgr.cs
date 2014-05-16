using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NSF.Share;
using NSF.Interface;
using NSF.Framework.Svc;

namespace NSF.Game.Logic
{
    /// <summary>
    /// AgentManager对象。
    /// </summary>
    public class MgrAgent : IScript
    {
        /// <summary>
        /// 初始化并运行AgentManager所提供的服务。
        /// </summary>
        public Task ExecuteAsync(Object tcpSvc, Object confParam)
        {
            Log.Debug("[Script][AgentMgr][Execute], Param={0}.", confParam);

            try
            {
                ///
                JObject jParam = confParam as JObject;                
                TcpAcceptor tcpAcc = tcpSvc as TcpAcceptor;

                /// 注册Tcp接收器的逻辑处理
                tcpAcc.RegisterSevice("NSF.Agent.Mgr", HandleAgent);
            }
            catch(Exception e)
            {
                Log.Error("[Script][Game][Execute], {0}.", e);
            }

            ///
            return
                Task.FromResult(0);
        }

        /// <summary>
        /// 处理新连接逻辑。
        /// </summary>
        private void HandleAgent(TcpClient client)
        {
        }
    }
}
