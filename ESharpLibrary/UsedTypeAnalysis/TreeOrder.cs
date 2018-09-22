using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace ESharp.UsedTypeAnalysis
{
	public class TreeOrder
	{
		static public List<TypeDefinition> Order(TypeDefinition root, IEnumerable<TypeDefinition> types)
		{
			var res = new List<TypeDefinition>();
			res.Add(root);

			var subTypes = types.Where(x => x.BaseType == root || x.BaseType != null && x.BaseType.Name == root.Name);				

			foreach (var subType in subTypes) {
				res.AddRange(Order(subType, types));
			}

			return res;
		}

		/// <summary>
		/// Shuffle struct to make sure they are defined before they are used.
		/// </summary>
		/// <param name="types"></param>
		/// <returns></returns>
		static public List<TypeDefinition> StructOrder(IEnumerable<TypeDefinition> types)
		{
			var res = new List<TypeDefinition>();
			var delayedTypes = new List<TypeDefinition>();


			foreach (var t in types) {
				// check if we can place a delayed type now      
				var processed = new List<TypeDefinition>();
				foreach (var d in delayedTypes) {
					if (UsedValueTypes(d).All(x => res.Contains(x))) {
						res.Add(d);
						processed.Add(d);
					}
				}
				foreach (var p in processed) {
					delayedTypes.Remove(p);
				}




				var usedTypes = UsedValueTypes(t).ToList();

				if (usedTypes.Count == 0 || usedTypes.All(x => res.Contains(x))) { // all the dependencies are satisfied.
					res.Add(t);
				} else {
					delayedTypes.Add(t);
				}
			}

			// don't loose any types
			// probably this is not quite right as the delayed types might depend on each other.
			// idea: put all structs in delayed. Every iteration take out types with satisfied dependencies.
			res.AddRange(delayedTypes);

			return res;
		}

		static IEnumerable<TypeDefinition> UsedValueTypes(TypeDefinition type)
		{
			foreach (var f in type.Fields) {
				var fieldType = f.FieldType.Resolve();
				if (fieldType.BaseType != null && fieldType.BaseType.Name == "ValueType" && !fieldType.IsPrimitive) {
					yield return fieldType;
				}
			}
		}
	}
}
