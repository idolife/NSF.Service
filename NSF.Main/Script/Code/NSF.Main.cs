using System;
using System.Net;
using System.Threading.Tasks;
using NSF.Share;
using NSF.Interface;
using NSF.Framework.Svc;

namespace NSF.Main
{
    /// <summary>
    /// Master服务对象。
    /// </summary>
    public class MasterServer : IScript
    {
        /// <summary>
        /// 初始化并运行Master所提供的服务。
        /// </summary>
        public Task Execute(String param)
        {
            try
            {
                /// 开启HTTP服务
                HttpAcceptor httpSvc = new HttpAcceptor();
                httpSvc.Init(new String[] {"http://localhost:9090/billing/"});
                /// 注册BILLING服务处理
                /// （支持不完整的路径）            
                httpSvc.RegisterService("/billing/", ProcPushBilling);

                /// 开启RPC连接
                ///RpcService.Default.RegisterRPC(rpc.Address, rpc.Port, String.Format("{0}:{1}", rpc.Address, rpc.Port));
            }
            catch(Exception e)
            {
                Log.Error("[MASTER][INIT], {0}.", e);
            }

            ///
            return Task.FromResult(0);
        }

        /// <summary>
        /// "billing"路径的服务处理。
        /// </summary>
        private async Task ProcPushBilling(HttpListenerResponse resp, String context)
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
            return
                Task.FromResult(0);
             ///
        }///订单推送的逻辑处理
    }
}
