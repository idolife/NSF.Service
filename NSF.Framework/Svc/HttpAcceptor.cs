using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using NSF.Share;

namespace NSF.Framework.Svc
{
    /// <summary>
    /// 处理HTTP连接接收的对象。
    /// </summary>
    public class HttpAcceptor
    {
        /// <summary>
        /// 注册的服务回调函数表。
        /// </summary>
        ConcurrentDictionary<String, Func<HttpListenerResponse, String, Task>> _Service =
            new ConcurrentDictionary<String, Func<HttpListenerResponse, String, Task>>();

        /// <summary>
        /// 注册服务回调。
        /// </summary>
        public void RegisterService(String path, Func<HttpListenerResponse, String, Task> callback)
        {
            if (_Service.TryAdd(path.ToUpper(), callback))
                Log.Debug("[HTTP][REG], Register service({0}) success.", path, callback);
            else
                Log.Debug("[HTTP][REG], Register service({0}) failed.", path, callback);
        }

        /// <summary>
        /// 服务初始化函数。
        /// </summary>
        public void Init(String[] prefixes = null)
        {
            /// 确定操作系统支持
            if (!HttpListener.IsSupported)
            {
                throw new NotImplementedException("HttpListener");
            }

            /// 开启服务目录
            /// （默认开启URI:+:80）
            if (prefixes == null || prefixes.Length ==0)
            {
                prefixes = new String[]{"http://+:80/"};
            }
            
            /// 创建HTTP侦听器
            HttpListener listener = new HttpListener();
            foreach (var v in prefixes)
                listener.Prefixes.Add(v);
            listener.Start();
            Log.Info("[HTTP][INIT], HttpListener is start at :");
            foreach(var v in prefixes)
            {
                Log.Info("\t({0})", v);
            }

            /// 开启侦听主循环线程
            Task.Run(async () => await Svc(listener))
                .ContinueWith(OnListenerException, TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>
        /// 循环获取客户端的请求。
        /// </summary>
        private async Task Svc(HttpListener listener)
        {
            while (true)
            {
                try
                {
                    /// 接收一个请求
                    HttpListenerContext context = await listener.GetContextAsync();
#pragma warning disable 4014 
                    /// 创建一个处理
                    /// （独立线程）
                    Task.Run(async () => await Proc(context))
                        .ContinueWith(OnClientException, TaskContinuationOptions.OnlyOnFaulted);
#pragma warning restore 4014

                }
                catch(Exception e)
                {
                    Log.Debug("[HTTP][SVC], HttpListener exception:{0}", e);
                }
            }

            ///// 停止服务
            //listener.Stop();
        }

        /// <summary>
        /// 每请求的处理逻辑。
        /// </summary>
        private async Task Proc(HttpListenerContext http)
        {
            Log.Debug("[HTTP][PROC]---{0,08}-----------------------", http.GetHashCode());

            HttpListenerRequest request = http.Request;
            Log.Debug("[HTTP][PROC], {0}|{1}: {2}", request.RemoteEndPoint, request.HttpMethod, request.Url.AbsolutePath); 
            String context = "";
            if (request.ContentLength64 > 0)
            {   

                Stream body = request.InputStream;
                Encoding encoding = request.ContentEncoding;
                StreamReader reader = new StreamReader(body, encoding);
                context = await reader.ReadToEndAsync();
                Log.Debug("[HTTP][PROC], {0}", context);
            }

            if (request.HttpMethod == "GET")
            {
                context = http.Request.Url.Query;
                Log.Debug("[HTTP][PROC], {0}", context);
            }

            do
            {
                /// 回调注册的服务
                String path = request.Url.AbsolutePath;
                if (!path.EndsWith("/"))
                    path = path + "/";
                path = String.Format("{0}:{1}", request.HttpMethod, path);
                Func<HttpListenerResponse, String, Task> callback;
                if (_Service.TryGetValue(path.ToUpper(), out callback))
                {
                    await callback(http.Response, context);
                }
                else
                {
                    Log.Warn("[HTTP][PROC], No service register for {0}.", path);
                    http.Response.StatusCode = (Int32)HttpStatusCode.NotFound;
                    http.Response.Close();
                    break;
                }

            } while (false);
            Log.Debug("[HTTP][PROC]-----------------------{0,08}---", http.GetHashCode());
        }

        /// <summary>
        /// 侦听器未处理异常回调。
        /// </summary>
        private void OnListenerException(Task t)
        {
            Exception e = t.Exception;
            Log.Debug(String.Format("[HTTP][PROC], Exception unhandled in listener task: {0}", e.ToString()));
        }

        /// <summary>
        /// 连接处理器未处理异常回调。
        /// </summary>
        private void OnClientException(Task t)
        {
            Exception e = t.Exception;
            Log.Debug(String.Format("[HTTP][PROC], Exception unhandled in handler task: {0}", e.ToString()));
        }
    }
}
