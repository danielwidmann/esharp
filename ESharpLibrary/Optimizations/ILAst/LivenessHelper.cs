using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.IL;

namespace ESharp.Optimizations.ILAst
{
	public static class LivenessHelper
	{
		public static List<ILInstruction> GetInstructions(ILFunction function)
		{
			var insts = new List<ILInstruction>();

			foreach (var b in function.Descendants.OfType<Block>()) {
				insts.AddRange(b.Instructions);
			}

			return insts;
		}
	}
}
