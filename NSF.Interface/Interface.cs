using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSF.Interface
{
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


}
