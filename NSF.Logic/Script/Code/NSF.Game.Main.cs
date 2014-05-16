using System;
using System.Net;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NSF.Share;
using NSF.Core;
using NSF.Interface;
using NSF.Framework.Svc;
using NSF.Framework.Rpc;
using System.Reflection;

namespace NSF.Game.Main
{
    /// <summary>
    /// Game服务对象。
    /// </summary>
    public class GameServer : IScript
    {
        /// <summary>
        /// 初始化并运行GameServer所提供的服务。
        /// </summary>
        public async Task ExecuteAsync(Object ___, Object confParam)
        {
            Log.Debug("[Script][Game][Execute], Param={0}.", confParam);

            try
            {
                ///
                JObject jParam = confParam as JObject;

                /// 开启TcpAcceptor服务  
                List<JObject> confTcpService = jParam.GetValue("TcpService").ToObject<List<JObject>>();
                foreach(var v in confTcpService)
                {
                    /// 开启TCP侦听
                    Int32 port = v.GetValue("Port").ToObject<Int32>();
                    var tcpSvc = new TcpAcceptor();
                    tcpSvc.Init(port);

                    /// 注册该TCP侦听服务处理
                    List<String> svc = v.GetValue("Svc").ToObject<List<String>>();
                    foreach(var s in svc)
                    {
                        await MgrScript.Instance.ExecuteAsync(s, tcpSvc);
                    }
                }

                /// 注册RpcServer服务
                List<JObject> confRpcService = jParam.GetValue("RpcService").ToObject<List<JObject>>();
                foreach(var r in confRpcService)
                {
                    /// 开启RPC服务
                    Int32  rPort = r.GetValue("Port").ToObject<Int32>();
                    String rImpl = r.GetValue("Impl").ToObject<String>();
                    if (!rImpl.EndsWith(".dll"))
                        rImpl += ".dll";
                    rImpl = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, rImpl);
                    RpcInterface rInt = null;
                    Assembly rAss = Assembly.LoadFile(rImpl);
                    foreach(var i in rAss.ExportedTypes)
                    {
                        if (i.GetInterfaces().Contains(typeof(RpcInterface)))
                        {
                            rInt = (Activator.CreateInstance(i) as RpcInterface);
                            break;
                        }
                    }

                    RpcServer rSvr = new RpcServer();
                    rSvr.Init(new IPEndPoint(IPAddress.Any, rPort), rInt);
                }

                /// 注册RpcClient服务
            }
            catch(Exception e)
            {
                Log.Error("[Script][Game][Execute], {0}.", e);
            }
        }
    }
}
