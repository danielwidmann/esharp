using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ICSharpCode.Decompiler.ECS
{
    public class RenameTransform
    {        
        public static string CTypeName(TypeReference typeref)
        {
            var name = typeref.Name;
            if (typeref.IsNested)
            {
                name = CTypeName(typeref.DeclaringType) + "_" + name;
            }

            name = ConvertIdentifier(name);

            return name;
        }

        private static string ConvertIdentifier(string name)
        {
            if(char.IsDigit(name[0]))
                name = "_" + name;

            name = name.Replace("<", "");
            name = name.Replace(">", "");
            name = name.Replace("$", "");
			name = name.Replace("/", "_");
			name = name.Replace(".", "");
			name = name.Replace("`", "");
			return name;
        }

        static public string CFunctionName(MethodReference methodRef)
        {
            var simpleName = CTypeName(methodRef.DeclaringType) + "_" + methodRef.Name;


            simpleName = ConvertIdentifier(simpleName);

            // use legitimate name for c ctor
            //simpleName = simpleName.Replace(".ctor", "ctor");

            // the original method potentially was already renamed. Also look for renamed variants
            var methods = methodRef.DeclaringType.Resolve().Methods.Where(x => x.Name == methodRef.Name || x.Name.StartsWith(simpleName + "_"));
            if (methods.Count() == 1 || methodRef.Parameters.Count == 0)
            {
                return simpleName;
            }
            else
            { // there are overloads for this method
                return simpleName + "_" + string.Join("_", methodRef.Parameters.Select(x => x.ParameterType.Name));
            }
        }

        static public string StaticFieldName(FieldReference fieldRef)
        {
            if (fieldRef.Name.StartsWith(CTypeName(fieldRef.DeclaringType) + "_"))
            {
                Debug.Assert(false, "Class name should not be part of static field");
            }


            var simpleName = "S_" + CTypeName(fieldRef.DeclaringType) + "_" + fieldRef.Name;


            simpleName = ConvertIdentifier(simpleName);

            return simpleName;
        }

        static public string CFieldName(FieldReference fieldRef)
        {
            var simpleName = fieldRef.Name;

            simpleName = ConvertIdentifier(simpleName);

            if (Regex.IsMatch(simpleName, @"^\d+"))
            {
                simpleName = "_" + simpleName;
            }

            return simpleName;
        }

        static public void Rename(IEnumerable<TypeDefinition> types)
        {
            foreach (var type in types)
            {

                Rename(type);

                foreach (var m in type.Methods)
                {
                    Rename(m);
                }

                foreach (var f in type.Fields)
                {
                    Rename(f);
                }
            }

            //unmark nested
            foreach (var type in types)
            {
                if (type.DeclaringType == null)
                    continue;

                type.DeclaringType.NestedTypes.Clear();
                type.DeclaringType = null;
            }

        }

        static void Rename(TypeDefinition type)
        {
            type.Name = CTypeName(type);
        }

        static void Rename(FieldDefinition field)
        {
            field.Name = CFieldName(field);

			if(field.IsStatic) {
				field.Name = StaticFieldName(field);
			}
        }


        static void Rename(MethodDefinition method)
        {
            // remove override as this might appear in function name
            method.Overrides.Clear();

            // all Methods need to be renamed            
            method.Name = CFunctionName(method);            

            if (method.Body == null) return;            
        }
    }
}
