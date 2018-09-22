using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ESharp.Annotations;
using ESharp;

namespace UnitTestProject1
{
	interface IExInterface
	{

	}
	class NewException : Exception, IExInterface
    {

    }

    [TestClass]
    [ETestFixture]
    public class IsInstanceTest
    {
        [TestMethod]
        public void SimpleIsInstance()
        {
            object e = new EException();
            Assert.IsTrue(e is EException);
			Assert.IsTrue(!(e is IsInstanceTest));
		}


		[TestMethod]
		public void InterfaceIsInstance()
		{
			object e = new NewException();
			Assert.IsTrue(e is IExInterface);

			object f = new EException();
			Assert.IsTrue(!(f is IExInterface));
		}

		[TestMethod]
        public void SimpleAsInstance()
        {
            object e = new EException();
            var e2 = e as EException;
            Assert.IsTrue(null != e2);
			Assert.IsTrue(e == e2);
		}

        [TestMethod]
        public void SimpleAsNull()
        {
            object e = new EException();
            var e2 = e as NewException;
            Assert.IsTrue(null == e2);
        }

        
        public void RefenceCode()
        {
            object e = new EException();


            if(e is Exception)
            {
                System.Console.WriteLine("a");
            } else if (e is EException)
            {
				System.Console.WriteLine("b");
            }
        }


        [TestMethod]
        public void BaseClassIsInstance()
        {
            var e = new NewException();
            Assert.IsTrue(e is Exception);
        }
    }

    [TestClass]
    [ETestFixture]
    public class ExceptionTest
    {
        [TestMethod]
        public void SimplePass()
        {

        }

  //      [TestMethod]
		////[Ignore]
		//[ExpectedException(typeof(Exception))]
  //      public void FailedTestWithException()
  //      {
  //          throw new Exception();
  //      }

        //[TestMethod]
        //public void SimpleTry()
        //{
        //    try
        //    {
        //        throw new Exception();
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("Exception handled");
        //    }
        //}
        //[TestMethod]
        //public void SimpleTry2()
        //{
        //    try
        //    {
        //        throw new Exception();
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //    }
        //}

        //void function()
        //{
        //    throw new Exception();
        //}

        //[TestMethod]
        //public void RetrhowFromFunction()
        //{
        //    try
        //    {
        //        function();
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("Exception handled");
        //    }
        //}
    }
	[TestClass]
	[ETestFixture]
	public class ExceptionTestFinally
	{
		public void load()
		{
			var e = EException.LastException;
		}

		public void store()
		{
			EException.LastException = null;
		}

		[TestMethod]
		public void SimpleTry()
		{
			load();
			store();
			int path = 0;

			if (EException.LastException is EException) { path += 1; }


			try {
				path += 1;
				throw new Exception();
				path += 2;
			}
			//catch(NullReferenceException e)
			//{
			//    Console.WriteLine("NULL Exception handled");
			//}
			catch (Exception e) {
				path += 4;
				System.Console.WriteLine("Exception handled");
			} 
			//finally {
			//	path += 8;
			//	System.Console.WriteLine("FINALLY");
			//}

			Assert.IsTrue(path == 1 + 4);
		}

	}
	[TestClass]
	[ETestFixture]
	public class CatchTest2
	{
		void ThisThrows()
		{
			//EException.LastException = new EException();
			throw new Exception();
			return;
		}

		[Throws]
		void ThisThrows2()
		{
			EException.LastException = new EException();

		}

		[TestMethod]
		public void NestedTry()
		{
			Exception testEx = null;
			bool caught = false;
			try {
				ThisThrows();
			} catch (Exception e) {
				testEx = e;
				caught = true;
			}

			Assert.IsTrue(caught);
			Assert.IsTrue(testEx != null);
		}
	}

	interface IDo
	{
		IDo Do();
	}

	class ExcpetionClass : IDo
	{
		public IDo Do()
		{
			throw new Exception();
		}
	}

	class B : IDo
	{
		public IDo Do()
		{
			return new B();
		}
	}


	[TestClass]
	[ETestFixture]
	public class VirtualExceptions
	{

		[TestMethod]
		public void VirtualTry()
		{
			bool caught = false;
			IDo a = new ExcpetionClass();
			IDo b = new B();
			IDo a_ = null;
			IDo b_ = null;
			try {
				b_ = b.Do();
				a_ = a.Do();
			} catch (Exception e) {
				caught = true;
			}

			Assert.IsTrue(caught);
			Assert.IsTrue(b_ != null);
			Assert.IsTrue(a_ == null);

		}
	}

			//[TestMethod]
			//public void MultiCatch()
			//{
			//	int path = 0;
			//	try {
			//		throw new NewException();
			//	}catch(NewException e) {
			//		path += 1;
			//	} catch (Exception e) {
			//		path += 2;
			//	}

			//	Assert.IsTrue(path == 1);

			//}


			//[TestMethod]
			//public void ReturnTest()
			//{
			//    try
			//    {
			//        Console.WriteLine("Nop");
			//        return;
			//    }
			//    finally
			//    {
			//        Console.WriteLine("Finally");
			//    }
			//    Assert.Fail();
			//}

			//       static bool dummy;

			//[TestMethod]
			//public void MultipleLeaveTest()
			//{
			//    try
			//    {
			//        Console.WriteLine("Nop");
			//        if(dummy)
			//        { 
			//            return;
			//        }
			//    }
			//    finally
			//    {
			//        Console.WriteLine("Finally");
			//    }

			//    Console.WriteLine("Done");
			//}
		//}

    //[TestClass]
    //[ETestFixture]
    //public class FinallyTest
    //{
    //    [TestMethod]
    //    public void TestFinally()
    //    {
    //        int x = 0;
    //        try
    //        {
    //            x += 1;
    //        }
    //        finally
    //        {
    //            x += 2;
    //        }

    //        Assert.IsTrue(x == 3);
    //    }
    //}



    [TestClass]
    [ETestFixture]
    public class StaticFieldETest
    {
        [TestMethod]
        public void TestFieldRead()
        {
            var e = EException.LastException;
			System.Console.WriteLine(e);
        }

        //[TestMethod]
        public void TestFieldWrite()
        {
            EException.LastException = null;            
        }
    }
}
