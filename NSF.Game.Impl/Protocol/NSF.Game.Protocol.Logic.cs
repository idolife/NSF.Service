using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSF.Share;
using NSF.Interface;

namespace NSF.Game.Logic
{
    public class ProtocolLogic
    {
        enum ClientState
        {
            UNKOWN = 1,///初始状态
            READY = 2, ///已经验证
        }
        class ClientSvc
        {
            public ClientState NeedState;
            public Func<Object, Task> HandleSvc;
        }

        /// <summary>
        /// 当前状态。
        /// </summary>
        ClientState _State;
        /// <summary>
        /// 关联的底层连接服务对象。
        /// </summary>
        IClientSvc _Client;
        /// <summary>
        /// 消息服务表。
        /// </summary>
        Dictionary<Int32, ClientSvc> _SvcDic = new Dictionary<Int32, ClientSvc>();

        public ProtocolLogic(IClientSvc client)
        {
            /// 设置状态
            _State = ClientState.UNKOWN;
            /// 绑定底层连接
            _Client = client;

            /// 创建服务逻辑表格
            _SvcDic.Add(ProtocolCommand.MSG_LOGIN_REQ, new ClientSvc { NeedState = ClientState.UNKOWN, HandleSvc = HandleLoginReq });
        }

        public async Task HandleMessage(Int32 msgId, Object jsonMsg)
        {
            if (!_SvcDic.ContainsKey(msgId))
            {
                Log.Error("[Agent][HandleMessage], [MID:{0}], Not svc register for this message.")
                return;
            }

            ClientSvc mSvc = _SvcDic[msgId];
            if (_State != mSvc.NeedState)
            {
                Log.Error("[Agent][HandleMessage], [MID:{0}], Not match state for svc.")
                return;
            }

            /// 调用实际处理
            await mSvc.HandleSvc(jsonMsg);
        }

        private async Task SendMessage(Int32 msgId, Object jsonRaw)
        {
            /// 装配完整消息
            JsonHeader jsonFull = new JsonHeader
            {
                Id = msgId,
                Msg = jsonRaw,
            };
            /// 打包完整消息
            ArraySegment<Byte> msgFull = ProtocollProvide.EncodeMessage(jsonFull);
            await _Client.SendData(msgFull.Array, msgFull.Offset, msgFull.Count);
        }

        public async Task HandleLoginReq(Object jsonRaw)
        {
            JsonLoginReq jsonReq = jsonRaw as JsonLoginReq;
            Log.Debug("[Agent][HandleLoginReq], [{0}|{1}].", jsonReq.UserId, jsonReq.Token);

            _State = ClientState.READY;

            JsonLoginAck jsonAck = new JsonLoginAck { Status = 1, Session = jsonReq.Token };
            await SendMessage(ProtocolCommand.MSG_LOGIN_ACK, jsonAck);
        }
    }
}
