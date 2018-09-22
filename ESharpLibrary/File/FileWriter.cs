using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcsTarget
{
    public class FileWriter
    {

        public static void SetOverrideFlagsBasedOnEndings(List<EcsFile> files)
        {
            var overrideEndings = new string[] { "c", "cpp", "h", "hpp" };
            
            foreach(var f in files)
            {
                f.Override = overrideEndings.Any(x => f.Name.EndsWith(x));
            }
        }

        public static void SaveFiles(IEnumerable<EcsFile> files, String outputDir)
        {
            if(!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            // delete old files, but keep directories
            System.IO.DirectoryInfo di = new DirectoryInfo(outputDir);
            foreach (FileInfo file in di.GetFiles())
            {
                if (files.Any(x => x.Name == file.Name))
                    continue;
                file.Delete();
            }
            
            foreach (var f in files)
            {
                var targetPath = Path.Combine(outputDir, f.Name);
                //// only override source and header files
                //if (File.Exists(targetPath) && !f.Override)
                //{
                //    Logger.Info("Skip file " + f.Name);
                //    continue;
                //}
                if (!Directory.Exists(Path.GetDirectoryName(targetPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                }

                if (File.Exists(targetPath) && File.ReadAllText(targetPath) == f.Content)
                { // skip not changed files 
                    continue;
                }
                File.WriteAllText(targetPath, f.Content);
            }
        }
    }
}
