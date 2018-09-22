using CommandLine;
using Ecs;
using ECS;
using EcsCompilerService;
using EcsTarget;
using Es.Helper;
using ESharp;
using ESharp.Target;
using ESharp.Target.PC;
using ESharpLibrary.Test;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecs
{
	public class ECS_Main
	{
		public static int Main(string[] args)
		{
			var options = new Options();
			var parser = new Parser(new ParserSettings(System.Console.Out));
			if (!parser.ParseArguments(args, options) || options.Input.Count < 1) {
				System.Console.WriteLine(options.GetUsage());
				return 1;
			}		

			var assembly = EcsCompilerService.AssemblyHelper.LoadAssemblyWithResolver(options.Input[0]);

			MethodDefinition entryPoint;
			if (options.Test != null) {
				var tests = TestDiscovery.DiscoverTests(assembly);
				var filteredTests = tests.Where(x => (x.DeclaringType.Name + "." + x.Name).StartsWith(options.Test) || x.FullName.StartsWith(options.Test)).ToArray();


				if (filteredTests.Count() == 0) {
					throw new ArgumentException("No matching test was found");
				} else
				{
					var mod = ModuleDefinition.CreateModule("TestModule", new ModuleParameters { AssemblyResolver = assembly.AssemblyResolver });
					var testClass = TestEntryGenerator.GenerateTestEntry(filteredTests, mod);
					
					// make sure the actual code is resolvable
					assembly.Types.Add(testClass);

					entryPoint = testClass.Methods.Last();
				}				    
			} else {
				if (options.EntryClass == null) {
					entryPoint = assembly.Types.Select(x => x.Methods.FirstOrDefault(m => m.Name == "Main" && m.IsStatic)).First(x => x != null);
				} else {
					var entryType = assembly.Types.First(x => x.Name == options.EntryClass);
					entryPoint = entryType.Methods.First(x => x.Name == "Main" && x.IsStatic);
				}
			}

			//Logger.IsVerbose = options.Verbose;

			// Ouput directory
			var outputdir = options.OutputDir;
			if (outputdir == null) {
				//var targetString = toolchain.GetType().Name.Replace("Target", "");
				var targetString = "";
				outputdir = "out_" + targetString;
			}

			var execName = assembly.Name;

			// directly include source/header files
			IncludeCFiles.SrcDir = "../../..";
			IncludeCFiles.DestDir = outputdir;

			System.Console.WriteLine("Generating C Code ...");
			var cFiles = CompilerService.Run(entryPoint);


			var target = new TargetPC();
			var targetFiles = target.ConvertTargetFiles(cFiles, execName, entryPoint.DeclaringType.Name + "_Main");

			// todo: is this still needed?
			if (options.Override) { // override all taget files
				foreach (var t in targetFiles) {
					t.Override = true;
				}
			}

			FileWriter.SaveFiles(targetFiles, outputdir);

			if (options.GenerateOnly)
				return 0;

			RunOptions runOptions = new RunOptions {
				Verbose = options.Verbose,
				WorkingDir = outputdir,
				Variant = options.Board
			};

			System.Console.WriteLine("Compiling ...");
			target.Compile(runOptions, execName);


			if (options.CompileOnly)
				return 0;

			int returnCode = 0;

			try {
				System.Console.WriteLine("Running ...");
				target.Exec(runOptions, execName);
			} catch (BadReturnCodeExcpetion e) {
				returnCode = e.Code;
			}

			System.Console.WriteLine();

			if (returnCode != 0) {
				System.Console.WriteLine("Program terminated with return code {0}", returnCode);
			}

			if (options.Wait) {
				System.Console.WriteLine("Done - Press Key to Exit");
				System.Console.ReadKey();
			}

			return returnCode;
		}
	}
}

