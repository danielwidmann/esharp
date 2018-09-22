using ESharp.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecs.Test.ETests
{

    //public class ArrayTestTarget
    //{
    //    static void Main()
    //    {
    //        var array = new int[3];
    //        array[0] = 1;
    //        array[1] = 2;

    //        foreach (var v in array)
    //        {
    //            System.Console.WriteLine(v);
    //        }

    //        TestRef();
    //    }

    //    class A
    //    {
    //        int i;
    //    }

    //    static void TestRef()
    //    {
    //        var x = new A[5];

    //        x[0] = new A();
    //        x[2] = new A();

    //        foreach (var v in x)
    //        {
    //            if (v == null)
    //            {
    //                System.Console.WriteLine("Null");
    //            }
    //            else
    //            {
    //                System.Console.WriteLine(v);
    //            }
    //        }
    //    }


    //}

    //public class ArrayTestSuite
    //{
    //    [TestMethod]
    //    public void ArrayTeest_Int()
    //    {
    //        EcsTestRunner.RunTest("ArrayTestTarget", "12");
    //    }
    //}
    [ETestFixture]
    [TestClass]
    public class ArrayTest
    {
        [TestMethod]
        public void TestIntArray()
        {
            var array = new int[3];
            array[0] = 1;
            array[1] = 2;

            int total = 0;
            foreach (var v in array)
            {
                total += v;
            }

            Assert.IsTrue(3 == total);
        }

        [TestMethod]
        public void TestByteArray()
        {
            var array = new byte[3];
            array[0] = 1;
            array[1] = 2;

            int total = 0;
            foreach (var v in array)
            {
                total += v;
            }

            Assert.IsTrue(3 == total);
        }

         class E
         {
             public int i;
         }

         [TestMethod]
         public void TestRefArray()
         {
             var x = new E[5];
             x[0] = new E { i = 4 };
             x[2] = new E { i = 2 };

             int total = 0;
             foreach (var v in x)
             {
                 if (v != null)
                 {
                     total += v.i;
                 }
             }
             Assert.IsTrue(total == 6);
         }

        [TestMethod]
        public void TestArrayInitializer()
        {
            var array = new byte[] {1,2,3};

            Assert.IsTrue(array[0] == 1);
            Assert.IsTrue(array[1] == 2);
            Assert.IsTrue(array[2] == 3);
        }

    }
    [ETestFixture]
    [TestClass]
    public class ArrayTest_NoInit
    {
        [TestMethod]
        public void TestArrayInitializer_NoVar()
        {
            
            Console.WriteLine(new byte[] { 1, 2, 3 });
        }
        //static byte[] m_array = new byte[] {1,2,3};
        //[TestMethod]
        //public void TestArray_StaticFiledInitializer()
        //{
        //    Assert.IsTrue(m_array[0] == 1);
        //    Assert.IsTrue(m_array[1] == 2);
        //    Assert.IsTrue(m_array[2] == 3);
        //}

        //public class Strings
        //{
        //    public static readonly string hello = "Hello ";
        //}
    }
    [ETestFixture]
    [TestClass]
    public class StringTestClass
    {
        [TestMethod]
        public void StringTest()
        {
            //var s1 = Strings.hello;
            var s1 = "Hello ";
            var s2 = "World!";
            var s3 = s1 + s2;

            Assert.IsTrue( s3 == "Hello World!");
        }
    }
}
