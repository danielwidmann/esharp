using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using ESharp.Annotations;

namespace ESharp.Helpers
{
	class ExceptionHelper
	{
		Dictionary<MethodReference, bool> m_cache = new Dictionary<MethodReference, bool>();

		public bool CanThrow(MethodReference reference){
			if(m_cache.TryGetValue(reference, out var value)) {
				return value;
			}

			// avoid endless loop
			m_cache[reference] = false;
			var newValue = CanThrow_(reference);
			m_cache[reference] = newValue;

			return newValue;
		}

		bool CanThrow_(MethodReference reference)
		{
			// todo: don't return true when Exception is caught.
			var m = reference.Resolve();

			if (m.CustomAttributes.Any(x => x.AttributeType.Name == typeof(Throws).Name))
				return true;

			if (m.CustomAttributes.Any(x => x.AttributeType.Name == typeof(DoesNotThrow).Name))
				return false;

			if (m == null || m.Body == null)
				return false;

			foreach (var i in m.Body.Instructions) {
				if (i.OpCode == OpCodes.Throw) {
					return true;
				}
				if (i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Callvirt) {
					// avoid endless loop
					if (reference != i.Operand && CanThrow(i.Operand as MethodReference))
						return true;
				}
			}
			return false;
		}
	}
}
