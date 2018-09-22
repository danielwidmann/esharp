using ESharp.Optimizations.IL;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Mono.Cecil.Cil;

namespace ESharp.Library.Optimizations.IL
{
    class PrimitiveDefaultValues : IILTranform
    {
        public void TransformIL(IEnumerable<TypeDefinition> types)
        {
            foreach (var t in types)
            {
                foreach (var m in t.Methods.Where(x => x.HasBody))
                {
                    foreach(var i in m.Body.Instructions)
                    {
                        if(i.OpCode == OpCodes.Initobj
                            && (i.Previous.OpCode == OpCodes.Ldloca || i.Previous.OpCode == OpCodes.Ldloca_S))
                        {
                            var type = (TypeReference)i.Operand;
                            var variable = (VariableDefinition)i.Previous.Operand;

                            if (type.IsPrimitive)
                            {                               
                                if(type.Name == "Int32")
                                {
                                    i.Previous.OpCode = OpCodes.Ldc_I4_0;
                                    i.Previous.Operand = null;
                                    i.OpCode = OpCodes.Stloc;
                                    i.Operand = variable;
                                } else
                                {
                                    throw new NotImplementedException("No default for " + type.Name);
                                }
                            }                                
                        }
                    }


                }


            }
        }
    }
}
