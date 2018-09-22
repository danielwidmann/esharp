using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESharp.Annotations
{
	/// <summary>
	/// Skip marks classes that are not available on the target. E.g. Annotations.
	/// </summary>
	[System.AttributeUsage(System.AttributeTargets.Class | AttributeTargets.Field), Skip]
	public class Skip : System.Attribute
	{
	}

	/// <summary>
	/// This function is implemented in an external C source file
	/// </summary>
	[System.AttributeUsage(System.AttributeTargets.Method | AttributeTargets.Constructor), Skip]
	public class ExternC : System.Attribute
	{

	}

	[System.AttributeUsage(System.AttributeTargets.Class), Skip]
	public class CustomHeaderFile : System.Attribute
	{
		public CustomHeaderFile(string s)
		{
		}
	}

	[System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Method), Skip]
	public class CustomHeader : System.Attribute
	{
		public CustomHeader(string s)
		{
		}
	}


	[System.AttributeUsage(System.AttributeTargets.Class), Skip]
	public class CustomSourceFile : System.Attribute
	{
		public CustomSourceFile(string s)
		{
		}
	}

	[System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Method), Skip]
	public class CustomSource : System.Attribute
	{
		public CustomSource(string s)
		{
		}
	}

	// this class has (implicit) dependencyies on other classes. The Uses attribute makes the dependency explicit .
	[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true), Skip]
	public class Uses : System.Attribute
	{
		public Uses(Type t)
		{
		}
	}

	[System.AttributeUsage(System.AttributeTargets.Method | AttributeTargets.Constructor), Skip]
	public class ManualRefCounting : System.Attribute
	{

	}

	[System.AttributeUsage(System.AttributeTargets.Method | AttributeTargets.Constructor), Skip]
	public class CName : System.Attribute
	{
		public string Name { get; set; }
		public CName(String name)
		{
			Name = name;
		}
	}

}
