using ESharp.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecs.Test.ETests
{
    [TestClass]
    [ETestFixture]
    class MemoryETest
    {
        class A { }

        A ReturnInstance()
        {
            var a = new A();
            return a;
        }

        [TestMethod]
        public void SimpleReturnTest()
        {
            ReturnInstance();
        }
    }
}
