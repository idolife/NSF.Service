using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSF.Game.Logic
{
    public class JsonHeader
    {
        public Int32 Id;
        public Object Msg;
    }

    public class JsonLoginReq
    {
        public String UserId;
        public String Token;
    }

    public class JsonLoginAck
    {
        public static Int32 LOGIN_OK = 1;
        public Int32 Status;
        public String Session;
    }
}
