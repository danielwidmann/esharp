using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESharp.Annotations;

namespace ECSharp.Core
{
    [CustomHeaderFile("ESharpCore.h")]
    [CustomSourceFile("ESharpCore.c")]
    public static class ESharpRT
    {
		public static void Error()
		{ }
    }
}
