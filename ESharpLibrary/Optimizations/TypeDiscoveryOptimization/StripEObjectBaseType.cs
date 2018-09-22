using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace ESharp.Optimizations.TypeDiscoveryOptimization
{
	/// <summary>
	/// This will avoid endless loops when using base types
	/// </summary>
	class StripEObjectBaseType
	{	
		public static void Optimize(TypeDefinition t)
		{
			if(t.Name == "EObject") {
				t.BaseType = null;
			}
			
		}	
	}
}
