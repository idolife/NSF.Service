using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using NSF.Share;
using NSF.Interface;
using NSF.Framework.Svc;

namespace NSF.Http.Billing
{
    /// <summary>
    /// Master服务对象。
    /// </summary>
    public class HttpBillingService : IScript
    {


        /// <summary>
        /// 初始化并运行Master所提供的服务。
        /// </summary>
        public Task ExecuteAsync(Object rtmParam, Object confParam)
        {
            Log.Debug("[Billing][Execute], Param={0}.", confParam);

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
        /// "billing"路径的服务处理。
        /// </summary>
        private async Task Svc(HttpListenerResponse resp, String context)
        {
            /// 无需保留Response对象
            resp.Close();
            resp = null;

            /// 解析数据
            /// (receipt=BASE64&sign=STRING&pushInfo=BASE64)
            /// (sign=md5(receipt+pushInfo)
            String[] fullInfo = context.Split('&');
            if (fullInfo.Length < 3)
            {
                Log.Error("[BILLING][PARSE], Context data is not 3+ parts composed by '&'.");
                return;
            }

            /// 
            String receiptInfo = null;
            String signInfo = null;
            String pushInfo = null;
            /// 遍历所有数据部分
            foreach(var v in fullInfo)
            {
                Int32 eq = v.IndexOf('=');
                if (eq == -1)
                {
                    Log.Error("[BILLING][PARSE], ({0}), Context part is not composed by '='.", v);
                    return;
                }

                String key = v.Substring(0, eq);
                String val = v.Substring(eq + 1);
                Log.Debug("[BILLING][PARSE], KEY={0}, VAL={1}", key, val);
                /// 鉴别部分所属
                switch(key.ToLower())
                {
                    case "receipt":
                        receiptInfo = val;
                        break;
                    case "sign":
                        signInfo = val;
                        break;
                    case "pushinfo":
                        pushInfo = val;
                        break;
                }///鉴别部分所属
            }///遍历所有数据部分      
             ///

            /// 检查数据
            if (String.IsNullOrEmpty(receiptInfo) 
                ||
                String.IsNullOrEmpty(signInfo) 
                ||
                String.IsNullOrEmpty(pushInfo))
            {
                Log.Error("[BILLING][PARSE], {0}|{1}|{2}, Invalid data.", receiptInfo, signInfo, pushInfo);
                return;
            }
            
            /// TODO：验证MD5
           
            /// 调用逻辑处理
            await HandlePushBilling(receiptInfo, pushInfo);
        }/// 处理订单推送
         /// 

        /// <summary>
        /// 订单推送的逻辑处理。
        /// </summary>
        private Task HandlePushBilling(String receipt, String push)
        {
            ///
            return Task.FromResult(0);
        }///订单推送的逻辑处理
    }
}
