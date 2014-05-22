using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSF.Share;
using NSF.Core;
using NSF.Interface;
using NSF.Framework.Base;
using NSF.Framework.Svc;
using NSF.Framework.Rpc;
using NSF.Game.Logic;

namespace NSF.Robot.Idle
{
    public class RobotIdle : TcpHandler, IClientSvc, IClientImpl, IScript
    {
        /// <summary>
        /// 机器人状态
        /// </summary>
        enum RobotState
        {
            UNKOWN = 0,
            CERTIFIED = 1,
        }

        /// <summary>
        /// 机器人对象身份ID产生种子。
        /// </summary>
        static Int64 UUID_SEED = 1000000;
        /// <summary>
        /// 本机器人对象身份ID。
        /// </summary>
        Int64 _UUID;
        /// <summary>
        /// 机器人状态。
        /// </summary>
        RobotState _State;

        /// <summary>
        /// 默认构造函数。
        /// </summary>
        public RobotIdle()
        {
            /// 初始化成员
            _State = RobotState.UNKOWN;
            _UUID = Interlocked.Increment(ref UUID_SEED);
        }

        public async Task ExecuteAsync(Object rtmParam, Object confParam)
        {
            Log.Debug("[RobotIdle][Execute], Param={0}.", confParam);
            try
            {
                ///
                TcpClient rClient = rtmParam as TcpClient;
                JObject jParam = confParam as JObject;
 
                /// 初始化底层是本对象处于运行状态
                Init(rClient);

                /// 触发连接就绪事件（逻辑）
                await OnReady(this);
            }
            catch (Exception e)
            {
                Log.Error("[RobotIdle][Execute], {0}.", e);
            }
        }

        ///--------------------------------------------------------------
        /// IClientSvc接口实现
        /// <summary>
        /// 本机器人了身份ID。
        /// </summary>
        public Int64 UUID
        {
            get { return _UUID; }
        }

        /// <summary>
        /// 本机器人的远端地址。
        /// </summary>
        public string RemoteIP
        {
            get { return Peer_.Client.RemoteEndPoint.ToString(); }
        }

        /// <summary>
        /// 外发数据包。
        /// </summary>
        public async Task SendData(byte[] buff, int offset, int length)
        {
            await Peer_.GetStream().WriteAsync(buff, offset, length);
            Log.Debug("[RobotIdle][SendData], [{0}|{1}], Data sended.", _UUID, length - offset);
        }

        public void Close()
        {
            Peer_.Close();
            Log.Debug("[RobotIdle][Close], [{0}], Finished close.", _UUID);
        }
        ///--------------------------------------------------------------

        ///--------------------------------------------------------------
        /// IClientImpl接口实现
        /// <summary>
        /// 机器人连接就绪处理。
        /// </summary>
        public async Task OnReady(IClientSvc cli)
        {
            Log.Debug("[RobotIdle][OnReady], [{0}], Robot ready.", _UUID);

            /// 发送登录请求
            JsonLoginReq jsonReq = new JsonLoginReq
            {
                UserId = _UUID.ToString(),
                Token = "",
            };
            await SendMessage(ProtocolCommand.MSG_LOGIN_REQ, jsonReq);
        }

        /// <summary>
        /// 机器人连接数据包到达。
        /// </summary>
        public async Task OnData(IDataBlock chunk)
        {
            /// 循环处理这个数据包的所有消息
            while (true)
            {
                /// 解包
                GameReq req = ProtocollProvide.DecodeMessage(chunk);
                /// 数据包不完整
                if (req == null)
                    break;

                /// 处理协议消息
                Log.Debug("[RobotIdle][OnData], [{0}][Json:{1}].", _UUID, req.Json);

                /// 反序列化Json消息
                JsonHeader jsonHead = JsonConvert.DeserializeObject<JsonHeader>(req.Json);
                await HandleMessage(jsonHead.Id, jsonHead.Msg);
            }
        }

        /// <summary>
        /// 机器人连接异常（断开连接）
        /// </summary>
        public Task OnException()
        {
            Log.Debug("[RobotIdle][OnException], [{0}].", _UUID);
            /// 重置登录状态
            _State = RobotState.UNKOWN;

            ///
            return Task.FromResult(0);
        }
        ///--------------------------------------------------------------
        ///
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
            await SendData(msgFull.Array, msgFull.Offset, msgFull.Count);
        }

        /// <summary>
        /// 处理消息协议。
        /// </summary>
        protected Task HandleMessage(Int32 msgId, Object jsonWild)
        {
            Log.Debug("[RobotIdle][HandleMessage], [{0}], [{1}|{2}].", _UUID, msgId, jsonWild);
            if (msgId == ProtocolCommand.MSG_LOGIN_ACK)
            {
                JsonLoginAck jsonAck = jsonWild as JsonLoginAck;
                Log.Debug("[RobotIdle][LoginAck], [{0}], [{1}|{2}].", _UUID, jsonAck.Status, jsonAck.Session);
                if (jsonAck.Status == JsonLoginAck.LOGIN_OK)
                {
                    _State = RobotState.CERTIFIED;
                    Log.Debug("[RobotIdle][LoginAck], [{0}], Ceritfy passed.", _UUID);
                }
                else
                {
                    Log.Debug("[RobotIdle][LoginAck], [{0}], Ceritfy failed.", _UUID);
                }
            }

            ///
            return Task.FromResult(0);
        }
    }
}
