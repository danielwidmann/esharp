using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ESharp.Optimizations.IL
{
	class MainNoReturnCodeTransform : IILTranform
	{
		public void TransformIL(IEnumerable<TypeDefinition> types)
		{

			foreach(var t in types) {
				foreach(var entryMethod in t.Methods.Where(m=>m.Name == "Main")) {
				
					if (entryMethod.ReturnType.Name != "Void")
						return;

					entryMethod.ReturnType = t.Module.ImportReference(typeof(int));

					// we have to change the return type
					entryMethod.Body.GetILProcessor().Remove(entryMethod.Body.Instructions.Last());

					entryMethod.Body.GetILProcessor().Emit(OpCodes.Ldc_I4_0);
					entryMethod.Body.GetILProcessor().Emit(OpCodes.Ret);
				}
			}						
		}
	}
}
