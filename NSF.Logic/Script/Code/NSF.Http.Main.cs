using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using NSF.Share;
using NSF.Core;
using NSF.Interface;
using NSF.Framework.Svc;
using NSF.Framework.Rpc;

namespace NSF.Http.Main
{
    /// <summary>
    /// Master服务对象。
    /// </summary>
    public class MasterServer : IScript
    {


        /// <summary>
        /// 初始化并运行Master所提供的服务。
        /// </summary>
        public async Task ExecuteAsync(Object ___, Object confParam)
        {
            Log.Debug("[Master][Execute], Param={0}.", confParam);
            //class JParam
            //{
            //    public List<String> Prefixes;
            //    public List<String> Services;
            //}

            try
            {
                JObject jParam = confParam as JObject;

                /// 开启HTTP服务
                HttpAcceptor httpSvc = new HttpAcceptor();
                var prefies = jParam.GetValue("Prefixes").ToObject<String[]>();
                httpSvc.Init(prefies);

                /// 注册HTTP服务
                foreach(var v in jParam.GetValue("Services").ToObject<String[]>())
                {
                    await MgrScript.Instance.ExecuteAsync(v, httpSvc);
                }

                /// 注册远端RPC
                List<JObject> jRpcProxies = jParam.GetValue("RpcProxies").ToObject<List<JObject>>();
                foreach (var jRpc in jRpcProxies)
                {
                    String addr = jRpc.GetValue("Addr").ToObject<String>();
                    Int32 port = jRpc.GetValue("Port").ToObject<Int32>();
                    RpcService.Default.RegisterRPC(addr, port, String.Format("{0}:{1}", addr, port));
                }                
            }
            catch(Exception e)
            {
                Log.Error("[Master][Execute], {0}.", e);
            }
        }
    }
}
