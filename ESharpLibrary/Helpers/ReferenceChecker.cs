using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESharp.UsedTypeAnalysis;
using Mono.Cecil;

namespace ESharp.Helpers
{
	public class ReferenceChecker
	{
		public static void CheckReferences(ModuleDefinition m)
		{
			foreach (var t in m.Types) {
				TypeVisitor.VisitAllMemberRefs(t, x => {
					var resolved = x.Resolve();
					var mod = resolved.DeclaringType.Module;
					if (mod.Name != "CommonLanguageRuntimeLibrary" && resolved.DeclaringType.Name != "C" && mod != m) {
						Debug.Assert(false);
					}
					return x;
				});

				TypeVisitor.VisitAllTypeRefsMemberTypeRefs(t, x => {
					if (x.IsPointer)
						x = x.GetElementType();

					var resolved = x.Resolve();
					if (resolved.Module.Name != "CommonLanguageRuntimeLibrary" && resolved.Module != m) {
						Debug.Assert(false);
					}					 
				});


				
			}
		}
	}
}
