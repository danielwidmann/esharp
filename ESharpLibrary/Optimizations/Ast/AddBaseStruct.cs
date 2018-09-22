using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;
using ICSharpCode.Decompiler.Semantics;
using Mono.Cecil;

namespace ESharp.Optimizations.Ast
{
	class AddBaseStruct: IAstTransform
	{
		public void Run(AstNode rootNode, TransformContext context)
		{
			foreach (var typedef in rootNode.Descendants.OfType<TypeDeclaration>()) {
				var typeRef = typedef.Annotations.OfType<TypeResolveResult>().Single().Type;
				var baseType = typeRef.DirectBaseTypes.FirstOrDefault();
				if (baseType == null) {
					continue;
				}

				// add base pointer         
				if (typeRef.Name != "EObject") {
					var baseField = (new FieldDeclaration());
					baseField.ReturnType = new SimpleType(baseType.Name + "_struct");
					baseField.Variables.Add(new VariableInitializer("_base"));

					typedef.Members.ReplaceWith(new EntityDeclaration[] { baseField }.Concat(typedef.Members));
				}

			}
		}
	}
}
