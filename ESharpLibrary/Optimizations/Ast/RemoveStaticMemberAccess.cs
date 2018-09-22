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
	class RemoveStaticMemberAccess : IAstTransform
	{
		public void Run(AstNode rootNode, TransformContext context)
		{

			// static fields
			foreach (var member in rootNode.Descendants.OfType<MemberReferenceExpression>()) {
				var cecilField = member.Annotation<MemberResolveResult>();
				if (cecilField == null) continue;
				if (!cecilField.Member.IsStatic) continue;
				member.ReplaceWith(new IdentifierExpression(cecilField.Member.Name));
			}

			// method calls (all method call are static at this point)
			foreach(var invocationExpression in rootNode.Descendants.OfType<InvocationExpression>()) {
				if (invocationExpression.Target is MemberReferenceExpression e) {
					invocationExpression.Target.ReplaceWith(new IdentifierExpression(e.MemberName));

				}
			}
		}
	}
}
