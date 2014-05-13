using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using NSF.Share;

namespace NSF.Framework.Svc
{
    /// <summary>
    /// 开启Tcp服务并接收Tcp连接。
    /// </summary>
    public class TcpAcceptor
    {
        class SvcNode
        {
            public Action<TcpClient> F;
            public String Name;
        };
        ConcurrentBag<SvcNode> _Service = new ConcurrentBag<SvcNode>();

        public void Init(Int32 port)
        {
            try
            {
                TcpListener listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                Log.Info("[TCP-SVC][INIT], TcpListener is start at :");
                Log.Info("\t{0}", listener.LocalEndpoint);

                /// 开启侦听主循环线程
                Task.Run(async () => await Svc(listener))
                    .ContinueWith(OnListenerException, TaskContinuationOptions.OnlyOnFaulted);
            }
            catch(Exception e)
            {
                Log.Error("[TCP-SVC][INIT], {0}.", e);
            }
        }

        /// <summary>
        /// 注册新连接服务。
        /// </summary>
        public void RegisterSevice(String name, Action<TcpClient> f)
        {
            SvcNode node = new SvcNode { Name = name, F = f };
            _Service.Add(node);
            Log.Debug("[TCP-SVC][REG], [{0}|{1, 8:X8}], Service registered.", name, f.GetHashCode());
        }

        /// <summary>
        /// 循环接收TCP连接。
        /// </summary>
        private async Task Svc(TcpListener listener)
        {
            while (true)
            {
                try
                {
                    /// 接收远程连接
                    TcpClient client = await listener.AcceptTcpClientAsync();
#pragma warning disable 4014
                    Task.Run(() => Proc(client))
                        .ContinueWith(OnClientException, TaskContinuationOptions.OnlyOnFaulted);
#pragma warning restore 4014
                }
                catch(Exception e)
                {
                    Log.Debug("[TCP-SVC][SVC], TcpListener exception:{0}", e);
                }
            }
        }

        /// <summary>
        /// 分派/处理接收的连接。
        /// </summary>
        private void Proc(TcpClient client)
        {
            Log.Debug("[TCP-SVC][PROC], [{0}], New TcpConnection.", client.Client.LocalEndPoint);            
            foreach(var n in _Service)
            {
                n.F(client);
            }
        }

        /// <summary>
        /// 侦听器未处理异常回调。
        /// </summary>
        private void OnListenerException(Task t)
        {
            Exception e = t.Exception;
            Log.Debug("[TCP-SVC][PROC], Exception unhandled in listener task: {0}", e);
        }

        /// <summary>
        /// 数据处理器未处理异常回调。
        /// </summary>
        private void OnClientException(Task t)
        {
            Exception e = t.Exception;
            Log.Debug("[TCP-SVC][PROC], Exception unhandled in handler task: {0}", e);
        }
    }
}
