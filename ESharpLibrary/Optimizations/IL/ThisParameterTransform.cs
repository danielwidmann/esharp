using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ESharp.Optimizations.IL
{
	public class ThisParameterTransform : IILTranform
	{
		public void TransformIL(IEnumerable<TypeDefinition> types)
		{
			foreach(var t in types) {
				foreach(var m in t.Methods) {
					if (m.HasThis) {
						m.HasThis = false;
						m.IsStatic = true;

						TypeReference thisType;
						if(t.IsValueType) {
							thisType = new PointerType(t);
						} else {
							thisType = t;
						}

						m.Parameters.Insert(0, new ParameterDefinition("_this", ParameterAttributes.None, thisType));                        

					}
				}
			}
		}
	}
}
