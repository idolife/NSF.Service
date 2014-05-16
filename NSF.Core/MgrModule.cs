using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSF.Interface;
using NSF.Share;

namespace NSF.Core
{
    /// <summary>
    /// 逻辑模块管理对象。
    /// </summary>
    public class MgrModule
    {
        /// <summary>
        /// 管理的模块仓库。
        /// </summary>
        ConcurrentDictionary<String, IModule> _Repo = new ConcurrentDictionary<String,IModule>();

        /// <summary>
        /// 单件访问逻辑。
        /// </summary>
        MgrModule() { }
        static MgrModule _Instance;
        public static MgrModule Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new MgrModule();
                return _Instance;
            }
        }

        /// <summary>
        /// 管理指定模块。
        /// </summary>
        public bool Join(IModule mod)
        {
            if (!_Repo.TryAdd(mod.Name, mod))
            {
                Log.Error("[MgrModule][Lock], [Name:{0}], Already has such module.", mod.Name);
                return false;
            }
            Log.Debug("[MgrModule][Lock], [Name:{0}], Module has managered.", mod.Name);
            return true;
        }

        /// <summary>
        /// 移除管理指定模块。
        /// </summary>
        public bool Leave(String name)
        {
            IModule mod;
            if (!_Repo.TryRemove(name, out mod))
            {
                Log.Error("[MgrModule][Free], [Name:{0}], Module does not exist.", name);
                return false;
            }
            Log.Debug("[MgrModule][Free], [Name:{0}], Module has removed.", name);
            return false;
        }

        /// <summary>
        /// 检索指定模块。
        /// </summary>
        public IModule Find(String name)
        {
            IModule mod;
            _Repo.TryGetValue(name, out mod);
            return mod;
        }
    }
}
