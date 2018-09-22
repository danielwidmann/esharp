using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ESharp.UsedTypeAnalysis
{
	public class TypeVisitor
	{
		public static void ReplaceTypeRefs(TypeDefinition t,
			Func<TypeReference, TypeReference> f)
		{
			VisitType(
				t,
				f,
				x => {
					return x;
				});
		}
		public static void VisitAllTypeRefsMemberTypeRefs(TypeDefinition t,
			Action<TypeReference> f)
		{
			VisitType(
				t,
				x => { f(x); return x; },
				x => {
					// common member fields
					f(x.DeclaringType);

					if (x is MethodDefinition || x is FieldDefinition) {

					} else if (x is MethodReference mRef) {
						f(mRef.ReturnType);
						foreach (var p in mRef.Parameters) {
							f(p.ParameterType);
						}
					} else if (x is FieldReference fRef && !(x is FieldDefinition)) {
						f(fRef.FieldType);
					} else {
						Debug.Assert(false, "Member Type " + x.GetType().Name + " is not Supported");
					}

					return x;
				});
		}

		public static void VisitAllMemberRefs(TypeDefinition t,
			Func<MemberReference, MemberReference> fm)
		{
			VisitType(t, x => x, fm);
		}

		public static void VisitType(TypeDefinition t,
			Func<TypeReference, TypeReference> f,
			Func<MemberReference, MemberReference> fm,
			bool WithAttributes = false)
		{
			if (t.BaseType != null)
				t.BaseType = f(t.BaseType);

			// visit the type itself
			f(t);

			foreach (var field in t.Fields) {
				field.FieldType = f(field.FieldType);
			}

			var newInterfaces = new List<TypeReference>();
			foreach (var i in t.Interfaces) {
				var newType = f(i.InterfaceType);
				if (newType != null)
					newInterfaces.Add(newType);
			}
			t.Interfaces.Clear();
			foreach (var i in newInterfaces)
				t.Interfaces.Add(new InterfaceImplementation(i));

			foreach (var m in t.Methods) {
				//if (m.CustomAttributes.Any(x => x.Constructor.DeclaringType.Name == "ExternC"))
				//	continue;

				m.ReturnType = f(m.ReturnType);
				foreach (var p in m.Parameters) {
					p.ParameterType = f(p.ParameterType);
				}

				if (m.Body == null) continue;

				// todo, move to operation
				//if (m.CustomAttributes.Any(x => x.Constructor.DeclaringType.Name == "ExternC"))
				//	continue;

				foreach (var v in m.Body.Variables) {

					v.VariableType = f(v.VariableType);
					if (v.VariableType.IsValueType && !v.VariableType.IsPrimitive) {
						// mark the value type before we override the information
						//todo: dp this properly
						//v.Name = v.Name+"_"+v.Index + "_valuetype";
					}
				}


				foreach (var inst in m.Body.Instructions) {
					if (inst.Operand is TypeReference) {
						var reference = inst.Operand as TypeReference;
						var resolved = f(reference);
						Debug.Assert(resolved != null);
						inst.Operand = resolved;
					} else if (inst.Operand is MemberReference) {
						var reference = inst.Operand as MemberReference;
						var resolved = fm(reference);
						if (resolved != null)
							inst.Operand = fm(reference);
					}

					//todo?
					//AddType(inst.Operand as PropertyReference);

					if (inst.OpCode == OpCodes.Throw) {

					}
				} //foreach body.instructions

				foreach (var h in m.Body.ExceptionHandlers) {
					if (h.CatchType != null) {
						h.CatchType = f(h.CatchType);
					}
				}
			}
		}
	}
}
