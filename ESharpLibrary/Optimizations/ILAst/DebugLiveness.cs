using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESharp.Helpers;
using ICSharpCode.Decompiler.ECS;
using ICSharpCode.Decompiler.IL;

namespace ESharp.Optimizations.ILAst
{
	public class DebugLiveness
	{
		public static Dictionary<ILInstruction, string> DebugLocalLiveness(ILFunction method, IEnumerable<ILVariable> variables) //DecompilerContext context,
		{
			var insts = LivenessHelper.GetInstructions(method);
			var live_in = insts.Select(x => "").ToList();
			var live_out = insts.Select(x => "").ToList();

			Liveness.ControlFlow(insts, out int[][] succ, out int[][] pres);

			foreach (var v in variables) {

				var live = Liveness.CalcLiveness(insts, v, succ, pres);
				
				for (int idx = 0; idx < live.Length; idx++) {
					if (live[idx].LiveIn)
						live_in[idx] += v.Name + ",";
					if (live[idx].LiveOut)
						live_out[idx] += v.Name + ",";
				}
			}

			var zipped = live_in.Zip(live_out, (x, y) => "[" + x + "|" + y + "]").ToArray();

			var d = new Dictionary<ILInstruction, string>();
			for(int i = 0; i < insts.Count; i++) {
				d.Add(insts[i], zipped[i]);
			}

			return d;
		}
	}
}
