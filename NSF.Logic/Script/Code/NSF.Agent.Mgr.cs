using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NSF.Share;
using NSF.Core;
using NSF.Interface;
using NSF.Framework.Svc;

namespace NSF.Game.Logic
{
    /// <summary>
    /// AgentManager对象。
    /// </summary>
    public class MgrAgent : IScript, IModule
    {
        /// <summary>
        /// 本模块名称。
        /// </summary>
        public String Name { get { return "NSF.Agent.Mgr"; } }
        
        /// <summary>
        /// 连接处理的脚本名称。
        /// （从配置文件获取）
        /// </summary>
        protected String HandlerScript { get; private set; }

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

                /// 获取连接处理脚本名称
                HandlerScript = jParam.GetValue("HandlerScript").ToObject<String>();

                /// 注册到连接接收器
                tcpAcc.RegisterSevice("NSF.Agent.Mgr", HandleAgent);

                /// 注册本身到模块管理
                MgrModule.Instance.Join(this);
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
            /// 创建连接处理器对象
            MgrScript.Instance.ExecuteAsync(HandlerScript, client).Wait();
        }
    }
}
