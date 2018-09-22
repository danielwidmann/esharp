using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ESharp.Optimizations.TypeDiscoveryOptimization
{
	class StripExternCBody
	{
		public static void Optimize(TypeDefinition t)
		{
			foreach(var m in t.Methods) {
				if (!m.CustomAttributes.Any(x => x.Constructor.DeclaringType.Name == "ExternC"))
					continue;

				// We should only get here with ExternC attributes				

				m.Body = null;
			}
		}

	}
}
