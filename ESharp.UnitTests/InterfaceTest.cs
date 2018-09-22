using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESharp.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ESharp.Tests.ETest
{

	public interface ITestInterface
	{
		int say();
	}

	public class Hello : ITestInterface
	{
		public int say()
		{
			return 42;
		}
	}

	[TestClass]
	[ETestFixture]
	class InterfaceTest
	{
		[TestMethod]
		public void TestInterfaceSimple()
		{
			ITestInterface i = new Hello();
			int res = i.say();

			Assert.IsTrue(res == 42);
		}
	}
}
