using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESharp.UsedTypeAnalysis;
using Mono.Cecil;

namespace ESharp.Helpers
{
	
	public class ReferenceImportHelper
	{
		public static void ImportReferences(ModuleDefinition m)
		{
			foreach(var t in m.Types) 			
			{
				TypeVisitor.VisitAllMemberRefs(t, x => {
					if (x is MethodReference)
						return m.ImportReference((MethodReference)x);
					if (x is FieldReference)
						return m.ImportReference((FieldReference)x);
					return x;

				});

				TypeVisitor.ReplaceTypeRefs(t, x => m.ImportReference(x));


				foreach (var c in t.CustomAttributes) {
					c.Constructor = m.ImportReference(c.Constructor);
				}
				foreach (var me in t.Methods) {
					foreach(var c in me.CustomAttributes) {
						c.Constructor = m.ImportReference(c.Constructor);
					}
				}
			}
		}

	}
}
