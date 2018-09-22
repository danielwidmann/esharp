using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EcsTarget
{
    class CMerger
    {
        public static IEnumerable<EcsFile> MergeIntoSingleFile(IEnumerable<EcsFile> files)
        {
            var newFiles = new List<EcsFile>();
             var w = new StringBuilder();
             var u = new StringBuilder();
            {
                w.AppendLine("#pragma once");

                w.AppendLine("#ifdef __cplusplus");
                w.AppendLine("extern \"C\" {");
                w.AppendLine("#endif");
                
                var header = files.Where(x => x.Name.EndsWith(".h"));
                var lines = header.Select(x => Regex.Split(x.Content, "\r\n|\r|\n"));
                var forward = lines.SelectMany(x => x.SkipWhile(l => !l.Contains("// pre-header")).Skip(1).TakeWhile(l => !l.Contains("// internal includes")));
                var decl = lines.SelectMany(x => x.SkipWhile(l => !l.Contains("// external includes")).Skip(1));
                //var body = files.Select(x => File.ReadLines(x)).SelectMany(x => x);//.Where(x=>!x.Contains("#include"));
                var body = files.Where(x => x.Name.EndsWith(".c") || x.Name.EndsWith(".cpp")).Select(x => x.Content);

                foreach (var l in forward.Concat(decl))
                {
                    w.AppendLine(l);
                }

                u.AppendLine("#include \"CSharp.h\"");
                foreach (var l in body)
                {
                    u.AppendLine(l);
                }

                w.AppendLine("#ifdef __cplusplus");
                w.AppendLine("}");
                w.AppendLine("#endif");

                newFiles.Add(new EcsFile("CSharp.h" , w.ToString()));
                newFiles.Add(new EcsFile("CSharp.c" , u.ToString()));

                return newFiles;
            }
        }
    }
}
