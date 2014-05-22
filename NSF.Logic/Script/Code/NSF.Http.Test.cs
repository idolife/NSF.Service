using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using NSF.Share;
using NSF.Interface;
using NSF.Framework.Svc;

namespace NSF.Http.Test
{
    /// <summary>
    /// Master服务对象。
    /// </summary>
    public class HttpTestService : IScript
    {


        /// <summary>
        /// 初始化并运行Master所提供的服务。
        /// </summary>
        public Task ExecuteAsync(Object rtmParam, Object confParam)
        {
            Log.Debug("[Test][Execute], Param={0}.", confParam);

            try
            {
                HttpAcceptor httpSvc = rtmParam as HttpAcceptor;
                String jParam = confParam as String;

                /// 注册HTTP服务
                httpSvc.RegisterService(jParam, Svc);
            }
            catch(Exception e)
            {
                Log.Error("[Master][Execute], {0}.", e);
            }

            ///
            return Task.FromResult(0);
        }

        /// <summary>
        /// 本路径的服务处理。
        /// （/test/）
        /// </summary>
        private async Task Svc(HttpListenerResponse resp, String qData)
        {
            Log.Debug("[Test][Execute], Q.Data={0}.", qData);
            /// 故意不关闭RESPONE对象
            resp.KeepAlive = true;            
            byte[] data = System.Text.Encoding.UTF8.GetBytes("Hello World");
            resp.ContentType = "text/plain";
            resp.ContentEncoding = System.Text.Encoding.UTF8;
            resp.ContentLength64 = data.Length;
            await resp.OutputStream.WriteAsync(data, 0, data.Length);
        }/// 处理订单推送
         /// 
    }
}
