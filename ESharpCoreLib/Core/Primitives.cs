using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESharpCore.Core
{
	public struct BoxedInt32
	{
		int value;
	}

	public class Boxed<Type>
	{
		Type value; 

		public static object box(Type instance)
		{
			var t = new Boxed<Type>();
			t.value = instance;

			return t;
		}

		public ref Type unbox()
		{
			return ref this.value;
		}

		public Type unbox_any()
		{
			return this.value;
		}


	}
}
