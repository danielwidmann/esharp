using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EcsTarget
{
    static public class EmbeddedFileLoader
    {



        static public IEnumerable<EcsFile> GetFilesAndUpdate(Assembly assembly, string containingFolder, string ProjectName, string EntryPoint)
        {
            Func<string,string> update = (string old) =>
            {
                var n = old.Replace("$NAME", ProjectName);
                n = n.Replace("$ENTRY", EntryPoint);
                return n;
            };

            return GetFiles(assembly, containingFolder, update);
        }

        static public string GetResourceFile(string filename, ModuleDefinition module)
        {
            var res = module.Resources.Single(x=>x.Name.EndsWith(filename)) as EmbeddedResource;
            var d = res.GetResourceData();
            // I don't really know where those three characters are coming from. Just skip for now.
            var st = System.Text.Encoding.Default.GetString(d.Skip(3).ToArray());
     
            return st;
        }


        static public IEnumerable<EcsFile> GetFiles(Assembly assembly, string containingFolder, Func<string, string> update = null)
        {

            var d = new List<EcsFile>();

            var files = assembly.GetManifestResourceNames();

            foreach(var file in files)
            {
                var content = new StreamReader(assembly.GetManifestResourceStream(file)).ReadToEnd();
                var match = Regex.Split(file, "."+containingFolder+".");
                if (match.Count() != 2) continue;

                var fileName = match[1];
                fileName = fileName.Replace("=", "/");

                if(update != null)
                {
                    d.Add(new EcsFile(update(fileName), update(content)));
                }
                else
                {
                    d.Add(new EcsFile(fileName, content));
                }
                
            }

            return d;
        }        
    }
}
