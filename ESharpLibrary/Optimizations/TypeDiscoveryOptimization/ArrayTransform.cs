using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESharp.UsedTypeAnalysis;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ESharp.Optimizations.TypeDiscoveryOptimization
{


	public enum EArrayType
	{ _4, _2, _1, _ref };

	class ArrayTransform
	{
		public void TransformIL(TypeDefinition t)
		{
			TypeVisitor.ReplaceTypeRefs(t, x => {
				if (x.IsArray) {
					return GetArrayType(GetArraySize(x));
				}				
				return x;
			}
			);

			foreach(var m in t.Methods) {
				if (!m.HasBody)
					continue;

				var ilp = m.Body.GetILProcessor();

				for (int idx = 0; idx < m.Body.Instructions.Count; idx++) {
					var inst = m.Body.Instructions[idx];

					if(IsLdelm(inst.OpCode)) {
						var at = GetArrayType(GetArraySize(inst.OpCode));
						var getter = at.Methods.Single(x => x.Name == "Get");
						ilp.Replace(inst, ilp.Create(OpCodes.Call, getter));
					}

					if (IsStelm(inst.OpCode)) {
						var at = GetArrayType(GetArraySize(inst.OpCode));
						var setter = at.Methods.Single(x => x.Name == "Set");
						ilp.Replace(inst, ilp.Create(OpCodes.Call, setter));
					}

					if (inst.OpCode == OpCodes.Ldlen) {
						// todo add Array_GetLength function
						var at = GetArrayType(EArrayType._1);
						var length = at.Methods.Single(x => x.Name == "GetLength");
						ilp.Replace(inst, ilp.Create(OpCodes.Call, length));
					}

					if (inst.OpCode == OpCodes.Newarr) {
						var elemType = (inst.Operand as TypeReference).Resolve();
						var arrayType = GetArrayType(GetArraySize(elemType));
						var newRef = arrayType.Methods.Single(x => x.Name == "new");
						ilp.Replace(inst, ilp.Create(OpCodes.Call, newRef));
					}

					if (inst.OpCode == OpCodes.Ldelema || inst.OpCode == OpCodes.Ldelem_Any || inst.OpCode == OpCodes.Stelem_Any) {
						throw new NotImplementedException();
					}					
				}
			}			
		}		

		readonly ModuleDefinition m_mod = ModuleDefinition.CreateModule("a", ModuleKind.Dll);

		public Mono.Cecil.TypeDefinition GetArrayType(EArrayType type)
		{
			var assembly = m_mod.ImportReference(typeof(EObject)).Resolve().Module;			
			var intarray = assembly.Types.FirstOrDefault(x => x.Name == "Array" + type.ToString());

			Debug.Assert(intarray != null);

			return intarray;
		}

		public static EArrayType GetArraySize(TypeReference typeRef)
		{
			var typeRefName = typeRef.Name.Replace("[]", "");
			if (typeRefName == "Int32" || typeRefName == "UInt32") {
				return EArrayType._4;
			}
			if (typeRefName == "Byte" || typeRefName == "SByte") {
				return EArrayType._1;
			}
			if (!typeRef.IsValueType) {
				return EArrayType._ref;
			}

			throw new Exception("Array type not supported: " + typeRef.Name);
		}

		static EArrayType GetArraySize(OpCode code)
		{
			if(code == OpCodes.Stelem_I4 || code == OpCodes.Ldelem_I4) {
				return EArrayType._4;
			}
			if (code == OpCodes.Stelem_I1 || code == OpCodes.Ldelem_I1 || code == OpCodes.Ldelem_U1) {
				return EArrayType._1;
			}

			if (code == OpCodes.Ldelem_Ref || code == OpCodes.Stelem_Ref) {
				return EArrayType._ref;
			}

			throw new Exception("Unkown code array size " + code);
		
		}

		static bool IsLdelm(OpCode code)
		{		

			if (code == OpCodes.Ldelem_I1 
				|| code == OpCodes.Ldelem_U1
				|| code == OpCodes.Ldelem_I2
				|| code == OpCodes.Ldelem_U2
				|| code == OpCodes.Ldelem_I4
				|| code == OpCodes.Ldelem_U4
				|| code == OpCodes.Ldelem_I8
				|| code == OpCodes.Ldelem_I
				|| code == OpCodes.Ldelem_R4
				|| code == OpCodes.Ldelem_R8
				|| code == OpCodes.Ldelem_Ref) {
				return true;
			}

			return false;

		}

		static bool IsStelm(OpCode code)
		{
			if (code == OpCodes.Stelem_I
				|| code == OpCodes.Stelem_I1
				|| code == OpCodes.Stelem_I2
				|| code == OpCodes.Stelem_I4
				|| code == OpCodes.Stelem_I8
				|| code == OpCodes.Stelem_R4
				|| code == OpCodes.Stelem_R8
				|| code == OpCodes.Stelem_Ref) {
				return true;
			}

			return false;

		}
	}
}
