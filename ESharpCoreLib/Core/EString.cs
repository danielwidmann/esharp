using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ECSharp;
using ESharp.Annotations;

namespace ECSharp.Core
{

    [CustomSourceFile("EString.c")]
    [CustomHeaderFile("EString.h")]
	[Uses(typeof(Array_1))]
    public class EString
    {
        static EString Concat(EString a, EString b)
        {
            return null;
        }

        static bool op_Equality(EString a, EString b)
        {
            return false;
        }
    }    
}
