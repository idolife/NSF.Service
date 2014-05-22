using System;
using System.Data;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using NSF.Share;
using NSF.Core;
using NSF.Interface;
using NSF.Framework.Base;
using NSF.Framework.Svc;

namespace NSF.Logger.DB
{
    /// <summary>
    /// LoggerMySql服务对象。
    /// </summary>
    public class LoggerDB : IScript
    {
        /// <summary>
        /// DB的连接字符串。
        /// </summary>
        String _ConnectionString;
        /// <summary>
        /// DB的连接对象。
        /// </summary>
        MySqlConnection _MySqlConnection;
        /// <summary>
        /// DB执行对象。
        /// </summary>
        MySqlCommand _MySqlCommand;
        /// <summary>
        /// 日志消息队列。
        /// </summary>
        WaitableQueue<String> _LogMsgQueue = new WaitableQueue<String>();

        /// <summary>
        /// 初始化LoggerDB服务。
        /// </summary>
        public Task ExecuteAsync(Object rtmParam, Object confParam)
        {
            Log.Debug("[LoggerDB][Execute], Param={0}.", confParam);

            try
            {
                /// 获取连接字符串
                JObject jParam = confParam as JObject;
                _ConnectionString = jParam.GetValue("ConnectionString").ToObject<String>();

                /// 注册UDP消息处理。
                UdpAcceptor udpSvc = rtmParam as UdpAcceptor;
                udpSvc.RegisterService("LoggerDB", HandleProc);

                /// 开启记录线程。
                Task.Run(async () => await HandleRcd());
            }
            catch(Exception e)
            {
                Log.Error("[LoggerDB][Execute], {0}.", e);
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
                String dataString = Encoding.UTF8.GetString(data);
                if (dataString != null && dataString.Length >= 8)
                    _LogMsgQueue.Enqueue(dataString);
            }
            catch(Exception e)
            {
                Log.Error("[LoggerDB][Proc], {0}.", e);
            }

            ///
            return Task.FromResult(0);
        }

        /// <summary>
        /// 处理日志消息的记录逻辑。
        /// </summary>
        private async Task HandleRcd()
        {
            StringBuilder SB_ = new StringBuilder();
            while(true)
            {
                try
                {
                    ///获得一个日志消息解析
                    ///%date`%level`[AGENT]`%message
                    ///2014-09-12`16:49:12,205`DEBUG`[AGENT]`XXXXXX
                    String logChunk = await _LogMsgQueue.Dequeue();
                    String[] logParts = logChunk.Split('`');                    
                    if (logParts.Length < 4)
                    {
                        Log.Debug("[LoggerDB][Rcd], Hack log message.", logChunk);
                        continue;
                    }
                    if (logParts.Length > 4)
                    {
                        SB_.Clear();
                        for (int i = 3; i < logParts.Length; ++i)
                            SB_.Append(logParts[i]);
                    }

                    /// 写入DB
                    if (!await EnsureDB())
                        continue;
                    if (logParts.Length > 4)
                        await WriteRcd(logParts[0], logParts[1], logParts[2], SB_.ToString());
                    else
                        await WriteRcd(logParts[0], logParts[1], logParts[2], logParts[3]);
                }
                catch(Exception e)
                {
                    Log.Debug("[LoggerDB][Rcd], {0}.", e);
                }
            }
        }

        private async Task<bool> EnsureDB()
        {
            try
            {            
                if (_MySqlConnection == null)
                {
                    MySqlConnection connection = new MySqlConnection(_ConnectionString);
                    await connection.OpenAsync();
                    MySqlCommand charset = new MySqlCommand("set names utf8", connection);
                    await charset.ExecuteNonQueryAsync();
                    MySqlCommand command = new MySqlCommand(@"INSERT INTO `logger`.`me_log` (`log_module`, `log_datetime`, `log_level`, `log_message`) VALUES (@mod, @dat, @lvl, @msg)", connection);
                    _MySqlConnection = connection;
                    _MySqlCommand = command;
                }                
                else
                {
                    if (false == _MySqlConnection.Ping())
                    {
                        await _MySqlConnection.CloseAsync();
                        _MySqlConnection = null;
                        _MySqlCommand = null;
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Debug("[LoggerDB][Rcd], {0}.", e);
                _MySqlConnection = null;
                _MySqlCommand = null;
                return false;
            }
            return true;
        }

        private async Task WriteRcd(String logDate, String logLevel, String logModule, String logMsg)
        {
            try
            {
                _MySqlCommand.Parameters.Clear();
                _MySqlCommand.Parameters.AddWithValue("@mod", logModule);
                _MySqlCommand.Parameters.AddWithValue("@dat", logDate);
                _MySqlCommand.Parameters.AddWithValue("@lvl", logLevel);
                _MySqlCommand.Parameters.AddWithValue("@msg", Encoding.UTF8.GetBytes(logMsg));
                await _MySqlCommand.ExecuteNonQueryAsync();

                Log.Info("[LoggerDB][Rcd] --- {0, 8}|{1}|{2, 5}\n  {3}", logModule, logDate, logLevel, logMsg);
            }
            catch (Exception e)
            {
                Log.Debug("[LoggerDB][Rcd], {0}.", e);
            }
        }
    }
}
