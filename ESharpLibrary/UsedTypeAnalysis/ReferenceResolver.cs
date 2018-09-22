using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ICSharpCode.Decompiler.ECS
{
    public class ReferenceResolver
    {
        public HashSet<TypeDefinition> m_usedTypes;
        public ReferenceResolver(HashSet<TypeDefinition> usedTypes)
        {
            m_usedTypes = usedTypes;
        }

        public ReferenceResolver(IEnumerable<TypeDefinition> usedTypes)
        {
            m_usedTypes = new HashSet<TypeDefinition>(usedTypes);
        }

        public TypeReference GetTypeReference(Type t)
        {
            return GetTypeReference(t.Name);
        }

        public TypeReference GetTypeReference(String s)
        {
            return m_usedTypes.Single(x => x.Name == s);
        }

        public TypeReference GetTypeReference(TypeReference t)
        {
            return GetTypeReference(t.Name);
        }

        public TypeReference GetStringType()
        {
            return GetTypeReference("EObject").Module.Import(typeof(string));
        }


        public MethodReference GetMethodReference(Expression<Action> expression)
        {
            var info = SymbolExtensions.GetMethodInfo(expression);
            var type = m_usedTypes.Single(x => info.DeclaringType.Name == x.Name);
            var method = type.Methods.Single(x => x.Name.Contains(info.Name));

            return method;
        }

        public MethodReference GetMethodReferenceInternal(Expression<Action> expression)
        {
            var info = SymbolExtensions.GetMethodInfo(expression);

            var m = ModuleDefinition.CreateModule("t", ModuleKind.Dll).Import(info);
            return m;
        }

        public MethodReference GetConstructorReferenceInternal(Expression<Action> expression)
        {
            var info = SymbolExtensions.GetCtorInfo(expression);

            var m = ModuleDefinition.CreateModule("t", ModuleKind.Dll).Import(info);
            return m;
        }

        public MethodDefinition GetMethodReference(TypeReference t, string name, IEnumerable<string> parameterTypes = null, bool after_rename = false)
        {
            if (after_rename)
            { // adjust for renaming
                name = t.Name + "_" + name;
            }

            var type = m_usedTypes.SingleOrDefault(x => x.FullName == t.FullName);
			if (type == null)
				return null;

            var methods = type.Methods.Where(x => x.Name == name);
            if (parameterTypes == null)
            {
                return methods.Single();
            }
            else
            {
                var matchingParameters = methods.Where(m => m.Parameters.Count == parameterTypes.Count());
                if (matchingParameters.Count() == 1)
                {
                    return matchingParameters.Single();
                }
                else
                {
                    var requestedParameters = String.Join(",", parameterTypes);
                    try
                    {
                        return methods.Single(x => GetParameterTypes(x) == requestedParameters);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Couldn't find method: "+ t.Name + "." + name);
                    }
                }
            }
        }

        //public MethodDefinition GetMethodReference(MethodReference reference)
        //{
        //    return GetMethodReference(reference.DeclaringType, reference.Name, GetParameterTypes(reference));
        //}

        public static string GetParameterTypes(MethodReference reference)
        {
            return string.Join(",", reference.Parameters.Select(p => p.ParameterType.Name));
        }


        public FieldReference GetField<T>(Expression<Func<T>> expression)
        {
            var info = SymbolExtensions.GetFieldInfo((LambdaExpression)expression);

            return GetField(info.DeclaringType.FullName, info.Name);
        }

        public FieldDefinition GetField(string typeName, string fieldName)
        {
            var type = m_usedTypes.Single(x => typeName == x.FullName);
            var field = type.Fields.Single(x => x.Name == fieldName);
            return field;
        }

        public Mono.Cecil.MethodReference GetTypeHandle()
        {
            ModuleDefinition corlib = ModuleDefinition.ReadModule(typeof(object).Module.FullyQualifiedName);

            var tMeta = corlib.Types.FirstOrDefault(x => x.Name == "Type");

            return tMeta.Methods.FirstOrDefault(x => x.Name == "GetTypeFromHandle"); ;
        }

        public MethodReference GetCtor(TypeReference t)
        {
            try
            {
                var ctor = GetMethodReference(t, ".ctor", after_rename: false);
                return ctor;
            }
            catch (Exception e)
            { // ctor doesn't exist. Create it.
                var ty = t as TypeDefinition;
                Debug.Assert(!ty.Methods.Any(x => x.Name.Contains("ctor")));
                var me = new MethodDefinition(".ctor", MethodAttributes.Public, t.Module.Import(typeof(void)));
                ty.Methods.Add(me);

                me.Body = new MethodBody(me);
                me.Body.GetILProcessor().Emit(OpCodes.Ret);

                return me;
            }
        }
    }
}
