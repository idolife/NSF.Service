using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSF.Framework.Rpc;

namespace NSF.Game.Impl
{
    /// <summary>
    /// 订单RPC的处理的回调对象。
    /// </summary>
    public class RpcBillingImpl : RpcInterface
    {
        public RpcBillingImpl()
        {
            RegisterCall<RpcBillingNotifReq, RpcBillingNotifAck>(OnProtocolRpcBillingNotifReq);
        }

        /// <summary>
        /// 处理MasterServer过来的订单通知。
        /// </summary>
        protected Task<RpcBillingNotifAck> OnProtocolRpcBillingNotifReq(RpcBillingNotifReq req)
        {
            throw new NotImplementedException("OnProtocolRpcBillingNotifReq");
        }
    }
}
