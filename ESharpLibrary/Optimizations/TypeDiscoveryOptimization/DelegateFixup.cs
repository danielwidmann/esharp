using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESharp.Annotations;
using ICSharpCode.Decompiler;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ESharp.Optimizations.TypeDiscoveryOptimization
{
	class DelegateFixup
	{
		public static void Optimize(TypeDefinition t)
		{

			if (!IsDelegate(t))
				return;



			t.Methods.Remove(t.Methods.Single(x => x.Name == "BeginInvoke"));
			t.Methods.Remove(t.Methods.Single(x => x.Name == "EndInvoke"));

			// todo pass resolver as context
			var mod = ModuleDefinition.CreateModule("a", ModuleKind.Dll);
			t.Fields.Add(new FieldDefinition("instance", 0, mod.ImportReference(typeof(object))));
			t.Fields.Add(new FieldDefinition("function", 0, mod.ImportReference(typeof(IntPtr))));



			var ctor = t.Methods.Single(x => x.Name == ".ctor");
			ctor.Body = new MethodBody(ctor);

			var proc = ctor.Body.GetILProcessor();
			proc.Emit(OpCodes.Ldarg_0);
			proc.Emit(OpCodes.Ldarg_1);
			proc.Emit(OpCodes.Stfld, t.Fields.Single(x => x.Name == "instance"));
			proc.Emit(OpCodes.Ldarg_0);
			proc.Emit(OpCodes.Ldarg_2);
			proc.Emit(OpCodes.Stfld, t.Fields.Single(x => x.Name == "function"));
			proc.Emit(OpCodes.Ret);



			var invoke = t.Methods.Single(x => x.Name == "Invoke");

			var code = String.Format("if(_this->instance == NULL) {{ {0} }} \r\n else {{ {1} }}",
						   GenerateMethodCall(t, invoke, false),
						   GenerateMethodCall(t, invoke, true));
			Helpers.EmitSource.SourceImplementation(invoke, code);

			// make sure we handle potential exceptions form the delegate
			invoke.CustomAttributes.Add(new CustomAttribute(mod.ImportReference(typeof(Throws).GetConstructor(new Type[] { })), new byte[] { 1, 0, 0, 0 }));
			
		}

		private static string GenerateMethodCall(TypeDefinition t, MethodDefinition method, bool useInstance)
		{
			var returnStatement = method.ReturnType.Name == "void" ? "" : "return";

			var pfnName = "pfn" + t.Name + (useInstance ? "_inst" : "");
			var args = method.Parameters.Select(x => x.ParameterType.Name);
			if (useInstance) { args = new[] { "void*" }.Concat(args); }
			var cTypedef = String.Format("typedef {0} (*{1})({2});",
				method.ReturnType.Name,
				pfnName,
				String.Join(",", args)
				);

			var argNames = method.Parameters.Select(x => x.Name);
			if (useInstance) { argNames = new[] { "_this->instance" }.Concat(argNames); }
			var call = String.Format("{0} (({1}) _this->function)({2});",
				returnStatement,
				pfnName,
				String.Join(",", argNames));

			var code = cTypedef + "\r\n" + call;
			return code;
		}

		public static bool IsDelegate(TypeDefinition type)
		{
			if (type.BaseType != null && type.BaseType.Namespace == "System") {
				if (type.BaseType.Name == "MulticastDelegate")
					return true;
				if (type.BaseType.Name == "Delegate" && type.Name != "MulticastDelegate")
					return true;
			}
			return false;
		}
	}
}
