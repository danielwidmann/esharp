using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ICSharpCode.Decompiler.ECS
{

    public class ModuleReferences
    {
        static AssemblyDefinition s_ecsharp;
        //public static void SetECSharpRef(AssemblyDefinition assembly)
        //{
        //    s_ecsharp = assembly;
        //}

        public static AssemblyDefinition GetECSharpRef()
        {
            if(s_ecsharp == null)
            {
                var assembly = typeof(ESharp.EObject).Assembly;
                s_ecsharp = Mono.Cecil.AssemblyDefinition.ReadAssembly(assembly.Location);
            }

            return s_ecsharp;            
        }


        static TypeDefinition s_eobj;
        public static TypeDefinition GetEObjRef()
        {
            //if (s_eobj == null)
            {

                s_eobj = GetECSharpRef().MainModule.GetType("ESharp.EObject");
            }

            return s_eobj;
        }

        static TypeDefinition s_seobj;
        public static TypeDefinition GetStructEObjRef()
        {
           // if (s_seobj == null)
            {

                s_seobj = GetECSharpRef().MainModule.GetType("ESharp.struct_EObject");
            }

            return s_seobj;
        }
    }
}
