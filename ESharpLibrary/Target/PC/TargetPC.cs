using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EcsTarget;
using Es.Helper;

namespace ESharp.Target.PC
{
	public class TargetPC : ITarget
	{
		public void Compile(RunOptions options, string ProjectName)
		{
			//call "%VS140COMNTOOLS%"\\vsvars32.bat
			//msbuild "%~dp0\\$NAME.vcxproj"
			//ProcessRunner.RunWithException("cmd", @"/C call ""%VS140COMNTOOLS%vsvars32.bat"" && msbuild ""%~dp0\\$NAME.vcxproj""", options.WorkingDir, options.Verbose);

			ProcessRunner.RunWithException("cmd", @"/C call ""C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\VC\Auxiliary\Build\vcvars32.bat"" && msbuild " + ProjectName + @".vcxproj", options.WorkingDir, options.Verbose);		
		}

		public IEnumerable<EcsFile> ConvertTargetFiles(IEnumerable<EcsFile> allFiles, string ProjectName, string EntryPoint)
		{
			var projectFiles = EmbeddedFileLoader.GetFilesAndUpdate(typeof(TargetPC).Assembly, "Target.PC.Target_VS", ProjectName, EntryPoint);
			var halFiles = EmbeddedFileLoader.GetFiles(typeof(TargetPC).Assembly, "Target.PC.Hal");
			return projectFiles.Concat(halFiles).Concat(allFiles);
		}

		public void Exec(RunOptions options, string ProjectName)
		{
			//@echo OFF
			//"%~dp0\Debug\$NAME.exe"
			//ProcessRunner.RunWithException(@"Debug\" + ProjectName + @".exe", "", options.WorkingDir, options.Verbose);
			// always show output
			ProcessRunner.RunWithException("cmd", @"/C Debug\" + ProjectName + @".exe", options.WorkingDir, true);
		}
	}
}
