using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.ECS;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ESharp.Optimizations.TypeDiscoveryOptimization
{
	public class NewObjectTransform
	{
		public static void Optimize(TypeDefinition t)
		{
			var mod = ModuleDefinition.CreateModule("bla", ModuleKind.Dll);
			var getHandle = mod.ImportReference(typeof(Type).GetMethod("GetTypeFromHandle"));
			var malloc = mod.ImportReference(typeof(CMalloc).GetMethod("Malloc"));

			// first add a create method to all non static types
			if (!t.IsAbstract) {
				var newMethods = new List<MethodDefinition>();
				foreach (var c in t.Methods.Where(m => m.IsConstructor && !m.IsStatic)) {
					var nm = new MethodDefinition(".new", MethodAttributes.Static, t);
					foreach (var p in c.Parameters) {
						nm.Parameters.Add(new ParameterDefinition(p.Name, p.Attributes, p.ParameterType));
					}

					var proc = nm.Body.GetILProcessor();

					proc.Append(Instruction.Create(OpCodes.Ldtoken, t));
					proc.Append(Instruction.Create(OpCodes.Call, getHandle));
					proc.Append(Instruction.Create(OpCodes.Call, malloc));
					proc.Append(Instruction.Create(OpCodes.Castclass, t));
					proc.Append(Instruction.Create(OpCodes.Dup));

					for (int i = 0; i < c.Parameters.Count; i++) {
						proc.Append(Instruction.Create(OpCodes.Ldarg, nm.Parameters[i]));
					}

					proc.Append(Instruction.Create(OpCodes.Call, c));
					proc.Append(Instruction.Create(OpCodes.Ret));

					newMethods.Add(nm);
				}
				foreach (var nm in newMethods) {
					t.Methods.Add(nm);
				}
			}

			// replacing the newobj is done in a il transform
			return;
			
		}

	}
}
