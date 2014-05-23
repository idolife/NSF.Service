using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NSF.Share;
using NSF.Core;
using NSF.Interface;
using NSF.Framework.Svc;
using System.Diagnostics;

namespace NSF.Game.Logic
{
    /// <summary>
    /// AgentManager对象。
    /// </summary>
    public class MgrAgent : IScript, IModule
    {
        /// <summary>
        /// 所有的客户端对象。
        /// </summary>
        ConcurrentDictionary<Int64, IClientSvc> _ClientRepo = new ConcurrentDictionary<Int64, IClientSvc>();

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
            Log.Debug("[AgentMgr][Execute], Param={0}.", confParam);

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
                Log.Error("[AgentMgr][Execute], {0}.", e);
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

        /// <summary>
        /// 模块支持的命令。
        /// </summary>
        public bool Command(String cmd, Object param)
        {
            Log.Debug("[AgentMgr][Command], [{0}].", cmd);
            bool retVal = false;
            switch(cmd.ToUpper())
            {
                /// 加入“客户端连接”管理
                case "JOIN" :
                    IClientSvc joinCli = param as IClientSvc;
                    Debug.Assert(joinCli != null);
                    retVal = _ClientRepo.TryAdd(joinCli.UUID, joinCli);
                    Log.Info("[AgentMgr][Join], [{0, 8}|{1, 4}] = [{2, 5}].", joinCli.UUID, retVal, _ClientRepo.Count);
                    break;
                /// 离开“客户端连接”管理
                case "LEAVE" :
                    Int64 uuid = (param as IClientSvc).UUID;
                    IClientSvc levCli;
                    retVal = _ClientRepo.TryRemove(uuid, out levCli);
                    Log.Info("[AgentMgr][Leav], [{0, 8}|{1, 4}] = [{2, 5}].", uuid, retVal, _ClientRepo.Count);
                    break;
                default :
                    Log.Warn("[AgentMgr][Command], [{0}], Unkown support command.", cmd);
                    break;
            }

            ////
            return retVal;
        }
    }
}
