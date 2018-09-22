using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESharp.Helpers;
using ICSharpCode.Decompiler.ECS;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.IL.Transforms;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.TypeSystem.Implementation;
using Mono.Cecil;

namespace ESharp.Optimizations.ILAst
{
	public class RefCounting : IILTransform
	{
		public void Run(ILFunction function, ILTransformContext context)
		{
			var insts = LivenessHelper.GetInstructions(function);

			if (insts.Count == 1) {
				// empty body, skip method
				return;
			}

			// skip marked methods
			//if (function.CecilMethod.CustomAttributes.Any(x => x.AttributeType.Name == "ManualRefCounting"))
			if (function.Method.GetAttributes().Any(x => x.AttributeType.Name == "ManualRefCounting"))
				return;

			IEnumerable<ILVariable> vars = function.Variables;

			var usedVars = new List<ILVariable>();
			foreach (var v in vars) {
				//var aliasSource = TypeInference.GetAliasSource(v);

				//// don't ref count parameters like this. Todo check for out and ref parameters (does O work already?)
				//if (aliasSource?.Kind == VariableKind.Parameter && aliasSource.StackType == StackType.O)
				//	continue;

				// no typeof intermediate variables
				if (v.StoreInstructions.Count == 1 && (v.StoreInstructions.First() as StLoc)?.Value is LdTypeToken)
					continue;

				// skip variables that only contain NULL, etc
				if (TypeInference.GetVariableType(v) == null)
					continue;

				// only reference types or valuetypes that require ref counting
				if (TypeInference.GetVariableType(v).Kind == TypeKind.Pointer) {

					continue;

					var pType = TypeInference.GetVariableType(v) as ICSharpCode.Decompiler.TypeSystem.PointerType;
					var elemType = pType.ElementType;

					//if (!elemType.IsReferenceType.Value)
					//	continue;
					if (!elemType.IsReferenceType.Value && !elemType.GetMethods().Any(x => x.Name.EndsWith("_AddRef")))
						continue;
				} else {
					if (!TypeInference.GetVariableType(v).IsReferenceType.Value && !TypeInference.GetVariableType(v).GetMethods().Any(x => x.Name.EndsWith("_AddRef")))
						continue;
				}


				// references are not supported in C
				Debug.Assert(TypeInference.GetVariableType(v).Kind != TypeKind.ByReference);


				//todo: are the following two needed?
				//if (TypeInference.GetVariableType(v).Kind == TypeKind.Pointer)
				//	continue;

				//if (TypeInference.GetVariableType(v).Kind == TypeKind.ByReference)
				//	continue;

				usedVars.Add(v);
			}

			vars = usedVars.ToArray();

			var liveDebug = DebugLiveness.DebugLocalLiveness(function, vars);
			AddRefCountingLocalLiveness(insts, vars);
			
			ILAstDebugPrinter.DebugIlAst(function, "after_ref", liveDebug);		

			// Add casts to make compiler happy
			// Todo: I would prefer to access base field insead. No easy representation at this stage though.
			// Todo: move to own optimization
			foreach (var c in function.Body.Descendants.OfType<CallInstruction>()) {
				for (int idx = 0; idx < c.Method.Parameters.Count; idx++) {
					var p = c.Method.Parameters[idx];
					var value = c.Arguments[idx];

					var argType = TypeInference.GetInstType(value);
					var pType = p.Type;

					if (argType != null && argType != pType) {
						value.ReplaceWith(new CastClass(value.Clone(), pType));
					}
				}

			}
		}

		static void InsertAfter(ILInstruction inst, ILInstruction newInst)
		{
			if (newInst == null)
				return;

			var insts = ((Block)inst.Parent).Instructions;
			var idx = insts.IndexOf(inst);
			insts.Insert(idx + 1, newInst);

		}

		static void InsertBefore(ILInstruction inst, ILInstruction newInst)
		{
			if (newInst == null)
				return;

			var insts = ((Block)inst.Parent).Instructions;
			var idx = insts.IndexOf(inst);
			insts.Insert(idx, newInst);
		}

		static IType GetETypeBase(IType t)
		{
			if (t == null)
				return null;
			if (t.Name == "EObject")
				return t;

			foreach (var b in t.DirectBaseTypes) {
				var baseEType = GetETypeBase(b);
				if (baseEType != null) {
					return baseEType;
				}
			}

			return null;
		}

		static ILInstruction GetRef(ILVariable v, string Name)
		{

			var ty = TypeInference.GetVariableType(v);
			if (ty == null) {
				// happens on ldNull;
				return null;
			}
			var isPointer = false;
			if (ty.Kind == TypeKind.Pointer) {
				ty = ((ICSharpCode.Decompiler.TypeSystem.PointerType)ty).ElementType;
				isPointer = true;
			}

			IType etype;
			ILInstruction loadInst;

			if (ty.IsReferenceType.Value) {
				etype = GetETypeBase(ty);
				if (isPointer) {
					loadInst = new LdObj(new LdLoc(v), ty);

				} else {
					loadInst = new LdLoc(v);
				}





				if (etype == null && !ty.FullName.StartsWith("System."))
					Debug.Assert(false, "Type is broken??");

				// only do ref counting for our own types
				if (etype == null)
					return null;

				// avoid compiler warnings
				loadInst = new CastClass(loadInst, etype);

			} else { // value type
				etype = ty;

				if (v.StackType == StackType.O) {
					loadInst = new Conv(new LdLoca(v), PrimitiveType.I, false, Sign.None);
				} else {
					loadInst = new Conv(new LdLoc(v), PrimitiveType.I, false, Sign.None);
				}

			}


			// make sure we don't end up with crazy unintended names
			//v.HasGeneratedName = false;

			//var test = (DefaultResolvedTypeDefinition)ty;

			var m = etype.GetMethods().First(x => x.Name.EndsWith(Name + "Ref"));
			var inst = new Call(m);
			inst.Arguments.Add(loadInst);

			return inst;
		}

