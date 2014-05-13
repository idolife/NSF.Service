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

namespace NSF.Logger.Main
{
    /// <summary>
    /// Logger服务对象。
    /// </summary>
    public class LoggerServer : IScript
    {


        /// <summary>
        /// 初始化Logger服务。
        /// </summary>
        public async Task ExecuteAsync(Object ___, Object confParam)
        {
            Log.Debug("[Script][LoggerServer][Execute], Param={0}.", confParam);

            try
            {
                JObject jParam = confParam as JObject;

                /// 开启UDP服务
                UdpAcceptor udpSvc = new UdpAcceptor();
                Int32 port = jParam.GetValue("UDP").ToObject<Int32>();
                udpSvc.Init(port);
              
                /// 注册UDP处理
                foreach (var v in jParam.GetValue("Services").ToObject<String[]>())
                {
                    await MgrScript.Instance.ExecuteAsync(v, udpSvc);
                }
            }
            catch(Exception e)
            {
                Log.Error("[Script][LoggerServer][Execute], {0}.", e);
            }
        }

    }
}
