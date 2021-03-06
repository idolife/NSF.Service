﻿using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NSF.Framework.Base
{
    /// <summary>
    /// 并发多任务执行器。
    /// </summary>
    public class MultiTask
    {
        /// <summary>
        /// 某项执行任务的包装。
        /// </summary>
        class Actor
        {
            /// <summary>
            /// 底层等待执行的任务对象。
            /// </summary>
            public Task Work { get; private set; }
            /// <summary>
            /// 执行任务时附带的参数对象。
            /// </summary>
            public Object Object { get; private set; }
            /// <summary>
            /// 执行的任务完成时回调对象。
            /// </summary>
            public Func<Task, Object, Task> Callback { get; private set; }
            /// <summary>
            /// 默认构造函数。
            /// </summary>
            /// <param name="t">需要执行的任务。</param>
            /// <param name="o">附带的参数对象。</param>
            /// <param name="cb">任务完成时回调对象。</param>
            public Actor(Task t, Object o, Func<Task, Object, Task> cb)
            {
                Work = t;
                Object = o;
                Callback = cb;
            }
            /// <summary>
            /// 重置环境。
            /// </summary>
            /// <param name="t">需要执行的任务。</param>
            /// <param name="o">附带的参数对象。</param>
            /// <param name="cb">任务完成时回调对象。</param>
            public void Set(Task t, Object o, Func<Task, Object, Task> cb)
            {
                Work = t;
                Object = o;
                Callback = cb;
            }
        }

        /// <summary>
        /// 任务容器。
        /// </summary>
        WaitableQueue<Actor> _Jobs;
        /// <summary>
        /// 任务取消对象。
        /// </summary>
        CancellationTokenSource _CancelTokenSource;
        /// <summary>
        /// 默认构造。
        /// </summary>
        public MultiTask()
        {
            _Jobs = new WaitableQueue<Actor>();
            _CancelTokenSource = new CancellationTokenSource();
            Task.Run(async () => await Proc(), _CancelTokenSource.Token);
        }
        /// <summary>
        /// 默认协构。
        /// </summary>
        ~MultiTask()
        {
            _CancelTokenSource.Cancel();
        }
        /// <summary>
        /// 添加需执行的任务。
        /// </summary>
        /// <param name="t">需要执行的任务。</param>
        /// <param name="o">附带的参数对象。</param>
        /// <param name="cb">任务完成时回调对象。</param>
        /// <remarks>
        /// 以保护修饰主要是不希望由外部直接调用此函数，
        /// 以免造成线程模型混乱。应该由其继承对象开放新的接口并封装调之。
        /// </remarks>
        protected void Put(Task t, Object o, Func<Task, Object, Task> cb)
        {
            Actor act = new Actor(t, o, cb);
            _Jobs.Enqueue(act);
        }

        /// <summary>
        /// 执行器执行循环发生异常时回调函数。
        /// </summary>
        protected virtual Task OnException(Exception e)
        {
            return Task.FromResult(0);
        }
        /// <summary>
        /// 执行器的驱动函数。
        /// </summary>
        /// <returns></returns>
        private async Task Proc()
        {
            /// 循环逻辑异常的异常数据。
            Exception E = null;
            /// 当前的执行队列。
            List<Actor> jobWait = new List<Actor>();
            /// 创建一个新任务通知
            Task<Actor> jobNtf = _Jobs.Dequeue();
            List<Task> taskWait = new List<Task>();
            taskWait.Add(jobNtf);
            try
            {
                while (true)
                {
                    /// 等待任意一个任务完成
                    Task finishTask = await Task.WhenAny(taskWait);

                    /// 有任务完成
                    if (finishTask != jobNtf)
                    {
                        /// 关联完成的任务对象
                        Actor job = jobWait.Find(x => x.Work == finishTask);
                        if (job != null)
                        {
                            /// 不是本地回调完成实际回调
                            if (job.Callback != null)
                                await job.Callback(finishTask, job.Object);
                            /// 移除正在回调的任务
                            jobWait.Remove(job);
                        }
                    }
                    /// 有任务到达
                    else
                    {
                        /// 添加新任务到分发队列
                        Actor job = jobNtf.Result;
                        jobWait.Add(job);
                        taskWait.Add(job.Work);

                        /// 创建一个新任务通知
                        jobNtf = _Jobs.Dequeue();
                        /// 加入分发队列
                        taskWait.Add(jobNtf);
                    }

                    /// 移除当前完成的任务
                    taskWait.Remove(finishTask);
                }
            }
            /// 只要出现异常该对象就没有必要运行下去了
            catch (Exception e)
            {
                E = e;
            }

            /// 驱动逻辑最后的异常处理回调
            await OnException(E);
        }
    }
}
