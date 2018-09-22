using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESharp.Annotations;
using ESharp.Helpers;
using ICSharpCode.Decompiler.ECS;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ESharp.Optimizations.IL
{
	class VirtualCallOptimization : IILTranform
	{
		// after type rename??
		public void TransformIL(IEnumerable<TypeDefinition> types)
		{
			var resolver = new ReferenceResolver(types);

			var mod = ModuleDefinition.CreateModule("bla", ModuleKind.Dll);
			
			var m_virtuals = VirtualHelper.VirtualAnalysis(types).Where(x => x.Method.Name != ".ctor").ToArray();
			var toReplace = new Dictionary<MethodReference, MethodReference>();

			// add a pacleholder for the virtual method
			foreach (var virt in m_virtuals) {
				if (!virt.Method.DeclaringType.IsInterface) {
					virt.OverwrittenBy.Add(virt.Method);
				}

				var org = virt.Method;
				var virtualMethod = new MethodDefinition(org.Name + "_virtual",org.Attributes, org.ReturnType);
				virtualMethod.IsAbstract = false;
				virtualMethod.IsVirtual = false;
				//Debug.Assert(virtualMethod.IsStatic);

				virtualMethod.CustomAttributes.Add(new CustomAttribute(mod.ImportReference(typeof(ManualRefCounting).GetConstructor(new Type[] { })), new byte[] {1,0,0,0 }));
				foreach (var p in org.Parameters) virtualMethod.Parameters.Add(p);

				org.DeclaringType.Methods.Add(virtualMethod);
				virt.Method = virtualMethod;
				

				toReplace.Add(org, virtualMethod);


				// imlement the virtual method
				virtualMethod.Body = new MethodBody(virtualMethod);

				var proc = virtualMethod.Body.GetILProcessor();

				
				var getHandle = mod.ImportReference(typeof(Type).GetMethod("GetTypeFromHandle"));
				var typeEq = mod.ImportReference(typeof(Type).GetMethod("op_Equality"));
				
				var getType = types.Single(x => x.Name == "EObject").Methods.Single(x => x.Name == "GetType");				

				var ret = Instruction.Create(OpCodes.Nop);

				foreach (var o in virt.OverwrittenBy) {
					var next = Instruction.Create(OpCodes.Nop);
					// compare types
					proc.Emit(OpCodes.Ldarg_0);					
					proc.Emit(OpCodes.Call, getType);
					proc.Emit(OpCodes.Ldtoken, o.DeclaringType);
					proc.Emit(OpCodes.Call, getHandle);
					proc.Emit(OpCodes.Call, typeEq);

					// call virtual if there is a match
					proc.Emit(OpCodes.Brfalse, next);

					Debug.Assert(virt.Method.IsStatic == false);
					// If this parameter has not been transformed, add one parameter for this.
					proc.Emit(OpCodes.Ldarg_0);
					for (var paramIdx = 0; paramIdx < virt.Method.Parameters.Count; paramIdx++) {
						proc.Emit(OpCodes.Ldarg, paramIdx + 1);
					} 

					proc.Emit(OpCodes.Call, o);
					proc.Emit(OpCodes.Br, ret);
					proc.Append(next);
				}

				// type not found
				proc.Emit(OpCodes.Call, resolver.GetMethodReference(()=> ECSharp.Core.ESharpRT.Error()));

				// Make sure the stack is correct
				if(virt.Method.ReturnType.Name != "Void") {
					if(virt.Method.ReturnType.IsValueType) {
						var variable = new VariableDefinition(virt.Method.ReturnType);
						virtualMethod.Body.Variables.Add(variable);
						proc.Emit(OpCodes.Ldloca, variable);
						proc.Emit(OpCodes.Initobj, virt.Method.ReturnType.Resolve());
						proc.Emit(OpCodes.Ldloc, variable);						
					} else {
						proc.Emit(OpCodes.Ldnull);
					}
				}

				proc.Append(ret);
				proc.Emit(OpCodes.Ret);

			}

			// change the reference to the virtual method
			foreach (var t in types) {
				foreach (var m in t.Methods) {
					if (m.Body == null) continue;

					for (int idx = 0; idx < m.Body.Instructions.Count; idx++) {


						var i = m.Body.Instructions[idx];
						var mRef = i.Operand as MethodReference;
						if (mRef == null) continue;

						// only replace virtual calls
						if (i.OpCode == OpCodes.Call) continue;

						if (toReplace.ContainsKey(mRef)) {
							i.Operand = toReplace[mRef];
							Debug.Assert(i.Operand is MethodDefinition);

							// only use non virtual call instructions
							if (i.OpCode == OpCodes.Ldvirtftn) {
								i.OpCode = OpCodes.Ldftn;
								// make sure the stack is correct.
								m.Body.GetILProcessor().InsertBefore(i, Instruction.Create(OpCodes.Pop));
								idx++;
							} else if (i.OpCode == OpCodes.Callvirt)
								i.OpCode = OpCodes.Call;
							else
								Debug.Assert(false, "Unecpected virtual opcode");

						}
					}
				}
			}
		}
	}
}
