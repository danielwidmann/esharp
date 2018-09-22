using EcsHelper;
using Es.Helper;
using ESharp;
using ESharp.Helpers;
using ESharp.Optimizations.IL;
using ICSharpCode.Decompiler.ECS;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.IL.Transforms;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESharp.Optimizations.IL
{
	public class ValueTypeHelper
	{
		public static bool NeedRefCounting(TypeDefinition t)
		{
			return RefCountingFields(t).Any();
		}

		public static IEnumerable<FieldDefinition> RefCountingFields(TypeDefinition t)
		{
			Debug.Assert(t.IsValueType);
			return t.Fields.Where(x => !x.FieldType.Resolve().IsValueType && !x.IsStatic);
		}


	}

    //public class Helper
    //{
    //    static public EObject Adr(EObject a)
    //    {
    //        return a;
    //    }
    //}


    //todo: always use struct (generate struct as normal type? use seperate boxed typeid)
    //todo: memset on initobj
    public class ValueTypeOptimization: IILTranform
	{
        //ReferenceResolver m_resolver;
        public ValueTypeOptimization(/*ReferenceResolver resolver*/)
        {
            //m_resolver = resolver;
        }


		public void TransformIL(IEnumerable<TypeDefinition> types)
		{
			var resolver = new ReferenceResolver(types);


			foreach (var t in types) {

				if (t.IsValueType) {
					// add initobj method
					var nm = new MethodDefinition(t.Name + "_initobj", MethodAttributes.Static, t.Module.ImportReference((typeof(void))));
					nm.Parameters.Add(new ParameterDefinition("_this", ParameterAttributes.None, new PointerType(t)));
					var code = "memset(_this, 0, sizeof(*_this));";
					EmitSource.SourceImplementation(nm, code);
					t.Methods.Add(nm);				


					if (ValueTypeHelper.NeedRefCounting(t)) {
						foreach(var methodName in new string[] { "AddRef", "RemoveRef" }) {

							// add ref adjustment methods
							nm = new MethodDefinition(t.Name + "_" + methodName, MethodAttributes.Static, t.Module.ImportReference((typeof(void))));
							nm.Parameters.Add(new ParameterDefinition("_vt", ParameterAttributes.None, new PointerType(t)));						
							t.Methods.Add(nm);

							var procs = nm.Body.GetILProcessor();

							foreach (var field in t.Fields.Where(x=>!x.FieldType.IsValueType)) {							

								var refMethod = types.Single(x => x.Name == "EObject").Methods.Single(x => x.Name.EndsWith("EObject_" + methodName)); // 
								procs.Emit(OpCodes.Ldarg_0);
								procs.Emit(OpCodes.Ldfld, field);
								procs.Emit(OpCodes.Call, refMethod);								
							}
							procs.Emit(OpCodes.Ret);
						}
					}					

					
					var finalizer = t.Methods.Single(x => x.Name == t.Name + "_Finalize");
					//if (!ValueTypeHelper.NeedRefCounting(t)) {
					t.Methods.Remove(finalizer);
					
				}
			}

			

			foreach (var t in types) {


				foreach (var m in t.Methods)
                {

					// C only knows pointers but no references. Replace with pointers.
					foreach(var p in m.Parameters) {
						if(p.ParameterType is ByReferenceType rt) {
							p.ParameterType = new PointerType(rt.ElementType);
						}
					}

                    if(!m.HasBody)
                        continue;

                    var newBody = new List<Instruction>();
                    var ilp = m.Body.GetILProcessor();
                    // replace default with new
                    for (int idx = 0; idx < m.Body.Instructions.Count(); idx++)
                    {
                        var inst = m.Body.Instructions[idx]; 
                        newBody.Add(inst);    
                        
                        if(inst.OpCode == OpCodes.Initobj)
                        {							
							var initType = inst.Operand as TypeDefinition;
							Debug.Assert(initType.IsValueType, initType.Name + " should be a Value Type");

							// adjust ref counting before memset
							// todo: skip this when struct is unused.
							if (ValueTypeHelper.NeedRefCounting(initType)) {															
									ilp.InsertBefore(inst, ilp.Create(OpCodes.Dup));
									ilp.InsertBefore(inst, ilp.Create(OpCodes.Call, initType.Methods.Single(x => x.Name.EndsWith("RemoveRef"))));								
							}

							if (initType.IsPrimitive) {
								ilp.InsertBefore(inst, Instruction.Create(OpCodes.Ldc_I4, 0));
								ilp.Replace(inst, Instruction.Create(OpCodes.Stobj, initType));
							} else {
								var initMethod = initType.Methods.Single(x => x.Name.EndsWith("_initobj"));								
								ilp.Replace(inst, Instruction.Create(OpCodes.Call, initMethod));
							}


						}

						
						// try to use pointers instead of references
						if (inst.OpCode == OpCodes.Ldflda) {
							var field = (inst.Operand as FieldDefinition);
							ilp.InsertAfter(inst, ilp.Create(OpCodes.Conv_I));
							idx++;
						}

						
					}
                    //m.Body.Instructions = newBody;
                    IlHelper.UpdateIlOffsets(m.Body);

                    // initalizing should be done in LocalVariableInitializaionOptimization
                    //continue;

                    foreach(var v in m.Body.Variables)
                    {

						// remove ref for valuetypes going out of scope.
						if(false) { // currently nor needed, done in ref counting
							if(v.VariableType.Resolve().IsValueType && ValueTypeHelper.NeedRefCounting(v.VariableType.Resolve())) {
								// Afaik, only one return in functions 
								var ret = ilp.Body.Instructions.Single(x => x.OpCode == OpCodes.Ret);

								if (ret.Operand != v) {
									ilp.InsertBefore(ret, ilp.Create(OpCodes.Ldloca, v));
									ilp.InsertBefore(ret, ilp.Create(OpCodes.Call, v.VariableType.Resolve().Methods.Single(x => x.Name.EndsWith("RemoveRef"))));
								}								
							}
						}                                                
                    }
                }
            }
        }       
	}
}
