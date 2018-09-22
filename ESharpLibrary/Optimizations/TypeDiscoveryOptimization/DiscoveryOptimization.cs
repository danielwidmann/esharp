using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ESharp.Optimizations.TypeDiscoveryOptimization
{
	public class DiscoveryOptimization
	{		
		public static void Optimize(TypeDefinition t)
		{
			NewObjectTransform.Optimize(t);
			StripExternCBody.Optimize(t);
			StripEObjectBaseType.Optimize(t);
			DelegateFixup.Optimize(t);
			new ArrayTransform().TransformIL(t);
			(new GenericOptimization()).Optimize(t);
			(new GenericOptimization()).TransformValueBoxing(t);
			ReplaceRefWithPointer.Optimize(t);			

		}
	}
}
