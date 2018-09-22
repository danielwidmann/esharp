using ESharp.Annotations;
using ECSharp.Core;
using ESharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESharp
{
    public static class Console
    {
        [ExternC]
        static public void Write(EString text)
        {
        }

        [ExternC]
        static public void Write(int text)
        {
        }

        [ExternC]
        static public void WriteLine(EString text)
        {
        }

        [ExternC]
        static public void WriteLine(EObject text)
        {
        }

        [ExternC]
        static public void WriteLine(int number)
        {
        }
    }
}
