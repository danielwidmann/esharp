using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESharp.Helpers;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ESharp.Optimizations.TypeDiscoveryOptimization
{

	class GenericOptimization
	{
		Dictionary<string, MethodDefinition> m_cache = new Dictionary<string, MethodDefinition>();

		public void Optimize(TypeDefinition t)
		{
			var newMethods = new List<MethodDefinition>();

			foreach (var m in t.Methods.Where(x => x.HasBody)) {
				for (int idx = 0; idx < m.Body.Instructions.Count; idx++) {
					var i = m.Body.Instructions[idx];
				


					if (i.Operand is MethodReference rref && rref.ContainsGenericParameter) 
					{
						var declType = rref.DeclaringType;
						IGenericInstance parent = null;
						if(rref.IsGenericInstance) {
							parent = (IGenericInstance) rref;
						} else if (declType.IsGenericInstance) {
							parent = (IGenericInstance)declType;
						}
				
						// implementation option: store original type in annotation and do this later
						if (parent.GenericArguments.Any(x => x.IsValueType)) {

							var name = rref.Name + "_" + string.Join("_", parent.GenericArguments.Select(x => x.GetElementType().Name));

							if(!m_cache.ContainsKey(name)) {
								var nm = new MethodDefinition(name, MethodAttributes.Static, ResolveGenericParameter(rref.ReturnType, parent));
								if(rref.HasThis) {
									var thisType = declType;
									if(thisType.IsValueType) {
										thisType = new PointerType(thisType);
									}
									nm.Parameters.Add(new ParameterDefinition("_this", ParameterAttributes.None, thisType));
								}
								foreach (var p in rref.Parameters) {
									nm.Parameters.Add(new ParameterDefinition(ResolveGenericParameter(p.ParameterType, parent)));
								}

								// implemet wrapper method
								var ilp = nm.Body.GetILProcessor();
								foreach(var p in nm.Parameters) {
									ilp.Emit(OpCodes.Ldarg, p);
									if (p.ParameterType.IsValueType && p.ParameterType.IsGenericParameter) {
										ilp.Emit(OpCodes.Box, p.ParameterType);
									}
								}
								ilp.Emit(OpCodes.Call, rref);

								if (nm.ReturnType.IsValueType && rref.ReturnType.IsGenericParameter) {
									ilp.Emit(OpCodes.Unbox_Any, nm.ReturnType);
								}
								ilp.Emit(OpCodes.Ret);
								
								newMethods.Add(nm);
								m_cache[name] = nm;
							}
							i.Operand = m_cache[name];
						}
					}

				}
			}

			foreach (var nm in newMethods) {
				t.Methods.Add(nm);
			}
		}

		static TypeReference ResolveGenericParameter( TypeReference type, IGenericInstance parent)
		{
			if (!type.ContainsGenericParameter)
				return type;


			if (type.IsArray) {
				Debug.Assert(false);
				return null;
			}

			if (type.IsByReference) {				
				return new PointerType(ResolveGenericParameter(type.GetElementType(), parent));
			}
			if (type.IsPointer) {
				return new PointerType(ResolveGenericParameter(type.GetElementType(), parent));
			}

			if (type.IsGenericParameter)
				return parent.GenericArguments[(type as GenericParameter).Position];

			if(type is GenericInstanceType inst) {
				GenericInstanceType nt = new GenericInstanceType(inst.ElementType);
				for (var i = 0; i < inst.GenericArguments.Count; i++) {
					if (inst.GenericArguments[i].IsGenericParameter) {
						var param = inst.GenericArguments[i] as GenericParameter;
						nt.GenericArguments.Add(parent.GenericArguments[param.Position]);
					} else {
						nt.GenericArguments.Add(inst.GenericArguments[i]);
					}					
				}

				return nt;
			}

			if (!type.HasGenericParameters)
				return type;

			Debug.Assert(false);
			return null;
		}

		static Dictionary<string, TypeDefinition> m_genericTypeCache = new Dictionary<string, TypeDefinition>();

		string GetGenericName(GenericInstanceType genericType)
		{
			var genericName = new string(genericType.ElementType.Name.TakeWhile(x=>x != '`').ToArray());
			foreach (var a in genericType.GenericArguments) {
				genericName += "_" + a.Name;
			}
			return genericName;
		}

		TypeDefinition ExpandGenericType(GenericInstanceType genericType)
		{
			var genericName = GetGenericName(genericType);

			if (m_genericTypeCache.ContainsKey(genericName))
				return m_genericTypeCache[genericName];

			var ot = genericType.Resolve();
			var nt = new TypeDefinition("", genericName, TypeAttributes.Class);
			m_genericTypeCache[genericName] = nt;

			nt.BaseType = ot.BaseType;

			// update fields
			foreach (var f in ot.Fields) {
				var ft = f.FieldType;				
				ft = ResolveGenericParameter(ft, genericType);
				
				var nf = new FieldDefinition(f.Name, f.Attributes, ft);
				nt.Fields.Add(nf);
			}

			// update Methods
			foreach (var m in ot.Methods) {
				var rt = ResolveGenericParameter(m.ReturnType, genericType);
				
				var nm = new MethodDefinition(m.Name, m.Attributes, rt);
				nt.Methods.Add(nm);
				foreach(var p in m.Parameters) {
					nm.Parameters.Add(new ParameterDefinition(p.Name, p.Attributes, ResolveGenericParameter(p.ParameterType, genericType)));
				}


				if (m.HasBody) {
					CopyTypeHelper.CopyMethodBody(m, nm);
					foreach(var v in nm.Body.Variables) {
						if(v.VariableType.ContainsGenericParameter) {
							v.VariableType = ResolveGenericParameter(v.VariableType, genericType);
							if(v.VariableType.ContainsGenericParameter || v.VariableType.IsGenericInstance) {
								v.VariableType = ExpandGenericType((GenericInstanceType)v.VariableType);
							}
						}
					}

					var ilp = nm.Body.GetILProcessor();
					foreach(var i in nm.Body.Instructions) {
						if(i.Operand is MemberReference fr && fr.ContainsGenericParameter) {
							if(fr.DeclaringType.ContainsGenericParameter) {
								fr.DeclaringType = ResolveGenericParameter(fr.DeclaringType, genericType);
								if (fr.DeclaringType.ContainsGenericParameter || fr.DeclaringType.IsGenericInstance) {
									fr.DeclaringType = ExpandGenericType((GenericInstanceType)fr.DeclaringType);
								}

							}
						}
					}
				}
			}
			
			return nt;
		}

		public void TransformValueBoxing(TypeDefinition t)
		{
			foreach (var m in t.Methods.Where(x => x.HasBody)) {
				for (int idx = 0; idx < m.Body.Instructions.Count; idx++) {
					var i = m.Body.Instructions[idx];

					// todo: move to own transform
					if (i.OpCode == OpCodes.Box || i.OpCode == OpCodes.Unbox || i.OpCode == OpCodes.Unbox_Any) {
						var ilp = m.Body.GetILProcessor();
						var mod = ModuleDefinition.CreateModule("a", ModuleKind.Dll);
						var orgType = i.Operand as TypeReference;
						var boxType = mod.ImportReference(typeof(ESharpCore.Core.Boxed<int>)).GetElementType();
						var boxedType = new GenericInstanceType(boxType);
						boxedType.GenericArguments.Add(orgType);

						var expanded = ExpandGenericType(boxedType);
						
						var boxMethod = expanded.Methods.Single(x => x.Name == i.OpCode.Name.Replace(".", "_").ToLower());

						ilp.Replace(i, ilp.Create(OpCodes.Call, boxMethod));
					}					
				}
			}
		}		
	}
}
