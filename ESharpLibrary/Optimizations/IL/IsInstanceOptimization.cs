using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESharp.Helpers;
using ICSharpCode.Decompiler.TypeSystem;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ESharp.Optimizations.IL
{
	class IsInstanceOptimization : IILTranform
	{
		bool IsAssignableTo(TypeDefinition from, TypeDefinition to)
		{

			if(to.IsInterface) {
				return from.Interfaces.Any(x=>x.InterfaceType == to);
			} else {				
				return TypesHierarchyHelpers.IsBaseType(to, from, false);

			}
			
		}

		public void TransformIL(IEnumerable<TypeDefinition> types)
		{
			var isInstaceTypes = types
				.Where(x => !(x.IsSealed && x.IsAbstract))
				.Where(x=>!x.IsValueType);
			var mod = ModuleDefinition.CreateModule("test", ModuleKind.Dll);

			var getHandle = mod.ImportReference(typeof(Type).GetMethod("GetTypeFromHandle"));
			var typeEq = mod.ImportReference(typeof(Type).GetMethod("op_Equality"));		
			var getType = types.Single(x => x.Name == "EObject").Methods.Single(x => x.Name.EndsWith("GetType"));



			// Add Empty Method bodies
			foreach (var t in isInstaceTypes) {
				var nm = new MethodDefinition(t.Name + "_isInstance", MethodAttributes.Static, mod.ImportReference(typeof(Boolean)));
				nm.Parameters.Add(new ParameterDefinition("input", ParameterAttributes.None, types.Single(x => x.Name == "EObject")));
				t.Methods.Add(nm);


				var assignables = types.Where(x => !x.IsAbstract && (IsAssignableTo(x, t) || x == t));
				var sb = new StringBuilder();


				foreach (var t2 in assignables) {


					//todo implement
					//if (IsDerivedFrom(t2, t) || t2 == t) {
					//sb.AppendFormat("if(input->etype == {0}_TypeId)\n", t2.Name);
					//sb.AppendFormat("\treturn true;\n");
					//	w.AppendLine("{");
					//	//w.AppendLine("    EObject_AddRef(input);");
					//	// todo: do this properly. Maybe more generic
					//	w.AppendLine("    input->refCount += 1;");
					//	w.AppendFormat("    return ({0})input;\n", t.Name);
					//	w.AppendLine("}");
					//}
				}

				//sb.AppendLine("return false;");

				//EmitSource.SourceImplementation(nm, sb.ToString());
				{ 
					nm.Body = new MethodBody(nm);

					var v = new VariableDefinition(mod.ImportReference(typeof(Boolean)));
					nm.Body.Variables.Add(v);
					var proc = nm.Body.GetILProcessor();

					var exit = Instruction.Create(OpCodes.Nop);

					foreach (var t2 in assignables) {
						var next = Instruction.Create(OpCodes.Nop);
						// compare types
						proc.Emit(OpCodes.Ldarg_0);
						proc.Emit(OpCodes.Call, getType);
						proc.Emit(OpCodes.Ldtoken, t2);
						proc.Emit(OpCodes.Call, getHandle);
						proc.Emit(OpCodes.Call, typeEq);			


						// return true if match. Use BrTrue to create nice c code
						var match = Instruction.Create(OpCodes.Nop);
						proc.Emit(OpCodes.Brtrue, match);
						proc.Emit(OpCodes.Br, next);
						proc.Append(match);
						proc.Emit(OpCodes.Ldc_I4_1);
						proc.Emit(OpCodes.Stloc, v);
						proc.Emit(OpCodes.Br, exit);
						proc.Append(next);
					}

					// return false 
					proc.Emit(OpCodes.Ldc_I4_0);
					proc.Emit(OpCodes.Stloc, v);
					proc.Emit(OpCodes.Br, exit);

					// return with result
					proc.Append(exit);
					proc.Emit(OpCodes.Ldloc, v);
					proc.Emit(OpCodes.Ret);
				}
				{

					var isInst = nm;
					nm = new MethodDefinition(t.Name + "_asInstance", MethodAttributes.Static, t);
					nm.Parameters.Add(new ParameterDefinition(types.Single(x => x.Name == "EObject")));
					t.Methods.Add(nm);


					nm.Body = new MethodBody(nm);
					var v = new VariableDefinition(t);
					nm.Body.Variables.Add(v);

					var proc = nm.Body.GetILProcessor();
					var exit= proc.Create(OpCodes.Nop);
					var exit_null = proc.Create(OpCodes.Nop);

					proc.Emit(OpCodes.Ldarg_0);
					proc.Emit(OpCodes.Call, isInst);

					proc.Emit(OpCodes.Brfalse, exit_null);

					proc.Emit(OpCodes.Ldarg_0);
					proc.Emit(OpCodes.Stloc, v);
					proc.Emit(OpCodes.Br, exit);

					proc.Append(exit_null);
					proc.Emit(OpCodes.Ldnull);
					proc.Emit(OpCodes.Stloc, v);					

					// return with result
					proc.Append(exit);
					proc.Emit(OpCodes.Ldloc, v);
					proc.Emit(OpCodes.Ret);
				}
			}

			// replace the call to isInst
			foreach (var t in types) {
				foreach (var m in t.Methods.Where(x => x.HasBody)) {
					var ilp = m.Body.GetILProcessor();
					for (int idx = 0; idx < m.Body.Instructions.Count; idx++) {
						var i = m.Body.Instructions[idx];
					
						if (i.OpCode == OpCodes.Isinst) {
							if (i.Next.OpCode == OpCodes.Ldnull && i.Next.Next.OpCode == OpCodes.Cgt_Un) {
								ilp.Remove(i.Next);
								ilp.Remove(i.Next);
								var isInstMethod = ((TypeDefinition)i.Operand).Methods.Single(x=>x.Name.EndsWith("_isInstance"));
								ilp.Replace(i, ilp.Create(OpCodes.Call, isInstMethod));
							} else {
								var asInstMethod = ((TypeDefinition)i.Operand).Methods.Single(x => x.Name.EndsWith("_asInstance"));
								ilp.Replace(i, ilp.Create(OpCodes.Call, asInstMethod));
							}							
						}
					}
				}
			}
		}
	}	
}
