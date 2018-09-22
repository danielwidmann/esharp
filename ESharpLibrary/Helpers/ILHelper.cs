using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcsHelper
{
    public class IlHelper
    {
        public static void UpdateIlOffsets(MethodBody method)
        {
            foreach (var inst in method.Instructions)
            {
                if (inst.Next != null)
                {
                    inst.Next.Offset = inst.Offset + inst.GetSize();
                }
            }
        }
    }
}
