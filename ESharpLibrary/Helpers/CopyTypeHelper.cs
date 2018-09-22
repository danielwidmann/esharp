using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using ECSharp;
using EcsTarget;
using Mono.Cecil;
using Mono.Cecil.Cil;
using ESharp.Annotations;

namespace ESharp.Helpers
{
	public class CopyTypeHelper
	{
		public static TypeDefinition CopyType(TypeDefinition t)
		{
			var nt = new TypeDefinition(t.Namespace, t.Name, t.Attributes, t.BaseType);			

			foreach (var a in t.CustomAttributes) {
				// move this out of here??
				if (a.AttributeType.Name == "CustomSourceFile") {
					var filename = (string)a.ConstructorArguments[0].Value;
					var sourceContent = filename == ""? "": EmbeddedFileLoader.GetResourceFile(filename, t.Module);
					var nas = new CustomAttribute(t.Module.ImportReference(typeof(CustomSource)).Resolve().Methods[0], a.GetBlob());
					nas.ConstructorArguments.Add(new CustomAttributeArgument(t.Module.ImportReference(typeof(string)), sourceContent));
					nt.CustomAttributes.Add(nas);
				} else if (a.AttributeType.Name == "CustomHeaderFile") {
					var filename = (string)a.ConstructorArguments[0].Value;
					var sourceContent = filename == "" ? "" : EmbeddedFileLoader.GetResourceFile(filename, t.Module);
					var nas = new CustomAttribute(t.Module.ImportReference(typeof(CustomHeader)).Resolve().Methods[0], a.GetBlob());
					nas.ConstructorArguments.Add(new CustomAttributeArgument(t.Module.ImportReference(typeof(string)), sourceContent));
					nt.CustomAttributes.Add(nas);
				} 
				
				else {
					var na = new CustomAttribute(a.Constructor, a.GetBlob());
					foreach (var ca in a.ConstructorArguments) {
						var nca = new CustomAttributeArgument(ca.Type, ca.Value);
						na.ConstructorArguments.Add(nca);
					}

					nt.CustomAttributes.Add(na);
				}
						
				
			}			

			foreach(var i in t.Interfaces) {
				nt.Interfaces.Add(new InterfaceImplementation(i.InterfaceType));
			}

			foreach (var f in t.Fields) {
				var nf = new FieldDefinition(f.Name, f.Attributes, f.FieldType);
					
				nt.Fields.Add(nf);
			}

			foreach (var m in t.Methods) {
				//var nm = m;
				//nm.DeclaringType = nt;
				var nm = new MethodDefinition(m.Name, m.Attributes, m.ReturnType);
				foreach(var p in m.Parameters) 
				{
					nm.Parameters.Add(new ParameterDefinition(p.Name, p.Attributes, p.ParameterType));
				}

				foreach (var a in m.CustomAttributes) {
					var na = new CustomAttribute(a.Constructor, a.GetBlob());
					nm.CustomAttributes.Add(na);
				}

				if (m.HasBody) {
					CopyMethodBody(m, nm);
				}

				nt.Methods.Add(nm);
			}

			if(t.IsPointer) 
				{

			}

			return nt;
		}

		public static void CopyMethodBody(MethodDefinition m, MethodDefinition nm)
		{
			nm.Body = new MethodBody(nm);
			foreach (var v in m.Body.Variables) {
				var nv = new VariableDefinition(v.VariableType);
				nm.Body.Variables.Add(nv);
			}

			foreach (var e in m.Body.ExceptionHandlers) {
				var eh = new ExceptionHandler(e.HandlerType);
				eh.CatchType = e.CatchType;
				eh.TryStart = e.TryStart;
				eh.TryEnd = e.TryEnd;
				eh.HandlerStart = e.HandlerStart;
				eh.HandlerEnd = e.HandlerEnd;
				eh.FilterStart = e.FilterStart;
				nm.Body.ExceptionHandlers.Add(eh);
			}

			foreach (var i in m.Body.Instructions) {
				nm.Body.Instructions.Add(i);

                // make sure none of the instructions point to a stale parameter
                if(i.Operand is ParameterDefinition pd)
                {
                    i.Operand = nm.Parameters[pd.Index];
                }

                if(i.Operand is VariableDefinition vd)
                {
                    i.Operand = nm.Body.Variables[vd.Index];
                }
			}
		}
	}
}
