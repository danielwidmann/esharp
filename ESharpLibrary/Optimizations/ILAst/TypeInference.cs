using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.TypeSystem;

namespace ESharp.Optimizations.ILAst
{
	public class TypeInference
	{

		public static IType GetInstType(ILInstruction inst, IType typeHint = null)
		{
			if(inst.MatchCastClass(out ILInstruction arg, out IType type)) {
				return type;
			}

			var call = inst as CallInstruction;
			if (call != null) { 
				return call.Method.ReturnType;
			}

			if(inst.MatchLdLoc(out ILVariable var)) {
				return GetVariableType(var);
			}

			if(inst.MatchLdNull()) {
				return null;
			}

			if (inst.MatchLdStr(out string xx)) {
				// we don't handle string literals. Should be replaced finally
				return null;
			}

			if(inst.MatchLdObj(out var target, out var fldType)) {
				if(target.MatchLdsFlda(out var sfield))
					return sfield.Type;

				if (target.MatchLdFlda(out var target2, out var field))
					return field.Type;
			}
			if (inst.MatchLdLoca(out var v)) {
				return new PointerType(v.Type);				
			}

			if (inst.MatchLdFlda(out var target3, out var field2)) {
				return new PointerType(field2.Type);				
			}

			if (inst.MatchLdsFlda(out var field4)) {
				return new PointerType(field4.Type);
			}

			if (inst.MatchLdObj(out var target4, out var type3)) {
				return type3;				
			}


			if(inst.MatchLdTypeToken(out var type4)) {
				return null;
			}

			if(inst is Conv c) {
				// todo handle integer types
				return null;
			}
			// todo field, etc
			Debug.Assert(false, "Implement this type: " + inst.GetType().Name);
			return null;
		}

		public static IType GetVariableType(ILVariable v)
		{
			if(v.Kind != VariableKind.StackSlot) {
				return v.Type;
			}

			// In may cases IlSpy has already figured it out.
			// IlSpy is not quite right in many cases!! don't trust it!
			if (v.Type != null && v.Type.Kind != TypeKind.Unknown 
				&& (v.Type.IsCSharpPrimitiveIntegerType() || v.Type.Name == "IntPtr"))
				return v.Type;

			var store = (StLoc)v.StoreInstructions.First();

			return GetInstType(store.Value);
		}

		public static ILVariable GetAliasSource(ILVariable v)
		{
			if (!v.IsSingleDefinition)
				return null;

			if (v.StoreInstructions.Count != 1)
				return null;

			var store = (StLoc)v.StoreInstructions.Single();
			if(store.Value.MatchLdLoc(out var ldvar)) {
				return ldvar;
			}

			if (store.Value.MatchLdLoca(out var ldavar)) {
				return ldavar;
			}
			//if (store.)

			return null;

		}

		public static ILVariable[] GetPointers(ILVariable v)
		{
			var ldaPointers = v.AddressInstructions.Select(x => (x.Parent as StLoc)?.Variable);
			var ldFieldaPointers = v.LoadInstructions.Select(x => ((x.Parent as LdFlda)?.Parent as StLoc)?.Variable);
			var convPointers = v.LoadInstructions.Select(x => ((x.Parent as Conv)?.Parent as StLoc)?.Variable);

			var directPointers = ldaPointers.Concat(ldFieldaPointers).Concat(convPointers).Where(x => x != null);
			var derived = directPointers.SelectMany(x => GetPointers(x));



			//return ldFieldaPointers.ToArray();
			return directPointers.Concat(derived).ToArray();
		}


		//public static void VisitStLoc(StLoc inst)
		//{
		//	var translatedValue = GetInstType(inst.Value);
		//	if (inst.Variable.Kind == VariableKind.StackSlot ) { //&& !loadedVariablesSet.Contains(inst.Variable)
		//		// Stack slots in the ILAst have inaccurate types (e.g. System.Object for StackType.O)
		//		// so we should replace them with more accurate types where possible:
		//		if ((inst.Variable.IsSingleDefinition || IsOtherValueType(translatedValue.Type) || inst.Variable.StackType == StackType.Ref)
		//				&& inst.Variable.StackType == translatedValue.Type.GetStackType()
		//				&& translatedValue.Type.Kind != TypeKind.Null) {
		//			inst.Variable.Type = translatedValue.Type;
		//		} else if (inst.Value.MatchDefaultValue(out var type) && IsOtherValueType(type)) {
		//			inst.Variable.Type = type;
		//		}
		//	}		

		//	bool IsOtherValueType(IType type)
		//	{
		//		return type.IsReferenceType == false && type.GetStackType() == StackType.O;
		//	}
		//}
	}
}
