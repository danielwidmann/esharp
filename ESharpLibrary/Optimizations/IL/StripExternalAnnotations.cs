using ESharp.Optimizations.IL;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESharp.Library.Optimizations.IL
{
    class StripExternalAnnotations : IILTranform
    {        
        public void TransformIL(IEnumerable<TypeDefinition> types)
        {
            foreach (var t in types)
            {
                CheckAttributes(t.CustomAttributes);
                foreach (var m in t.Methods)
                {
                    CheckAttributes(m.CustomAttributes);
                }
            }
        }

        private void CheckAttributes(Mono.Collections.Generic.Collection<CustomAttribute> CustomAttributes)
        {
            for (int i = 0; i < CustomAttributes.Count(); i++)
            {
                var a = CustomAttributes[i];
                
                if (a.AttributeType.Namespace != "ESharp.Annotations")
                {
                    CustomAttributes.Remove(a);
                    i--;
                }


            }
        }
    }

}
