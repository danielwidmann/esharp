using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESharp
{
    public class AssertException: Exception
    {

    }

    public class EAssert
    {
        public static bool exception = false;
        public static void IsTrue(bool condition)
        {
            if(!condition)
            {
				System.Console.WriteLine("Assertion Failed!");
                
				throw new AssertException();				
			}
        }

        public static void Equals(ESharp.EObject a, ESharp.EObject b)
        {
            bool eq = (a == b);
            IsTrue(eq);
        }
    }
}
