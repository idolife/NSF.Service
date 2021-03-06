﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSF.Share;
using NSF.Interface;

namespace NSF.Game.Logic
{
    public class ProtocolLogic
    {
        enum ClientState
        {
            UNKOWN = 1,///初始状态
            CERTIFIED = 2, ///已经验证
        }

        enum ClientCont
        {
            CERTIFY_TIMEOUT = 5000,///认证超时（5秒）
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
        Dictionary<Int32, Func<JObject, Task>> _SvcDic = new Dictionary<Int32, Func<JObject, Task>>();

        public ProtocolLogic(IClientSvc client)
        {
            /// 设置状态
            _State = ClientState.UNKOWN;
            /// 绑定底层连接
            _Client = client;
            /// 创建一个认证超时任务

            /// 创建服务逻辑表格
            _SvcDic.Add(ProtocolCommand.MSG_LOGIN_REQ, HandleLoginReq);
        }

        public async Task HandleMessage(Int32 msgId, JObject jsonMsg)
        {
            Log.Debug("[Agent][HandleMessage],  [{0}], [{1}|{2}].", _Client.UUID, msgId, jsonMsg);
            if (!_SvcDic.ContainsKey(msgId))
            {
                Log.Error("[Agent][HandleMessage],  [{0}], [MID:{0}], Not svc register for this message.", _Client.UUID, msgId);
                return;
            }

            /// 调用实际处理
            await _SvcDic[msgId](jsonMsg);
        }

        private async Task SendMessage(Int32 msgId, Object jsonWild)
        {
            /// 装配完整消息
            JsonHeader jsonFull = new JsonHeader
            {
                Id = msgId,
                Msg = jsonWild,
            };
            /// 打包完整消息
            ArraySegment<Byte> msgFull = ProtocollProvide.EncodeMessage(jsonFull);
            await _Client.SendData(msgFull.Array, msgFull.Offset, msgFull.Count);
        }

        public async Task HandleLoginReq(JObject jsonWild)
        {
            JsonLoginReq jsonReq = jsonWild.ToObject<JsonLoginReq>();
            Log.Debug("[Agent][HandleLoginReq], [{0}], [{1}|{2}].", _Client.UUID, jsonReq.UserId, jsonReq.Token);

            do
            {
                /// 如果已经通过验证则无需处理
                if (_State != ClientState.UNKOWN)
                    break;

                /// TODO：进行验证（UID+TOKEN, 唯一登录）
                _State = ClientState.CERTIFIED;
                Log.Info("[Agent][HandleLoginReq], [{0}], [{1}], Certified done.", _Client.UUID, jsonReq.UserId);

            } while (false);

            JsonLoginAck jsonAck = new JsonLoginAck { Status = JsonLoginAck.LOGIN_OK, Session = jsonReq.Token };
            await SendMessage(ProtocolCommand.MSG_LOGIN_ACK, jsonAck);
            Log.Debug("[Agent][HandleLoginReq], [{0}], [{1}], Login ack sended.", _Client.UUID, jsonReq.UserId);
        }
    }
}
