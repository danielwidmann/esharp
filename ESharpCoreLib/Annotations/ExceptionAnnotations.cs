using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESharp.Annotations;

namespace ESharp.Annotations
{
	[System.AttributeUsage(System.AttributeTargets.Method | AttributeTargets.Constructor), Skip]
	public class Throws : System.Attribute
	{
	}

	[System.AttributeUsage(System.AttributeTargets.Method | AttributeTargets.Constructor), Skip]
	public class DoesNotThrow : System.Attribute
	{
	}


}