		public static void CalcReturnRefAdjust(List<ILInstruction> code, ILVariable variable, int[] refAdjusts)
		{
			for (int i = 0; i < code.Count; i++) {
				if (code[i].MatchStLoc(variable, out var value) && value is CallInstruction) {
					refAdjusts[i] -= 1;
				}
			}
		}


		public static void CalcFieldRefAdjust(List<ILInstruction> code, ILVariable variable, int[] refAdjusts)
		{

			for (int i = 0; i < code.Count; i++) {


				var exp = code[i];
				if (exp is StObj stobj) {
					if (stobj.Value is LdLoc ldloc && ldloc.Variable == variable) {
						// don't need to check which field. Refcount should always be increased.
						refAdjusts[i]++;
					}
				}
			}
		}

		// True if v is only live while source is alive
		static bool shorterLive(Liveness.LivenessValue[] source, Liveness.LivenessValue[] v)
		{
			for (int i = 0; i < source.Length; i++) {
				if (v[i].LiveIn && !source[i].LiveIn)
					return false;

				if (v[i].LiveOut && !source[i].LiveOut)
					return false;
			}

			return true;
		}


		public static void AddRefCountingLocalLiveness(List<ILInstruction> code, IEnumerable<ILVariable> variables)
		{
			// First calculate everything we need to make decisions
			Liveness.ControlFlow(code, out int[][] succ, out int[][] pres);
			var liveness = new Dictionary<ILVariable, Liveness.LivenessValue[]>();
			foreach (var v in variables) {
				liveness[v] = Liveness.CalcLiveness(code, v, succ, pres);
			}

			foreach (var v in variables) {
				if (v.Kind == VariableKind.Parameter) {
					// normal parameters are always alive
					// todo: handle out/ref parameters. 
					liveness[v] = code.Select(x => new Liveness.LivenessValue { LiveIn = true, LiveOut = true }).ToArray();
				}
			}

			var refAdjusts = new Dictionary<ILVariable, int[]>();

			foreach (var v in variables) {

				// todo move to own intialization
				// add null initializer if unassinged variable is used.
				var live = liveness[v];
				if (live[0].LiveIn && v.Type.Kind != TypeKind.Pointer && v.Type.IsReferenceType.Value && v.Kind != VariableKind.Parameter)
					((Block)code[0].Parent).Instructions.Insert(0, new StLoc(v, new LdNull()));

				int[] addRef;


				var aliasSource = TypeInference.GetAliasSource(v);

				//// don't ref count parameters like this. Todo check for out and ref parameters (does O work already?)
				//if (aliasSource?.Kind == VariableKind.Parameter && aliasSource.StackType == StackType.O)
				//	continue;			


				addRef = Liveness.AddRef(live);

				// don't need to adjust variable that is a short lived copy of a live varaible
				if (v.IsSingleDefinition && aliasSource != null && liveness.ContainsKey(aliasSource) && shorterLive(liveness[aliasSource], liveness[v])) {
					addRef = addRef.Select(x => 0).ToArray();
				}



				CalcReturnRefAdjust(code, v, addRef);

				CalcFieldRefAdjust(code, v, addRef);

				refAdjusts[v] = addRef;


			}
			// don't to do an extra add/rem cycle when something is passed from one variable to another
			foreach (var v in variables) {
				var aliasSource = TypeInference.GetAliasSource(v);
				if (v.IsSingleDefinition && aliasSource != null) {
					var addIdx = Array.IndexOf(refAdjusts[v], 1);

					if (addIdx != -1 && refAdjusts.ContainsKey(aliasSource) && refAdjusts[aliasSource][addIdx] == -1) {
						Debug.Assert(refAdjusts[v][addIdx] + refAdjusts[aliasSource][addIdx] == 0);

						// remove the redundant addRef and remRef
						refAdjusts[v][addIdx] = 0;
						refAdjusts[aliasSource][addIdx] = 0;
					}
				}
			}

			foreach (var v in variables) {
				var addRef = refAdjusts[v];
				for (int i = 0; i < code.Count; i++) {


					//newBody.Add(body[i]);
					Debug.Assert(Math.Abs(addRef[0]) <= 1);

					if (addRef[i] < 0) {
						//todo
						//if (newBody.Last().IsConditionalControlFlow()
						//  || newBody.Last().IsUnconditionalControlFlow()) { // there can be the (rare) case that we need a remove before return.
						//													// (Or potentially brach) Should only happen if life comes out of a branch  
						//	newBody.Insert(newBody.Count - 1, RemoveRef(v));
						//} else {

						//newBody.Add(RemoveRef(v));

						InsertBefore(code[i + 1], GetRef(v, "Remove"));

						//}
					}

					if (addRef[i] > 0) {
						InsertAfter(code[i], GetRef(v, "Add"));
						//newBody.Add(AddRef(v));
					}
				}
				//body = newBody;
			}

			// Remove ref of field before it is overwritten
			for (int i = 0; i < code.Count; i++) {
				if (code[i].MatchStObj(out var target, out var value, out var type) && (target is IInstructionWithFieldOperand ldflda)) {					
					var field = ldflda.Field;
					var etype = GetETypeBase(field.DeclaringType);

					if (etype == null)
						continue;

					if (!field.Type.IsReferenceType.Value)
						continue;

					var m = etype.GetMethods().First(x => x.Name.EndsWith("RemoveRef"));
					var inst = new Call(m);
					inst.Arguments.Add(new LdObj(target.Clone(), type));

					InsertBefore(code[i], inst);					
				}

			}

		}

	}
}
