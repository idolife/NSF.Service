using System;
using System.IO;
using System.Linq;
using System.Text;
using System.CodeDom;
using Microsoft.CSharp;
using System.Reflection;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NSF.Share;
using NSF.Interface;

namespace NSF.Core
{
    public class MgrScript
    {
        public class JsonSetting
        {
            public String Version;
            public Object Param;
            public List<String> Reference = new List<String>();
        }

        public class CompliedScriptAssembly
        {
            public JsonSetting Setting { get; private set; }
            public DateTime CompliedDate { get; private set; }
            public Assembly CompliedCode { get; private set; }

            public CompliedScriptAssembly(JsonSetting json, Assembly code)
            {
                Setting = json;
                CompliedCode = code;
                CompliedDate = DateTime.Now;
            }
        }

        ConcurrentDictionary<String, CompliedScriptAssembly> _CodeCache = new ConcurrentDictionary<String, CompliedScriptAssembly>();

        MgrScript() { }
        static MgrScript _Instance;
        public static MgrScript Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new MgrScript();
                return _Instance;
            }
        }

        protected async Task<bool> LoadScriptAsync(String assName)
        {
            bool loadOK = false;
            try 
            {
                ///
                String fileJsonPath = "./Script/Json/" + assName + ".json";
                String fileJsonCode;
                using (StreamReader read = File.OpenText(fileJsonPath))
                {
                    fileJsonCode = await read.ReadToEndAsync();
                }

                JsonSetting jsonScriptData =
                    JsonConvert.DeserializeObject<JsonSetting>(fileJsonCode);

                ///
                CSharpCodeProvider cp = new CSharpCodeProvider();
                CompilerParameters cm = new CompilerParameters();
                foreach (var r in jsonScriptData.Reference)
                    cm.ReferencedAssemblies.Add(r);
                cm.GenerateExecutable = false;
                cm.GenerateInMemory = true;

                ///
                String fileScriptPath = "./Script/Code/" + assName + ".cs";
                String fileScriptCode;
                using(StreamReader read = File.OpenText(fileScriptPath))
                {
                    fileScriptCode = await read.ReadToEndAsync();
                }

                Log.Debug("[MgrScript][Load], [{0}], Load script.", assName);
                CompilerResults cr = cp.CompileAssemblyFromSource(cm, fileScriptCode);
                if (cr.Errors.HasErrors)
                {
                    Log.Error("[MgrScript][Load], [{0}], Load failed:", assName);
                    foreach (CompilerError err in cr.Errors)
                    {
                        Log.Error("\t {0}", err);
                    }
                }
                else
                {
                    Assembly ab = cr.CompiledAssembly;
                    Log.Debug("[MgrScript][Load], [{0}|{1}], Load success.", assName, jsonScriptData.Version);
                    CompliedScriptAssembly csa = new CompliedScriptAssembly(jsonScriptData, ab);
                    ///                    
                    if (_CodeCache.TryAdd(assName, csa) == false)
                    {
                        CompliedScriptAssembly ___;
                        _CodeCache.TryRemove(assName, out ___);
                        if (_CodeCache.TryAdd(assName, csa) == false)
                        {
                            Log.Error("[MgrScript][Load], [{0}|{1}, Add assembly failed.", assName, jsonScriptData.Version);
                        }
                        else
                        {
                            loadOK = true;
                        }
                    }
                    else
                    {
                        loadOK = true;
                    }
                }
            }
            catch(Exception e)
            {
                Log.Error("[MgrScript][Load], [{0}], {1}.", assName, e);
            }

            ///
            return loadOK;
        }

        protected async Task<CompliedScriptAssembly> RequireLoadedAssemblyAsync(String assName)
        {
            CompliedScriptAssembly ass = null;
            if (!_CodeCache.TryGetValue(assName, out ass))
            {
                if (! await LoadScriptAsync(assName))
                    return null;
            }
            if (ass == null)
            {
                if (!_CodeCache.TryGetValue(assName, out ass))
                {
                    Log.Debug("[MgrScript][Execute], [{0}], Assembly not loaded.", assName);
                    return null;
                }
            }

            ///
            return ass;
        }

        public async Task ExecuteAsync(String assName, Object rtmParam = null)
        {
            CompliedScriptAssembly ass = await RequireLoadedAssemblyAsync(assName);
            if (ass == null)
                return;

            IScript instance = null;
            foreach (var v in ass.CompliedCode.ExportedTypes)
            {
                if (v.GetInterfaces().Contains(typeof(IScript)))
                {
                    instance = (Activator.CreateInstance(v) as IScript);
                    break;
                }
            }

            if (instance == null)
            {
                Log.Error("[MgrScript][Execute], [{0}|{1}], Assembly instance failed.", assName, typeof(IScript));
                return;
            }
            await instance.ExecuteAsync(rtmParam, ass.Setting.Param);
        }
    }
}
