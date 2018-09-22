using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using ECSharp;
using ESharp.Helpers;
using ICSharpCode.Decompiler;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ESharp.Optimizations.IL
{

	public class DelegateTransform : IILTranform
	{
		public void TransformIL(IEnumerable<TypeDefinition> types)
		{
			foreach (var t in types) {				

				// a method might be iterated twice, but it doesn't matter with this operation. 
				for(int mIdx = 0; mIdx < t.Methods.Count; mIdx++) {
					var m = t.Methods[mIdx];
					if (!m.HasBody)
						continue;

					for (int idx = 0; idx < m.Body.Instructions.Count; idx++) {
						var i = m.Body.Instructions[idx];
						if (i.OpCode == OpCodes.Ldftn || i.OpCode == OpCodes.Ldvirtftn) {
							//i.Op
							var fn = i.Operand as MethodDefinition;
							i.OpCode = OpCodes.Call;
							i.Operand = CreateLdFtnMethod(fn);							

						}
					}
				}
			}
		}

		public string LdFtnMethodName(MethodDefinition def)
		{
			var s = def.Name + "_LdFtn";
			return s;
		}

		public MethodDefinition CreateLdFtnMethod(MethodDefinition def)
		{
			var t = def.DeclaringType;

			var existing = t.Methods.SingleOrDefault(x => x.Name == LdFtnMethodName(def));
			if (existing != null)
				return existing;

			var nm = new MethodDefinition(LdFtnMethodName(def), def.Attributes, t.Module.ImportReference((typeof(IntPtr))));
			
			var code = "return ldftn(" + def.Name + ");";
			EmitSource.SourceImplementation(nm, code);
			t.Methods.Add(nm);
			return nm;
		}

	}
}
