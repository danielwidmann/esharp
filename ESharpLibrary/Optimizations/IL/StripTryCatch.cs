using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ESharp.Optimizations.IL
{
	class StripTryCatch : IILTranform
	{
		public void TransformIL(IEnumerable<TypeDefinition> types)
		{
			foreach(var t in types) {
				foreach(var m in t.Methods.Where(x=>x.HasBody)) {
					var ilp = m.Body.GetILProcessor();
					foreach(var e in m.Body.ExceptionHandlers) {
						int startIdx = m.Body.Instructions.IndexOf(e.HandlerStart);
						int endIdx = m.Body.Instructions.IndexOf(e.HandlerEnd);
						for (int i = startIdx; i < endIdx; i++) {
							ilp.Replace(m.Body.Instructions[i], ilp.Create(OpCodes.Nop));
							
						}
					}
					m.Body.ExceptionHandlers.Clear();
				}
			}
		}
	}
}
