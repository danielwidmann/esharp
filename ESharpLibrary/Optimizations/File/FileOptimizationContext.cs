using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace ESharp.Optimizations.File
{
	public class FileOptimizationContext
	{
		public IEnumerable<TypeDefinition> UsedTypes;
	}
}
