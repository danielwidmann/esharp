using ESharp.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecs.Test.ETests
{

	struct TestA
	{
		public int a;
		public int b;

		public void SetB(int b)
		{
			this.b = b;
		}
	}

	class TestB
	{
		public TestA testA;

	}

	

	[TestClass]
	[ETestFixture]
	public class ValueTypeTest
	{
		void callByRef(ref TestA a)
		{
			a.a = 5;
			a.SetB(2);
		}

		void callByValue(TestA a)
		{
			a.a = 6;

			Console.WriteLine(a.a);
		}

		//unsafe void tt(TestA* test){
		//    int a;
		//    var b = &a;

		//    Console.WriteLine(*b);
		//}
		TestA callWithReturnValue()
		{
			return new TestA { a = 7 };
		}
		[TestMethod]
		public void TestCallByRef()
		{
			TestA t = new TestA();
			callByRef(ref t);
			Assert.IsTrue(5 == t.a);

			t.SetB(6);
			Assert.IsTrue(6 == t.b);

			TestA u = callWithReturnValue();
			Assert.IsTrue(7 == u.a);

			callByValue(t);
		}

		[TestMethod]
		public void TestField()
		{
			var b = new TestB();
			Console.WriteLine(b.testA.a);

			b.testA = new TestA { b = 5 };
		}

		[TestMethod]
		public void TestFieldRef()
		{
			var b = new TestB();
			Console.WriteLine(b.testA.a);

			b.testA = new TestA { b = 5 };
		}
    }

	[TestClass]
	[ETestFixture]
	public class ValueTypeRefReturn
	{

		[TestMethod]
		public void TestRefReturn()
		{
			s_struct.num = 42;
			ref var r = ref ReturnRef();
			r.num = 43;

			Assert.IsTrue(s_struct.num == 43);
		}

		static MyStruct s_struct;
		

		[TestMethod]
		ref MyStruct ReturnRef()
		{
			return ref s_struct;
		}
	}

	[TestClass]
	[ETestFixture]
	public class ValueTypeTestSimple { 

		[TestMethod]
		public void TestSimple()
		{
			MyStruct a = new MyStruct();

			Console.WriteLine(a.num);

			a.num = 43;

			Console.WriteLine(a.num);
		}
	}
	struct MyStruct
	{
		public int num;

	};

	[TestClass]
	[ETestFixture]
	public class ValueTypeTestBoxing
	{

		[TestMethod]
		public void TestBoxSimple()
		{
			var a = new BoxStruct();
			a.s = "a";
			a.n = 42;

			object o = (object)a;

			//Assert.AreEqual("a", o.ToString());

			Assert.IsTrue(42 == ((BoxStruct)o).n);

			var unboxed = (BoxStruct)o;

			//Assert.AreEqual("a", unboxed.s);
			Assert.IsTrue(42 == unboxed.n);
		}

		[TestMethod]
		public void TestBoxInt()
		{
			var a = 42;

			object o = (object)a;

			var unboxed = (int)o;
			
			Assert.IsTrue(42 == unboxed);
		}

		[TestMethod]
		public void TestCopy()
		{
			var a = new BoxStruct();

			a.n = 42;

			BoxStruct b = a;
			Assert.IsTrue(42 == b.n);
		}
	}

	struct BoxStruct
	{
		public string s;
		public int n;

		public override string ToString()
		{
			return s;
		}
	};

	[TestClass]
	[ETestFixture]
	public class ValueTypeTestWithReference
	{

		[TestMethod]
		public void TestUnRef()
		{
			var a = new StructWithRef();
			a.obj = new TestX();			
		}

		[TestMethod]
		public void TestFieldInit()
		{
			var a = new WrappedStructWithRef();
			a.value.obj = new TestX { num=42 };

			Assert.IsTrue(a.value.obj.num == 42);

			Assert.IsTrue(a.get_num() == 42);

			a.value = new StructWithRef();
		}

		[TestMethod]
		public void TestFieldAccess()
		{
			var a = new WrappedStructWithRef();
			a.value.obj = new TestX { num = 42 };
			
		}

		[TestMethod]
		public void TestAssign()
		{
			var a = new StructWithRef();
			var b = new StructWithRef();
			a.obj = new TestX();
			b.obj = new TestX();

			a = b;
		}


	}

	class TestX
	{
		public int num;
	}

	struct StructWithRef
	{
		public TestX obj;
	};

	class WrappedStructWithRef
	{
		public StructWithRef value;

		public int get_num()
		{
			return value.obj.num;
		}
	};

}
