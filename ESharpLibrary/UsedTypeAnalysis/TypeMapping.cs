using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ECSharp;
using ECSharp.Net;
//using ECSharp.Net;
//using ECSharp.Task;
using Mono.Cecil;


namespace ESharp.UsedTypeAnalysis
{
	public class TypeMapping
	{
		static TypeMapping s_instance;
		public static TypeMapping GetInstance()
		{
			if (s_instance == null) {
				s_instance = TypeMapping.Default();
			}
			return s_instance;
		}

		private TypeMapping()
		{

		}

		// initial type mapping
		// move out of this class
		public static TypeMapping Default()
		{
			var mapping = new TypeMapping();
			//mapping.AddTypeMapping("System.MulticastDelegate", typeof(esharp::System.MulticastDelegate));
			mapping.AddTypeMapping("System.MulticastDelegate", typeof(EDelegate));
			mapping.AddTypeMapping(typeof(object), typeof(ESharp.EObject));
			mapping.AddTypeMapping("Microsoft.VisualStudio.TestTools.UnitTesting.Assert", typeof(ESharp.EAssert));
			mapping.AddTypeMapping("NUnit.Framework.Assert", typeof(ESharp.EAssert));
			mapping.AddTypeMapping(typeof(Exception), typeof(ESharp.EException));
			//mapping.AddTypeMapping(typeof(Task), typeof(ECSharp.Task.ETask_obj));
			mapping.AddTypeMapping("System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1", typeof(ESharp.Task.EAsyncTaskMethodBuilder_obj));
			mapping.AddTypeMapping("System.Runtime.CompilerServices.TaskAwaiter`1", typeof(ESharp.Task.TaskAwaiter_obj));
			mapping.AddTypeMapping("System.Threading.Tasks.Task`1", typeof(ESharp.Task.ETask_obj));
			mapping.AddTypeMapping("System.Runtime.CompilerServices.AsyncTaskMethodBuilder", typeof(ESharp.Task.EAsyncTaskMethodBuilder_obj));
			mapping.AddTypeMapping("System.Runtime.CompilerServices.TaskAwaiter", typeof(ESharp.Task.TaskAwaiter));
			mapping.AddTypeMapping("System.Threading.Tasks.Task", typeof(ESharp.Task.ETask));
			mapping.AddTypeMapping("System.Action`1", typeof(ECSharp.Action_String));
			mapping.AddTypeMapping("System.Action", typeof(ECSharp.Action));
			//mapping.AddTypeMapping("System.ValueType", typeof(ECSharp.Task.ValueType));
			mapping.AddTypeMapping("System.Console", typeof(ESharp.Console));
			mapping.AddTypeMapping("System.Runtime.CompilerServices.IAsyncStateMachine", typeof(ESharp.Task.IAsyncStateMachine));
			mapping.AddTypeMapping("System.Runtime.CompilerServices.INotifyCompletion", typeof(ESharp.Task.INotifyCompletion));

			mapping.AddTypeMapping(typeof(System.Net.Sockets.UdpClient), typeof(ECSharp.Net.EUdpClient));
			mapping.AddTypeMapping("System.Net.Sockets.UdpReceiveResult", typeof(EUdpReceiveResult));
			mapping.AddTypeMapping("System.Runtime.CompilerServices.RuntimeHelpers", typeof(ECSharp.Core.RuntimeHelpers));
			mapping.AddTypeMapping(typeof(string), typeof(ECSharp.Core.EString));
			

			// todo new replace generic types
			//fasdf rename return type and args before trying to resolve method.
			//think of generic tasks

			return mapping;
		}

		ModuleDefinition m_module = ModuleDefinition.CreateModule("RenameObject", ModuleKind.Dll);
		Dictionary<string, TypeDefinition> m_typeMapping = new Dictionary<string, TypeDefinition>();

		public void AddTypeMapping(string FQN, TypeDefinition def)
		{
			m_typeMapping.Add(FQN, def);
		}

		public void AddTypeMapping(string FQN, Type replacement)
		{
			m_typeMapping.Add(FQN, m_module.Import(replacement).Resolve());
		}

		public void AddTypeMapping(Type orig, Type replacement)
		{
			m_typeMapping.Add(orig.FullName, m_module.Import(replacement).Resolve());
		}

		public TypeDefinition LookupReplacementType(String FQN)
		{
			// ignore generic paramer for now
			var filteredName = Regex.Replace(FQN, "<.*>", "");
			TypeDefinition typedef;
			if (m_typeMapping.TryGetValue(filteredName, out typedef)) {
				return typedef;
			} else {
				return null;
			}
		}

		public TypeDefinition LookupReplacementType(TypeReference t)
		{
			var replacement = LookupReplacementType(t.FullName);
			if(replacement != null)
				Debug.Assert(t.IsValueType == replacement.IsValueType, "Replacement types must be of the same kind (Reference Type or Value Type)");
			return replacement;
		}

		public TypeDefinition Replace(TypeReference reference)
		{
			var replacement = LookupReplacementType(reference);
			if (replacement != null) {
				return replacement;
			} else {				
				return reference.Resolve();
			}
		}

	}
}
