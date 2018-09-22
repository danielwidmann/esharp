using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EcsHelper;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ESharp.Optimizations.IL
{
	class FinalizerOptimization : IILTranform
	{
		public void TransformIL(IEnumerable<TypeDefinition> types)
		{
			foreach (var t in types.Where(x => !x.IsInterface && !x.IsAbstract)) {
				var finalizer = t.Methods.SingleOrDefault(x => x.Name == "Finalize");
				if (finalizer == null) { // create finalizer, if doesn't exist yet
					finalizer = new MethodDefinition("Finalize", MethodAttributes.Virtual, t.Module.ImportReference(typeof(void)));
					t.Methods.Add(finalizer);

					finalizer.Body = new MethodBody(finalizer);
					finalizer.Body.GetILProcessor().Emit(OpCodes.Ret);
				}				
			}



			// static finalizer
			//var ty = new TypeDefinition("", "StaticFieldFinalizer", TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed);
			var mod = ModuleDefinition.CreateModule("CreatedModule", ModuleKind.Dll);


			var finalizers = new MethodDefinition("NullStaticFields", MethodAttributes.Static, mod.ImportReference(typeof(void)));
			// todo add to ESharpCore instead??
			types.Single(x => x.Name == "EObject").Methods.Add(finalizers);

			finalizers.Body = new MethodBody(finalizers);
			finalizers.Body.MaxStackSize = 1;
			var procs = finalizers.Body.GetILProcessor();

			foreach (var field in types.SelectMany(x => x.Fields).Where(x => x.IsStatic && !x.FieldType.IsValueType)) {

				procs.Emit(OpCodes.Ldnull);
				procs.Emit(OpCodes.Stsfld, field);

				IlHelper.UpdateIlOffsets(finalizers.Body);
			}
			procs.Emit(OpCodes.Ret);

		}

	}

	// has to run after value type optimization
	class FinalizerImplementationOptimization : IILTranform
	{
		public void TransformIL(IEnumerable<TypeDefinition> types)
		{
			foreach (var t in types.Where(x => !x.IsInterface && !x.IsAbstract)) {
				var finalizer = t.Methods.SingleOrDefault(x => x.Name.EndsWith("Finalize"));

				if (finalizer == null)
					continue;
				
				var proc = finalizer.Body.GetILProcessor();
				finalizer.Body.MaxStackSize = Math.Max(finalizer.Body.MaxStackSize, 2);


				var ret = finalizer.Body.Instructions.LastOrDefault();
				if (ret != null)
					finalizer.Body.Instructions.Remove(ret);

				foreach (var field in t.Fields.Where(
					x => !x.IsStatic && !x.FieldType.IsValueType)) {
					proc.Emit(OpCodes.Ldarg_0);
					proc.Emit(OpCodes.Ldnull);
					proc.Emit(OpCodes.Stfld, field);
				}

				foreach (var field in t.Fields.Where(x => x.FieldType.IsValueType && ValueTypeHelper.NeedRefCounting(x.FieldType.Resolve()))) {
					proc.Emit(OpCodes.Ldarg_0);
					proc.Emit(OpCodes.Ldflda, field);
					proc.Emit(OpCodes.Call, field.FieldType.Resolve().Methods.Single(x => x.Name.EndsWith("RemoveRef")));
				}

				// call base finalizer
				if(!t.IsValueType && t.BaseType != null && t.BaseType.Name != "EObject") {
					var baseFinalizer = t.BaseType.Resolve().Methods.SingleOrDefault(x => x.Name.EndsWith("Finalize"));
					if(baseFinalizer != null) {
						proc.Emit(OpCodes.Ldarg_0);
						proc.Emit(OpCodes.Call, baseFinalizer);
					}
				}

				proc.Emit(OpCodes.Ret);
				IlHelper.UpdateIlOffsets(finalizer.Body);
			}
		}
	}
}
