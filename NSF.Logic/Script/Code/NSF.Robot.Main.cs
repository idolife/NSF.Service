using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NSF.Share;
using NSF.Core;
using NSF.Interface;
using NSF.Framework.Svc;
using NSF.Framework.Rpc;

namespace NSF.Robot.Main
{
    public class RobotManager : IScript
    {
        public Task ExecuteAsync(Object ___, Object confParam)
        {
            Log.Debug("[Script][RobotManager][Execute], Param={0}.", confParam);
            try
            {
                ///
                TcpConnector tcpBuilder = new TcpConnector();
                JObject jParam = confParam as JObject;

                /// 注册连接任务
                List<JObject> confRobot = jParam.GetValue("Robot").ToObject<List<JObject>>();
                foreach(var robot in confRobot)
                {
                    String rAddr = robot.GetValue("Remote").ToObject<String>();
                    String rSvc = robot.GetValue("Svc").ToObject<String>();
                    Int32 rCount = robot.GetValue("Count").ToObject<Int32>();
                    Int32 rConcurrent = robot.GetValue("Concurrent").ToObject<Int32>();

                    tcpBuilder.RegisterService(rAddr, rCount, rConcurrent, HandleRobotReady, rSvc);
                }

            }
            catch (Exception e)
            {
                Log.Error("[Script][RobotManager][Execute], {0}.", e);
            }

            ///
            return Task.FromResult(0);
        }

        /// <summary>
        /// 连接创建处理。
        /// </summary>
        private void HandleRobotReady(TcpClient client, String param)
        {
            Log.Debug("[Script][RobotManager][HandleRobotReady], [{0}:{1}].", client.Client.RemoteEndPoint, param);
            /// TODO:（Lauch robot script and associate blabla）
        }
    }
}
