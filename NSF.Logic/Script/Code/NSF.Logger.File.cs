using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NSF.Share;
using NSF.Core;
using NSF.Interface;
using NSF.Framework.Svc;

namespace NSF.Logger.File
{
    /// <summary>
    /// LoggerFile服务对象。
    /// </summary>
    public class LoggerFile : IScript
    {
        /// <summary>
        /// 初始化LoggerFile服务。
        /// </summary>
        public Task ExecuteAsync(Object rtmParam, Object confParam)
        {
            Log.Debug("[LoggerFile][Execute], Param={0}.", confParam);

            try
            {
                UdpAcceptor udpSvc = rtmParam as UdpAcceptor;
                udpSvc.RegisterService("LoggerFile", HandleProc);
            }
            catch(Exception e)
            {
                Log.Error("[LoggerFile][Execute], {0}.", e);
            }

            ///
            return Task.FromResult(0);
        }

        /// <summary>
        /// 处理UDP数据包到达逻辑。
        /// </summary>
        public Task HandleProc(Byte[] data, IPEndPoint remote)
        {
            try
            {
                /// 获得UDP数据包的字符串格式
                ///[AGENT] %date %level %message
                ///[AGENT] 2014-04-25 14:38:50,243 DEBUG Message begin from here ...
                String dataString = Encoding.UTF8.GetString(data);
                //String cServer = dataString.Substring(0, 7);
                //String cDate = dataString.Substring(8, 23);
                //String cLevel = dataString.Substring(32, 5);
                //String cMsg = dataString.Substring(38);
                //Console.WriteLine("------");
                //Console.WriteLine("{0}", cServer);
                //Console.WriteLine("{0}", cDate);
                //Console.WriteLine("{0}", cLevel);
                //Console.WriteLine("{0}", cMsg);
                //Console.WriteLine("------");
                Log.Debug("{0}", dataString);
            }
            catch(Exception e)
            {
                Log.Error("[LoggerFile][Proc], {0}.", e);
            }

            ///
            return Task.FromResult(0);
        }
    }
}
