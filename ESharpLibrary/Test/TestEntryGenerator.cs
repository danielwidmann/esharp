using System;
using System.Collections.Generic;
using System.Text;

using EcsHelper;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.IO;
using System.Linq;

namespace ESharpLibrary.Test
{
    public class TestEntryGenerator
    {

        public static TypeDefinition GenerateTestEntry(MethodDefinition[] originalTests, ModuleDefinition mod)
        {
            var originalTest = originalTests.First();
            var entryClass = CreateNewClass(mod, originalTest.DeclaringType.Name + "_ETest");
            entryClass.Namespace = originalTest.DeclaringType.Namespace;
           
            var ecsMain = AddEsMain(entryClass, originalTests, mod);

            return entryClass;
        }

        private static TypeDefinition CreateNewClass(ModuleDefinition module, string name)
        { 

            var newClass = new TypeDefinition("", name, TypeAttributes.Class | TypeAttributes.Public);
            newClass.BaseType = module.ImportReference(typeof(System.Object));
            
            AddEmptyCtor(newClass, module);
            return newClass;
        }

        private static MethodDefinition AddEsMain(TypeDefinition newClass, MethodDefinition[] originalTests, ModuleDefinition moduleToImport)
        {
            var method = new MethodDefinition(
                "Main",
                MethodAttributes.Public | MethodAttributes.Static,
                moduleToImport.ImportReference(typeof(int)));
            newClass.Methods.Add(method);

            ImplementEsMain(method, originalTests, moduleToImport);


            return method;
        }


        private static void ImplementEsMain(MethodDefinition newMethod, MethodDefinition[] originalTests, ModuleDefinition moduleToImport)
        {

            var il = newMethod.Body.GetILProcessor();
            il.Emit(OpCodes.Ldc_I4_0);

            foreach(var originalTest in originalTests)
            {
                // obtain reference to ctor
                var testClass = moduleToImport.ImportReference(originalTest.DeclaringType);
                var testCtor = moduleToImport.ImportReference(originalTest.DeclaringType.Methods.First(x => x.Name == ".ctor"));

                il.Emit(OpCodes.Nop);

                // create Test fixture
                il.Emit(OpCodes.Newobj, testCtor);

                // create action
                il.Emit(OpCodes.Ldftn, moduleToImport.ImportReference(originalTest));
                il.Emit(OpCodes.Newobj, GetCtor(typeof(ECSharp.Action), moduleToImport));

                var testname = originalTest.Name;
                il.Emit(OpCodes.Ldstr, testname);
                
                // run test
                il.Emit(OpCodes.Call, moduleToImport.ImportReference(SymbolExtensions.GetMethodInfo(() => TargetTestRunner.Run(null, ""))));

                // add results
                il.Emit(OpCodes.Add);
            }

            il.Emit(OpCodes.Ret);

            IlHelper.UpdateIlOffsets(il.Body);
        }

        private static string GetTestEntryClass(MethodReference originalTest)
        {
            return originalTest.DeclaringType.Name + "_" + originalTest.Name + "_Main";
        }

        private static MethodReference GetCtor(Type t, ModuleDefinition moduleToImport)
        {
            var typeRef = moduleToImport.ImportReference(t);
            var ctor = moduleToImport.ImportReference(typeRef.Resolve().Methods.First(x => x.Name == ".ctor"));

            return ctor;
        }


        /// <summary>
        /// Add an empty ctor to the class
        /// </summary>
        /// <param name="newClass"></param>
        private static void AddEmptyCtor(TypeDefinition newClass, ModuleDefinition module)
        {
            var ctor = new MethodDefinition(
                ".ctor",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                module.ImportReference(typeof(void)));
            ctor.Body.GetILProcessor().Emit(OpCodes.Ldarg_0);
            ctor.Body.GetILProcessor().Emit(OpCodes.Call, GetCtor(typeof(object), module));
            ctor.Body.GetILProcessor().Emit(OpCodes.Ret);
            newClass.Methods.Add(ctor);
        }
    }
}




