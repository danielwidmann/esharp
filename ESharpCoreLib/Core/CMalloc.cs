using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECSharp;
using ECSharp.Core;
using ESharp.Annotations;

namespace ESharp
{
	[CustomHeaderFile("")]
	[CustomSourceFile("EMalloc.c")]
	public static class CMalloc
	{

		[ExternC]
		static public EObject Malloc(int t)
		{
			return null;
		}

		[ExternC]
		static public Array_1 ArrayMalloc_1(int count)
		{
			return null;
		}

		[ExternC]
		static public EObject ArrayMalloc_4(object t, int count)
		{
			return null;
		}

		[ExternC]
		static public Array_ref ArrayMalloc_ref(object t, int count)
		{
			return null;
		}

		[ExternC]
		static public void Free(EObject obj)
		{ }

	}
	class DummyDestructor
	{
		~DummyDestructor() { }
	}
}
