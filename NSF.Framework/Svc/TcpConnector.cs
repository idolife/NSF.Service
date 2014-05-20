using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NSF.Share;
using NSF.Framework.Base;

namespace NSF.Framework.Svc
{
    /// <summary>
    /// TCP连接器对象。
    /// </summary>
    public class TcpConnector : MultiTask
    {
        /// <summary>
        /// 连接任务信息。
        /// </summary>
        class JobConnectItem
        {
            public IPEndPoint RemoteAddr;
            /// <summary>
            /// 需要创建的连接总数目。
            /// </summary>
            public Int32 TotalCount;
            /// <summary>
            /// 已经创建的连接数目。
            /// </summary>
            public Int32 FinishCount;
            /// <summary>
            /// 已经计划的连接数目。
            /// </summary>
            public Int32 ScheduleCount;
            /// <summary>
            /// 最大并发连接的数目。
            /// </summary>
            public Int32 ConcorrentCount;
            /// <summary>
            /// 连接完成时需要调用的回调。
            /// </summary>
            public Action<TcpClient, String> FinishSvc;
            /// <summary>
            /// 回调参数（String）
            /// </summary>
            public String SvcParam;
        }
        
        class JobConnectPack
        {
            public JobConnectItem Job;
            public TcpClient Client;
        }

        /// <summary>
        /// 注册连接服务。
        /// </summary>
        public void RegisterService(String remote, Int32 count, Int32 concorrent, Action<TcpClient, String> svcFunc, String svcParam)
        {
            ////
            try
            {
                /// 解析远端地址
                String[] addrParts = remote.Split(':');
                IPEndPoint remoteAddr = new IPEndPoint(IPAddress.Parse(addrParts[0]), Int32.Parse(addrParts[1]));
                /// 创建连接工作项
                JobConnectItem jobItem = new JobConnectItem
                {
                    RemoteAddr = remoteAddr,
                    TotalCount = count,
                    FinishCount = 0,
                    ScheduleCount = 0,
                    ConcorrentCount = concorrent,
                    FinishSvc = svcFunc,
                    SvcParam = svcParam,
                };

                /// 发起最大并发个连接任务
                for(Int32 i = 1; i <= jobItem.ConcorrentCount; ++i)
                {
                    /// 创建连接任务
                    TcpClient client = new TcpClient();
                    Task connectTask = client.ConnectAsync(jobItem.RemoteAddr.Address, jobItem.RemoteAddr.Port);
                    Put(connectTask, new JobConnectPack { Job = jobItem, Client = client }, HandleFinish);

                    /// 更改连接工作数据
                    Interlocked.Increment(ref jobItem.ScheduleCount);
                }
            }
            catch(Exception e)
            {
                Log.Debug("[TcpConnector][RegisterService], {0}.", e);
            }
        }

        /// <summary>
        /// 连接任务完成处理。
        /// </summary>
        private async Task HandleFinish(Task finishTask, Object finishObject)
        {
            try
            {
                ///
                JobConnectPack finishJob = finishObject as JobConnectPack;
                JobConnectItem jobItem = finishJob.Job;

                /// 连接正常完成
                await finishTask;
                /// 更新连接工作数据
                Interlocked.Increment(ref jobItem.FinishCount);
                Interlocked.Decrement(ref jobItem.ScheduleCount);
                /// 回调连接建立（单独线程与连接任务无关）
#pragma warning disable 4014
                Task.Run(() => jobItem.FinishSvc(finishJob.Client, jobItem.SvcParam));
#pragma warning restore 4014

                /// 还有剩余连接未创建
                if (jobItem.FinishCount < jobItem.TotalCount)
                {
                    /// 继续发起剩余的连接任务
                    Interlocked.Increment(ref jobItem.ScheduleCount);
                    /// 可以发起
                    if (jobItem.ScheduleCount <= jobItem.ConcorrentCount)
                    {
                        /// 创建连接任务
                        TcpClient client = new TcpClient();
                        Task connectTask = client.ConnectAsync(jobItem.RemoteAddr.Address, jobItem.RemoteAddr.Port);
                        Put(connectTask, new JobConnectPack { Job = jobItem, Client = client }, HandleFinish);
                    }
                    /// 撤销发起（可能由其他任务发起了）
                    else
                    {
                        Interlocked.Decrement(ref jobItem.ScheduleCount);
                    }
                }
            }
            catch(Exception e)
            {
                Log.Debug("[TcpConnector][HandleFinish], {0}.", e);
            }
        }
    }
}
