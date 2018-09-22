using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESharp.Optimizations.IL;
using Mono.Cecil;

namespace ESharp.Optimizations.IL
{
	public class InterfaceOptimization : IILTranform
	{
		public void TransformIL(IEnumerable<TypeDefinition> types)
		{
			foreach (var t in types.Where(x => x.IsInterface)) {
				if (t.IsInterface) {
					t.BaseType = types.Single(x => x.Name == "EObject");
					t.IsInterface = false;
				}
			}
		}
	}
}
