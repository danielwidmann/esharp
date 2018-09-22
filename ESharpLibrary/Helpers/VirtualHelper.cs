using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;
using Mono.Cecil;

namespace ESharp.Helpers
{
	public class VirtualMethod
	{
		public MethodDefinition Method;
		public List<MethodDefinition> OverwrittenBy = new List<MethodDefinition>();

		public override string ToString()
		{
			return Method.DeclaringType.Name + "." + Method.Name + ":" + String.Join(",", OverwrittenBy.Select(x => x.DeclaringType.Name));
		}
	}

	public class VirtualHelper
	{
		public static IEnumerable<VirtualMethod> VirtualAnalysis(IEnumerable<TypeDefinition> types)
		{
			var virtuals = new Dictionary<MethodDefinition, VirtualMethod>();

			foreach (var t in types) {
				foreach (var m in t.Methods) {
					//types
					var overrides = TypesHierarchyHelpers.FindBaseMethods(m).Where(b => b.IsVirtual);

					foreach (var basem in overrides) {
						AddMethod(virtuals, m, basem);
					}

					foreach (var i in t.Interfaces) {
						foreach (var interfaceMethod in i.InterfaceType.Resolve().Methods) {
							// don't create virtual for the instance checker metod
							if (interfaceMethod.Name.Contains("IsInstance"))
								continue;

							//if (MatchInterfaceMethod(interfaceMethod, m))
							if (TypesHierarchyHelpers.MatchInterfaceMethod(interfaceMethod, m, i.InterfaceType)) {
								AddMethod(virtuals, m, interfaceMethod);
							}
						}
					}
				}
			}

			return virtuals.Values.ToArray();
		}

		private static bool MatchInterfaceMethod(MethodReference i, MethodReference m)
		{
			if (i.Name != m.Name)
				return false;

			if (i.Parameters.Count != m.Parameters.Count)
				return false;

			for (int idx = 0; idx < i.Parameters.Count; idx++) {
				if (i.Parameters[idx].ParameterType.Name != m.Parameters[idx].ParameterType.Name)
					return false;
			}

			return true;
		}

		private static void AddMethod(Dictionary<MethodDefinition, VirtualMethod> virtuals, MethodDefinition m, MethodDefinition basem)
		{
			if (!virtuals.ContainsKey(basem)) {
				virtuals[basem] = new VirtualMethod { Method = basem };
			}
			virtuals[basem].OverwrittenBy.Add(m);
		}
	}
}
