using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.ECS;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ESharp.Optimizations.IL
{	

	public class StringLiteralOptimization //: IILTranform
	{

		StringLiteralDictionary dict;

		Dictionary<String, byte[]> array_dict;

		public void GlobalOptimization(List<Mono.Cecil.TypeDefinition> types)
		{
			ReferenceResolver resolver = new ReferenceResolver(types);
			dict = new StringLiteralDictionary(resolver);
			foreach (var t in types) {
				foreach (var method in t.Methods) {
					if (method.Body == null)
						continue;
					Instruction prev_inst = null;
					foreach (var inst in method.Body.Instructions) {
						if (inst.OpCode == OpCodes.Ldstr) {
							inst.OpCode = OpCodes.Ldsfld;
							var stringValue = (string)inst.Operand;
							inst.Operand = dict.AddString(stringValue);
							//inst.
						}

						// array initializer
						if (inst.OpCode == OpCodes.Call
							&& (inst.Operand as MethodReference).Name.Contains("InitializeArray")
							&& (inst.Operand as MethodReference).DeclaringType.Name.Contains("RuntimeHelpers")) {
							var fieldRef = (FieldReference)prev_inst.Operand;
							var data = fieldRef.Resolve().InitialValue;
							// todo convert data to actual values. 

							prev_inst.Operand = dict.AddArray_1(data);
							prev_inst.OpCode = OpCodes.Ldsfld;
						}

						prev_inst = inst;
					}
				}
			}

			//      todo iterate over il, replace ldstring

			//add new class with custom c code
			types.Add(dict.GetStringsType());

			

		}		
	}
}
