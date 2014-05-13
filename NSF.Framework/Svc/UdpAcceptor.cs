using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using NSF.Share;

namespace NSF.Framework.Svc
{
    /// <summary>
    /// 开启UDP服务并处理UDP包的接收。
    /// </summary>
    public class UdpAcceptor
    {
        class SvcNode
        {
            public Func<Byte[], IPEndPoint, Task> F;
            public String Name;
        };
        ConcurrentBag<SvcNode> _Service = new ConcurrentBag<SvcNode>();

        public void Init(Int32 port)
        {
            UdpClient listener = new UdpClient(port);
            Log.Info("[UDP-SVC][INIT], UdpListener is start at :");
            Log.Info("\t{0}", listener.Client.LocalEndPoint);

            /// 开启侦听主循环线程
            Task.Run(async () => await Svc(listener))
                .ContinueWith(OnListenerException, TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>
        /// 注册一个服务处理。
        /// </summary>
        public void RegisterService(String name, Func<Byte[], IPEndPoint, Task> f)
        {
            _Service.Add(new SvcNode { F = f, Name = name});
            Log.Info("[UDP-SVC][REG], [{0}:{1,8:X8}], Servcie is registered.", name, f.GetHashCode());
        }

        /// <summary>
        /// 循环获取远程UDP数据包。
        /// </summary>
        private async Task Svc(UdpClient listener)
        {
            while (true)
            {
                try
                {
                    UdpReceiveResult result = await listener.ReceiveAsync();

                    /// 分派/处理接收的数据
#pragma warning disable 4014
                    Task.Run(() => Proc(result))
                        .ContinueWith(OnClientException, TaskContinuationOptions.OnlyOnFaulted);
#pragma warning restore 4014
                }
                catch(Exception e)
                {
                    Log.Debug("[UDP-SVC][SVC], UdpListener exception:{0}", e);
                }
            }
        }

        /// <summary>
        /// 分派/处理接收的数据。
        /// </summary>
        private void Proc(UdpReceiveResult result)
        {
            Log.Debug("[UDP-SVC][PROC], {0}|{1}.", result.RemoteEndPoint, result.Buffer.Length);

            //Func<Byte[], IPEndPoint, Task>[] handlers = _Service.ToArray();
            foreach (var n in _Service)
            {
                n.F(result.Buffer, result.RemoteEndPoint);
            }
        }

        /// <summary>
        /// 侦听器未处理异常回调。
        /// </summary>
        private void OnListenerException(Task t)
        {
            Exception e = t.Exception;
            Log.Debug("[UDP-SVC][PROC], Exception unhandled in listener task: {0}", e);
        }

        /// <summary>
        /// 数据处理器未处理异常回调。
        /// </summary>
        private void OnClientException(Task t)
        {
            Exception e = t.Exception;
            Log.Debug("[UDP-SVC][PROC], Exception unhandled in handler task: {0}", e);
        }
    }
}
