using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESharp.UsedTypeAnalysis;
using Mono.Cecil;

namespace ESharp.Optimizations.TypeDiscoveryOptimization
{
	class ReplaceRefWithPointer
	{
		public static void Optimize(TypeDefinition t)
		{
			// C only knows pointers but no references. Replace with pointers;
			//Type
			TypeVisitor.ReplaceTypeRefs(t, x => x.IsByReference ? new PointerType(x.GetElementType()) : x);

		}
	}
}
