using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using ESharp.Annotations;

namespace ESharp.Helpers
{
	class EmitSource
	{
		public static byte[] CompressUint32(int num)
		{
			// see ReadCompressedUInt32
			if (num < 0x40) {
				return new byte[] { (byte) num};
			}
			if (num < 0x4000) {
				return new byte[] { (byte)(0x80 | (num >> 8)), (byte)num };
			}


			throw new NotImplementedException();
		}

		public static byte[] CreateBlob(string code)
		{
			return new byte[] { 1, 0 }.Concat(CompressUint32(code.Length)).Concat(code.Select(x => (byte)x)).Concat(new byte[] { 0, 0 }).ToArray();
		}

		public static void SourceImplementation(MethodDefinition method, string code)
		{
			var mod = ModuleDefinition.CreateModule("bla", ModuleKind.Dll);
			var blob = CreateBlob(code);
			var ctor = new CustomAttribute(mod.ImportReference((typeof(CustomSource))).Resolve().Methods.First(x => x.IsConstructor), blob.ToArray());			
			method.CustomAttributes.Add(ctor);
		}

		public static void AddTypeSource(TypeDefinition t, string code, bool header=false)
		{
			var nas = new CustomAttribute(t.Module.ImportReference(header? typeof(CustomHeader): typeof(CustomSource)).Resolve().Methods[0], CreateBlob(code));
			nas.ConstructorArguments.Add(new CustomAttributeArgument(t.Module.ImportReference(typeof(string)), code));
			t.CustomAttributes.Add(nas);
		}

		public static void AddTypeHeader(TypeDefinition t, string code)
		{
			AddTypeSource(t, code, true);
		}
	}
}
