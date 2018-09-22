using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESharp.Helpers;
using ESharp.Optimizations.TypeDiscoveryOptimization;
using ICSharpCode.Decompiler.ECS;
using Mono.Cecil;

namespace ESharp.UsedTypeAnalysis
{
	class NewUsedTypeAnalysis
	{
		HashSet<TypeDefinition> m_all = new HashSet<TypeDefinition>();
		HashSet<TypeDefinition> m_toProcess = new HashSet<TypeDefinition>();	
		ReferenceResolver m_resolver;

		TypeReference AddAndResolveType(TypeReference t)
		{
			if (t == null) return null;

			TypeReference unwrapped = t;
			if(t.IsPointer || t.IsByReference) {
				unwrapped = t.GetElementType();
			}

			var def = AddAndResolveTypeDef(unwrapped);

			if (t.IsPointer && def != null && !def.IsPointer) {
				return new PointerType(def);
			} else if (t.IsByReference && !def.IsByReference) {
				return new ByReferenceType(def);
			} else {
				return def;
			}
		}

		TypeReference AddAndResolveTypeDef(TypeReference t)
		{			
			var resolved = Replace(t);

			// do not add system references, but leave them in the code
			if (resolved == null) return t;

			if (resolved.CustomAttributes.Any(att => att.AttributeType.Name == "Skip"))
				return null;

			// do not include the private implementation
			if (resolved.Name.StartsWith("<PrivateImplementationDetails>"))
				return null;

			if (m_all.Contains(resolved))
				return resolved;

			var existing = m_all.SingleOrDefault(x => x.FullName == resolved.FullName);
			if (existing != null) {
				// this might be a problem with re-resolving types
				// Make sure we always return the existing type

				return existing;
			}

			// Use copied type
			var copy = CopyTypeHelper.CopyType(resolved);

			// nested classes need to keep a valid name
			if(resolved.DeclaringType != null) {
				copy.Name = resolved.DeclaringType.Name + "/" + copy.Name;
				copy.Namespace = resolved.DeclaringType.Namespace;
			}
			//copy.DeclaringType = resolved.DeclaringType;

			// Names need to be the same, otherwise this logic doesn't work
			Debug.Assert(resolved.FullName == copy.FullName);

			m_all.Add(copy);

			// It's a newly discovered type so we shouldn't try to process it already
			Debug.Assert(!m_toProcess.Any(x => x.FullName == copy.FullName));
			//if (!m_toProcess.Any(x=>x.FullName == copy.FullName))
			m_toProcess.Add(copy);

			return copy;
		}

		/// <summary>
		/// A new type was descovered.
		/// </summary>
		/// <param name="t"></param>
		void AddType(TypeReference t)
		{
			AddAndResolveType(t);			
		}

		void AddType(MethodReference t)
		{
			if (t == null) return;
			var resolved = t.Resolve();
			AddType(t.Resolve().DeclaringType);
		}

		void AddType(FieldReference t)
		{
			if (t == null) return;
			AddType(t.Resolve().DeclaringType);
		}

		void AddType(PropertyReference t)
		{
			if (t == null) return;
			AddType(t.Resolve().DeclaringType);
		}

		// todo pass in as dictionary
		TypeDefinition Replace(TypeReference t)
		{
			if (t.Name.StartsWith("!")) {
				return TypeMapping.GetInstance().LookupReplacementType("System.object");
			}
			var def = TypeMapping.GetInstance().Replace(t);

			if (def.FullName.StartsWith("System.") && def.Module.Name == "CommonLanguageRuntimeLibrary")
				return null;

			return def;
		}

		MemberReference ReplaceMemberRef(MemberReference m)
		{
			if (m is MethodReference) {
				// Every instruction is only process once, so nothing should be resolved
				//Debug.Assert(!(m is MethodDefinition));

				var reference = (m as MethodReference);

				var declatingType = AddAndResolveType(reference.DeclaringType);
				var parameters = reference.Parameters.Select(x => AddAndResolveType(x.ParameterType).Name);
				if (declatingType != null)
					try {
						return m_resolver.GetMethodReference(declatingType, reference.Name, parameters);
					} catch { }
			}
			if (m is FieldReference ) {
				var reference = m as FieldReference;
				var resolved = AddAndResolveType(reference.DeclaringType);
				if (resolved != null)
					return  m_resolver.GetField(resolved.FullName, reference.Name);
			}
			return m;
		}

		void Process(TypeDefinition t)
		{	
			DiscoveryOptimization.Optimize(t);


			TypeVisitor.VisitType(t, x => { return AddAndResolveType(x); }, ReplaceMemberRef);

			foreach(var a in t.CustomAttributes.Where(x=>x.AttributeType.Name == "Uses")) {
				var usedType = (TypeReference)a.ConstructorArguments[0].Value;
				AddType(usedType);
			}
		}


		public IEnumerable<TypeDefinition> GetUsedTypes(MethodReference EntryPoint)
		{

			m_resolver = new ReferenceResolver(m_all);

			// start with entry point type
			AddType(EntryPoint.Resolve().DeclaringType);

			while (m_toProcess.Count > 0) {
				var first = m_toProcess.First();

				Process(first);
				m_toProcess.Remove(first);
			}

			return m_all;			
		}


	}
}
