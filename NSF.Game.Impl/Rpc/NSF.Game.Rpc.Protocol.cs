using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSF.Game.Impl
{
    public class RpcBillingNotifReq
    {
        public String UUID;
        public String Receipt;
        public String Push;
    }

    public class RpcBillingNotifAck
    {
        public String UUID;
    }
}
