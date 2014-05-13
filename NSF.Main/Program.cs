using System;
using System.Linq;
using System.Text;
using NSF.Share;
using NSF.Core;
using NSF.Interface;

namespace NSF.Main
{
    class Program
    {
        static void Main(string[] args)
        {
            /// 主脚本名称
            String mainScript = "NSF.Http.Main";
            if (args.Length == 1)
                mainScript = args[0];

            /// 载入主脚本编译并运行
            Log.Debug("[Main], Initialize ...");
            MgrScript.Instance.ExecuteAsync(mainScript).Wait();
            Log.Debug("[Main], Initialize done.");

            /// 等待程序退出
            (new ConsoleBreakHandler()).Wait();
        }
    }
}
