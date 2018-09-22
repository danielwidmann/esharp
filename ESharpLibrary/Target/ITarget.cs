using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EcsTarget;

namespace ESharp.Target
{
	public class RunOptions
	{
		public string WorkingDir { get; set; }
		public bool Verbose { get; set; }
		/// <summary>
		/// The targeted Variant. E.g. the Arduino Board or the x86/x64 architecture. 
		/// </summary>
		public string Variant { get; set; }

	}

	interface ITarget
	{
		/// <summary>
		/// Takes the previously generated files and puts everything into a target project
		/// </summary>
		/// <param name="generatedFiles"></param>
		IEnumerable<EcsFile> ConvertTargetFiles(IEnumerable<EcsFile> allFiles, string ProjectName, string EntryPoint);


		void Compile(RunOptions options, string ProjectName);

		void Exec(RunOptions options, string ProjectName);
	}
}
