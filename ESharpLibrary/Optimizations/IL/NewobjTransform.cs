using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESharp.Optimizations.TypeDiscoveryOptimization;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ESharp.Optimizations.IL
{

	public class NewobjTransform : IILTranform
	{
		public void TransformIL(IEnumerable<TypeDefinition> types)
		{			
			// replace the call to newInst
			foreach (var t in types) {
				foreach (var m in t.Methods.Where(x=>x.HasBody)) {
					foreach(var i in m.Body.Instructions) {
						if(i.OpCode == OpCodes.Newobj) {
							var ctor = (i.Operand as MethodReference).Resolve();
							i.OpCode = OpCodes.Call;
							i.Operand = ctor.DeclaringType.Methods.Single(x => x.Name == ".new" && x.Parameters.Count == ctor.Parameters.Count);
						}
						
					}					
				}
			}
		}
	}
}
