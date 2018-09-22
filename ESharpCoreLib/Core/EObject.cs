using System;
using ECSharp;
using ECSharp.Core;
using ESharp.Annotations;

namespace ESharp
{

	[Uses(typeof(ESharpRT))]
	[Uses(typeof(EException))] // Some stuff could throw 
	[CustomHeaderFile("EObject.h")]
	public class EObject
	{

		[ExternC]
		public EObject()
		{
		}

		[ExternC]
		~EObject()
		{
		}

		[ExternC]
		public static EObject Allocate(object t)
		{
			return null;
		}

		[ExternC]
		public void AddRef()
		{
		}

		[ExternC]
		public void RemoveRef()
		{ }

		[ExternC]
		static public void s_AddRef(EObject e)
		{
		}

		[ExternC]
		static public void s_RemoveRef(EObject e)
		{ }

		[ExternC]
		[DoesNotThrow]
		new public System.Type GetType()
		{
			return null;
		}


		// For value types only
		[ExternC]
		static public IntPtr address(IntPtr e)
		{
			return default(IntPtr);
		}

	}
}

