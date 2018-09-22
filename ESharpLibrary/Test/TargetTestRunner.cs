using System;
using System.Collections.Generic;
using System.Text;

namespace ESharpLibrary.Test
{
    class TargetTestRunner
    {        
        public static int Run(ECSharp.Action test, string name)
        {            
            Console.Write("\nStarting Test: ");
            Console.WriteLine(name);
            try
            {
                test.Invoke();
            } catch (Exception e)
            {
                Console.WriteLine("TEST FAILED");
                return 1;
            }

            return 0;
        }
    }
}
