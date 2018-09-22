using System;
using System.Collections.Generic;
using System.Linq;
using ESharp.Optimizations.ILAst;
using ICSharpCode.Decompiler.IL;

namespace ICSharpCode.Decompiler.ECS
{
	public class Liveness
	{

		static bool usesSingle(ILInstruction inst, ILVariable variable)
		{
			foreach (var i in inst.Descendants) {
				if (i.MatchLdLoc(variable)) {
					return true;
				}
				if (i.MatchLdLoca(variable)) {
					return true;
				}
			}

			return false;
		}

		public static bool defs(ILInstruction node, ILVariable variable)
		{
			// simple assignement
			if (node.MatchStLoc(variable, out ILInstruction value)) {
				return true;
			}

			return false;
		}

		public static IEnumerable<int> Succ(List<ILInstruction> code, int curr)
		{
			List<int> succs = new List<int>();

			Block target;
			if (code[curr].MatchBranch(out target)) {
				succs.Add(code.IndexOf(target.Instructions[0]));
			} else if (code[curr].MatchIfInstruction(out ILInstruction T, out ILInstruction F)) {
				if (T.MatchBranch(out target))
					succs.Add(code.IndexOf(target.Instructions[0]));
				if (F.MatchBranch(out target))
					succs.Add(code.IndexOf(target.Instructions[0]));
				// falling through if/else is possible
				if (succs.Count < 2)
					succs.Add(curr + 1);
			} else if (code[curr].MatchLeave(out BlockContainer targetCon, out ILInstruction value)) {
				//Function exit?? Might need refinement in the future
			} else {
				succs.Add(curr + 1);
			}

			return succs;
		}

		public static IEnumerable<int>[] Succs(List<ILInstruction> code, bool expectUnreachable = false)
		{
			var pres = Enumerable.Range(0, code.Count).Select(x => Succ(code, x)).ToArray();
			return pres;
		}

		public static IList<int[]> Pres(IEnumerable<int>[] s)
		{
			var p = Enumerable.Range(0, s.Count())
				 .Select(i => Enumerable.Range(0, s.Count()).Where(j => s[j].Contains(i)).ToArray())
				 .ToList();
			return p;
		}		

		public static void ControlFlow(List<ILInstruction> code, out int[][] succ, out int[][] pres)
		{
			succ = Succs(code).Select(x => x.ToArray()).ToArray();
			pres = Pres(succ).Select(x => x.ToArray()).ToArray();
		}


		public struct LivenessValue
		{
			public bool LiveIn;
			public bool LiveOut;
		}
		
		public static LivenessValue[] CalcLiveness(List<ILInstruction> code, ILVariable variable, int[][] succ, int[][] pres)
		{			
			var use = code.Select(x => usesSingle(x, variable)).ToArray();
			var def = code.Select(x => defs(x, variable)).ToArray();

			var pointers = TypeInference.GetPointers(variable);
			foreach (var pointer in pointers) {
				var pointerUse = code.Select(x => usesSingle(x, pointer)).ToArray();
				use = use.Zip(pointerUse, (a, b) => a || b).ToArray();
			}


			var liveIn = new bool[code.Count];
			var liveOut = new bool[code.Count];
			var liveInOld = new bool[code.Count];
			var liveOutOld = new bool[code.Count];

			do {
				for (int i = 0; i < code.Count; i++) {
					liveInOld[i] = liveIn[i];
					liveOutOld[i] = liveOut[i];

					liveIn[i] = use[i] || (liveOut[i] && !def[i]);

					//if(pres[i].Count() > 1)
					{ // special case for branches, where live comes out only one branch
						var preLiveOut = pres[i].Any(x => liveOut[x]);
						liveIn[i] |= preLiveOut;
					}
					liveOut[i] = succ[i].Any(x => liveIn[x]);
				}

			} while (!liveInOld.SequenceEqual(liveIn) || !liveOutOld.SequenceEqual(liveOut));

			// The return value is still alive when leaving the function.		
			if (code.Last() is Leave leave && leave.IsLeavingFunction) {
				leave.Value.MatchLdLoc(out var retVar);
				if (retVar == variable) {
					liveOut[code.Count - 1] = true;
				}
			}

			var res = liveIn.Zip(liveOut, (a, b) => new LivenessValue { LiveIn = a, LiveOut = b }).ToArray();


			return res; //new Tuple<bool[], bool[]>(liveIn, liveOut);
		}

		public static int[] AddRef(LivenessValue[] live) //Tuple<bool[], bool[]> live
		{
			//
			//var live = s_CalcLiveness(code, variable, succ, pres);
			//var pres = Enumerable.Range(0, code.Count).Select(x => Pre(code, x)).ToArray();
			var res = new int[live.Count()];

			for (int i = 0; i < live.Count(); i++) {
				var lIn = live[i].LiveIn;
				var lOut = live[i].LiveOut;

				if (!lIn && lOut) {
					// add ref
					res[i] += 1;
				}

				if (lIn && !lOut) {
					// remove
					res[i] -= 1;
				}

			}			

			return res.ToArray();
		}		
	}
}
