using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using NSF.Share;
using NSF.Framework.Base;

namespace NSF.Framework.Svc
{
    /// <summary>
    /// TCP连接逻辑处理对象。
    /// </summary>
    public class TcpHandler : MultiTask
    {
        /// <summary>
        /// 读缓存最大长度。
        /// </summary>
        public static Int32 READ_BUFFER_SIZE = 1024 * 4;
        /// <summary>
        /// 写缓存最大长度。
        /// </summary>
        public static Int32 WRITE_BUFFER_SIZE = 1024 * 4;
        /// <summary>
        /// 默认构造函数。
        /// </summary>
        public TcpHandler(TcpClient client)
        {
            Log.Debug("[TcpHandler][Ctor], Begin.");
            /// 保存底层SOCKET对象
            Peer_ = client;

            /// 发起第一个读请求
            PostRead();
            Log.Debug("[TcpHandler][Ctor], End.");
        }

        /// <summary>
        /// 无参数构造函数必须与Init配合使用。
        /// </summary>
        public TcpHandler() {}

        /// <summary>
        /// 初始化对象。
        /// </summary>
        public void Init(TcpClient client)
        {
            Log.Debug("[TcpHandler][Init], Begin.");

            /// 保存底层SOCKET对象
            Peer_ = client;

            /// 发起第一个读请求
            DataBlock chunk = new DataBlock(READ_BUFFER_SIZE);
            Task<Int32> readTask =
                Peer_.GetStream().ReadAsync(chunk.Buffer, chunk.WritePosition, chunk.Space);
            Put(readTask, chunk, OnRead);


            Log.Debug("[TcpHandler][Init], End.");
        }

        protected Task PostRead(DataBlock chunk = null)
        {
            Log.Debug("[TcpHandler][PostRead], Begin.");

            /// 准备数据缓存对象
            if (chunk == null)
                chunk = new DataBlock(READ_BUFFER_SIZE);
            else
                chunk.Crunch();

            /// 计划读任务
            Task<Int32> readTask =
                Peer_.GetStream().ReadAsync(chunk.Buffer, chunk.WritePosition, chunk.Space);
            Put(readTask, chunk, OnRead);
            ///
            Log.Debug("[TcpHandler][PostRead], End.");
            return Task.FromResult(0);
        }

        /// <summary>
        /// 处理数据包读取完成。
        /// </summary>
        protected Task OnRead(Task finishTask, Object finishData)
        {
            Log.Debug("[TcpHandler][OnRead], Begin.");
            Exception raiseException = null;
            
            /// 
            do
            {
                DataBlock chunk = finishData as DataBlock;
                /// 使用异常促使底层循环退出
                Task<Int32> recvTask = finishTask as Task<Int32>;
                /// 完成接收任务
                Int32 recvLen = recvTask.Result;
                if (recvLen == 0)
                    raiseException = new SocketException();

                /// 更新接收缓存
                chunk.WriteOffset(recvLen);
                Log.Debug("[TcpHandler][OnRead], Recv data = {0}.", recvLen);

                /// 创建逻辑处理任务
                Task procTask = OnData(chunk);
                Put(procTask, chunk, OnFinish);
            }
            while (false);

            /// 什么也不要做
            Log.Debug("[TcpHandler][OnRead], End.");
            /// 保存异常或者返回一个已经完成的任务
            if (raiseException != null)
                throw raiseException;
            return Task.FromResult(0);
        }

        /// <summary>
        /// 处理数据包到达逻辑。
        /// </summary>
        protected virtual Task OnData(DataBlock chunk)
        {
            Log.Debug("[TcpHandler][OnData], Begin.");

            Log.Debug("[TcpHandler][OnData], End.");
            return
                Task.FromResult(0);
        }

        /// <summary>
        /// 处理数据处理完成逻辑。
        /// </summary>
        protected Task OnFinish(Task finishTask, Object finishData)
        {
            Log.Debug("[TcpHandler][OnFinish], Begin.");

            /// 还原
            DataBlock chunk = finishData as DataBlock;
            /// 发起下一个读请求            
            var ___ = PostRead(chunk);
            Log.Debug("[TcpHandler][OnFinish], End.");

            ///
            return ___;
        }

        ////
        protected TcpClient Peer_;        
    }   
}
