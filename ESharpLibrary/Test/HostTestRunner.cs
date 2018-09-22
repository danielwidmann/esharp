using Es.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ESharpLibrary.Test
{
    class HostTestRunner
    {
        public static void Run(TypeInfo type, string board = "native")
        {
            var dll = type.Assembly.Location;
            string result = "";
            string[] parameters = new string[] {
                String.Format("{0}", dll),
                String.Format("-e{0}", type.Name),
                String.Format("-tPCVS"),
                "--override",
                //"--verbose",
                };

            //-tArduino
            int ret;
            if (Debugger.IsAttached)
            {
                //ret = ECS_Main.Main(parameters);
                ret = 1;
            }
            else
            {
                parameters = parameters.Select(x => x.Contains(' ') ? '"' + x + '"' : x).ToArray();
                string parameter = string.Join(" ", parameters);
                ret = ProcessRunner.RunAndStoreOutput(
                    "ESharp",
                    parameter,
                    out result,
                    null/*working dir*/);

            }

            if (ret != 0)
            {
                throw new Exception("Error Running Test: " + string.Join(" ", parameters) + "\r\n" + result);
            }            
        }
    }
}
