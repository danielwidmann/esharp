using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ESharp.Helpers;
using ICSharpCode.Decompiler.ECS;
using Mono.Cecil;

namespace ESharp.Optimizations.IL
{
	class StringLiteralDictionary
	{
		const string ARRAY_CLASS_NAME = "G";
		Dictionary<string, FieldReference> m_dict;
		HashSet<string> m_usedNames;
		TypeDefinition m_typedef;
		ReferenceResolver m_resolver;

		public StringLiteralDictionary(ReferenceResolver resolver)
		{
			m_dict = new Dictionary<string, FieldReference>();
			m_resolver = resolver;
			m_usedNames = new HashSet<string>();

			m_typedef = new TypeDefinition("", ARRAY_CLASS_NAME,
				TypeAttributes.Public |
				TypeAttributes.Sealed | TypeAttributes.Abstract /*static*/);


			// make sure other parts of the system don't get confused
			m_resolver.GetTypeReference(typeof(ESharp.EObject)).Module.Types.Add(m_typedef);
		}

		string GetStringFieldName(string s)
		{
			Regex regexObj = new Regex(@"[^\w]");
			var resultString = "S_" + regexObj.Replace(s, "_");
			return resultString;
		}

		public FieldReference AddString(string s)
		{
			if (m_dict.ContainsKey(s)) {
				return m_dict[s];
			}


			var fieldName = GetStringFieldName(s);

			var initializer = s.Select(x => "'" + ToLiteral(x) + "'");

			var field = AddGeneric(fieldName, m_resolver.GetStringType(), "Array_1", initializer);
			m_dict[s] = field;
			return field;			
		}

		struct ArrayField
		{
			public IEnumerable<object> content;
			public string type;
		}

		Dictionary<string, ArrayField> m_staticArrays = new Dictionary<string, ArrayField>();
		int m_counter;

		public FieldReference AddArray_1(byte[] data)
		{
			return AddGeneric("array" + m_counter++, m_resolver.GetTypeReference("Array_1"), "Array_1", data.Cast<object>());

		}

		private FieldReference AddGeneric(string fieldName, TypeReference type, string typeString, IEnumerable<object> content)
		{
			// make name unique
			while (m_usedNames.Contains(fieldName)) {
				fieldName += "_";
			}
			m_usedNames.Add(fieldName);

			// use updated name for code generation          
			var fildNameWithClass = fieldName; //"S_" + ARRAY_CLASS_NAME + "_" +

			m_staticArrays.Add(fildNameWithClass, new ArrayField { content = content, type = typeString });

			var def = new FieldDefinition(
				fieldName,
				FieldAttributes.Static | FieldAttributes.Public, // | FieldAttributes.Literal
				type
				);

			// make sure the reference can be used later on
			m_typedef.Fields.Add(def);

			return def;
		}

		public string GetString(FieldReference f)
		{
			foreach (var e in m_dict) {
				if (e.Value == f)
					return e.Key;
			}

			return null;
		}

		public TypeDefinition GetStringsType()
		{
			EmitSource.AddTypeSource(m_typedef, GetStringsSource());
			EmitSource.AddTypeHeader(m_typedef, GetStringsHeader());
			return m_typedef;
		}

		private static string ToLiteral(char input)
		{
			if (input == '\n')
				return "\\n";
			if (input == '\r')
				return "\\r";
			if (input == '\t')
				return "\\t";
			if (input == '\'')
				return "\\'";

			return input.ToString();
		}

		public string GetStringsSource()
		{
			var sb = new StringBuilder();
			sb.AppendLine("// generated strings");

			foreach (var item in m_staticArrays) {
				var arrayContent = item.Value.content;
				var itemName = item.Key;
				var initializerElements = arrayContent.Select(x => x.ToString()).ToArray();
				var initializerString = string.Join(", ", initializerElements);
				//ALIGN

				initializerString += ", PAD_" + initializerElements.Length % 4;
				initializerString = "{" + initializerString + "}";

				sb.AppendFormat("const EString_struct {0}_struct = {{ 1, EString_TypeId, {1}, {2} }};\r\n", itemName, initializerElements.Length, initializerString);
			}
			return sb.ToString();
		}

		public string GetStringsHeader()
		{
			var sb = new StringBuilder();
			sb.AppendLine("#pragma once// pre-header");
			sb.AppendLine("// generated strings");

			sb.AppendLine("// header");

			foreach (var item in m_staticArrays) {
				sb.AppendFormat("extern const EString_struct {0}_struct;\r\n", item.Key);
				sb.AppendFormat("#define {0} ((Array_1)&{0}_struct)\r\n", item.Key);
			}

			return sb.ToString();
		}
	}
}
