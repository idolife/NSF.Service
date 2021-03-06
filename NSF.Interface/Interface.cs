﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSF.Interface
{
    /// <summary>
    /// 普通脚本接口。
    /// </summary>
    public interface IScript
    {
        /// <summary>
        /// 脚本调用函数。
        /// </summary>
        /// <param name="rtmPrama">运行时参数。</param>
        /// <param name="confParam">脚本配置参数。</param>
        /// <returns></returns>
        Task ExecuteAsync(object rtmPrama, object confParam);
    }

    /// <summary>
    /// 普通模块接口。
    /// </summary>
    public  interface IModule
    {
        String Name { get; }
        bool Command(String cmd, Object target);
    }

    public interface IDataBlock
    {
        Int32 ReadPosition { get; }
        Int32 WritePosition { get; }
        Int32 ReadOffset(Int32 offset);
        Int32 WriteOffset(Int32 offset);
        Int32 Total { get; }
        Int32 Length { get; }
        Int32 Space { get; }
        Byte[] Buffer { get; }
        void Crunch();
        void Reset();
    }

    /// <summary>
    /// TCP连接逻辑实现接口。
    /// </summary>
    public interface IClientImpl
    {
        /// <summary>
        /// 身份ID。
        /// </summary>
        Int64 UUID { get; }
        /// <summary>
        /// 连接就绪事件。
        /// </summary>
        Task OnReady(IClientSvc cli);

        /// <summary>
        /// 数据包到达事件。
        /// </summary>
        Task OnData(IDataBlock chunk);

        /// <summary>
        /// 发生异常事件。
        /// </summary>
        Task OnException();
    }

    /// <summary>
    /// TCP连接功能提供接口。
    /// </summary>
    public interface IClientSvc
    {
        /// <summary>
        /// 身份ID。
        /// </summary>
        Int64 UUID { get; }
        /// <summary>
        /// 远端IP地址。
        /// </summary>
        String RemoteIP { get; }
        /// <summary>
        /// 外发数据。
        /// </summary>
        Task SendData(Byte[] buff, Int32 offset, Int32 length);
        /// <summary>
        /// 关闭连接。
        /// </summary>
        void Close();
    }
}
