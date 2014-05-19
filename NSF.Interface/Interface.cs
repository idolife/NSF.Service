using System;
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

    ///// <summary>
    ///// 协议提供者接口。
    ///// </summary>
    //public interface IProtocolProvider
    //{
    //    /// <summary>
    //    /// 根据消息的自定义ID获得消息的运行时类型。
    //    /// </summary>
    //    /// <param name="msgId">消息ID。</param>
    //    /// <returns>消息的运行时类型。</returns>
    //    Type GetTypeById(Int32 msgId);
    //    /// <summary>
    //    /// 根据消息的运行时获得消息的自定义ID。
    //    /// </summary>
    //    /// <param name="msgType">消息的运行时类型。</param>
    //    /// <returns>消息的自定义ID。</returns>
    //    Int32 GetIdByType(Type msgType);
    //    /// <summary>
    //    /// 解包处理。
    //    /// </summary>
    //    /// <param name="chunk">数据包对象。</param>
    //    /// <returns>解包成功返回具体数据对象，解包异常抛出异常，数据不完整返回null。</returns>
    //    Object DecodeMessage(IDataBlock chunk);
    //    /// <summary>
    //    /// 打包处理。
    //    /// </summary>
    //    /// <param name="msgId">消息对象编号。</param>
    //    /// <param name="msgObj">包含数据的消息对象。</param>
    //    /// <returns>成功返回数据流对象，打包异常抛出异常，其他返回null。</returns>
    //    Byte[] EncodeMessage(Int32 msgId, Object msgObj);
    //}
}
