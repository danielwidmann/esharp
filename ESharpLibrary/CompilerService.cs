using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EcsCompilerService;
using EcsTarget;
using ESharp.Helpers;
using ESharp.ILSpyService;
using ESharp.Library.Optimizations.IL;
using ESharp.Optimizations.File;
using ESharp.Optimizations.IL;
using ESharp.UsedTypeAnalysis;
using ICSharpCode.Decompiler.ECS;
using Mono.Cecil;


namespace ESharp
{
	public class CompilerService
	{
		public static IEnumerable<EcsFile> Run(MethodReference entryPoint)
		{
			var usedTypes = GetUsedTypesOrdered(entryPoint).ToList();

			var moduleParameter = new ModuleParameters { Kind = ModuleKind.Dll, AssemblyResolver = entryPoint.Module.AssemblyResolver };
			var mod = ModuleDefinition.CreateModule("test", moduleParameter);
			// copy search path from old assembly

			// todo move to new transform, discovery transform?
			foreach (var t in usedTypes) {
				t.IsNestedPrivate = false;
			}

			foreach (var t in usedTypes) {
				mod.Types.Add(t);
			}
    
            (new NewobjTransform()).TransformIL(usedTypes);
			//(new ArrayTransform()).TransformIL(usedTypes);
			(new FinalizerOptimization()).TransformIL(usedTypes);
			(new VirtualCallOptimization()).TransformIL(usedTypes);
			(new MainNoReturnCodeTransform()).TransformIL(usedTypes);
			RenameTransform.Rename(usedTypes);
            (new ThisParameterTransform()).TransformIL(usedTypes);
            (new DelegateTransform()).TransformIL(usedTypes);
			(new StringLiteralOptimization()).GlobalOptimization(usedTypes);
            (new ValueTypeOptimization()).TransformIL(usedTypes);
            (new IsInstanceOptimization()).TransformIL(usedTypes); // has to be before Interface Optimization (IsInterface is set to false there)
			(new InterfaceOptimization()).TransformIL(usedTypes);
			(new FinalizerImplementationOptimization()).TransformIL(usedTypes);
			(new ExceptionTransform()).TransformIL(usedTypes);
            (new StripExternalAnnotations()).TransformIL(usedTypes);
            //(new StripTryCatch()).TransformIL(usedTypes);

            // do this after ExceptionTransform as it will create default values.
            (new PrimitiveDefaultValues()).TransformIL(usedTypes);






            ILAstDebugPrinter.ClearLogs();

			ReferenceImportHelper.ImportReferences(mod);
			mod.Write(@"logs\compacted.dll");

            File.Copy("ESharpCore.dll", @"logs\ESharpCore.dll");

			// todo, just disabled temporarly because of poiter types resolving to null?? Should this happen?
			ReferenceChecker.CheckReferences(mod);

			var files = (new DecompilerService()).Generate(usedTypes, @"logs\compacted.dll").ToList();

			// file optimization
			var fileOptimizationContext = new FileOptimizationContext { UsedTypes = usedTypes };

			TypeEnumGenerator.FileOptimization(files, fileOptimizationContext);			

			IncludeCFiles.Run(files);

			var mergedFiles = CMerger.MergeIntoSingleFile(files);

			return mergedFiles;
		}

		public static IEnumerable<TypeDefinition> GetUsedTypesOrdered(MethodReference entryPoint)
		{
			// Load assembly
			//var assembly = entryPoint.DeclaringType.Module.Assembly;
			//var projectName = assembly.MainModule.Name.ToString().Replace(".exe", "").Replace(".dll", "");


			var unorderedTypes = new NewUsedTypeAnalysis().GetUsedTypes(entryPoint).ToList();

			// todo new replace base types
			//why don;t generics show up in used?

			// order by subtypes. EObject is the root.
			var usedTypes = TreeOrder.Order(unorderedTypes.First(x => x.Name == "EObject"), unorderedTypes);

			// add Value types (which are not derived from EObject)
			var valueTypes = unorderedTypes.Where(x => x.IsValueType);
			usedTypes = usedTypes.Take(1).Concat(valueTypes).Concat(usedTypes.Skip(1)).ToList();

			// make sure the structs are in order
			valueTypes = TreeOrder.StructOrder(valueTypes);		


			// todo: interfaces should be tree ordered as well
			usedTypes.AddRange(unorderedTypes.Where(x => x.IsInterface));
			if(usedTypes.Count != unorderedTypes.Count) {
				var missing = unorderedTypes.Where(x => !usedTypes.Contains(x)).ToArray();
				Debug.Assert(false, missing.ToString());
			}

			//// move ESharpCore as first file for header definitions
			//var core = usedTypes.Single(x => x.Name == "ESharpCore");
			//usedTypes.Remove(core);
			//usedTypes.Insert(0, core);

			return usedTypes;
		}
	}
}
