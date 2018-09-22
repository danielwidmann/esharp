using EcsTarget;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcsCompilerService
{
    public class IncludeCFiles
    {
        public static string SrcDir { get; set; }
        public static string DestDir { get; set; }

        static IEnumerable<String> Visitor(String root, String searchPattern)
        {
            foreach (var file in Directory.GetFiles(root, searchPattern, SearchOption.TopDirectoryOnly))
            {
                yield return file;
            }

            foreach (var folder in Directory.GetDirectories(root))
            {
                if (folder.Contains("bin\\Debug") || folder.Contains("bin\\Release") 
                    || folder.Contains("\\."))
                { //skip certain folders
                    continue;
                }
                foreach (var file in Visitor(folder, searchPattern))
                    yield return file;
            }
        }

        /// <summary>
        /// Include source files so they can be directly changed for debugging.
        /// </summary>
        /// <param name="files"></param>
        public static void Run(IEnumerable<EcsFile> files) 
        {
            // currently only c files work. Split header into header_fwd to make this work.
            // .Concat(Visitor(SrcDir, "*.h"))
            foreach (var f in Visitor(SrcDir, "*.c"))
            {
				var contentOnDisk = File.ReadAllText(f);

				foreach (var ecsFile in files) {
                    if (f.EndsWith(ecsFile.Name) || contentOnDisk == ecsFile.Content)
                    {
                        Uri fullPath = new Uri(Path.GetFullPath(f), UriKind.Absolute);
                        Uri relRoot = new Uri(Path.GetFullPath(DestDir+"/"), UriKind.Absolute);

                        string relPath = Uri.UnescapeDataString(relRoot.MakeRelativeUri(fullPath).ToString());
                        //var contentOnDisk = File.ReadAllText(f);


                        if (contentOnDisk != ecsFile.Content 
                            && contentOnDisk + "\r\n" != ecsFile.Content)
                        {
                            throw new Exception("File dones't match");
                        }

                        ecsFile.Content = String.Format("#include \"{0}\" \r\n", relPath);
                    }
                }
            }
            
        }
    }
}
