using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESharp.Annotations;

namespace ECSharp
{
    [Skip]
    public class C
    {

        public static void Call(string name) { }
        public static void Call(string name, object arg1) { }
        public static void Call(string name, object arg1, object arg2) { }
        public static void Call(string name, object arg1, object arg2, object arg3) { }
        public static void Call(string name, object arg1, object arg2, object arg3, object arg4) { }

        public static T Call<T>(string name)  { return default(T); }
        public static T Call<T>(string name, object arg1) { return default(T); }
        public static T Call<T>(string name, object arg1, object arg2) { return default(T); }
        public static T Call<T>(string name, object arg1, object arg2, object arg3) { return default(T); }
        public static T Call<T>(string name, object arg1, object arg2, object arg3, object arg4) { return default(T); }
        
        public static void Code(string code) { }
        public static void Code(string code, object arg1) { }
        public static void Code(string code, object arg1, object arg2) { }
        public static void Code(string code, object arg1, object arg2, object arg3) { }
        public static void Code(string code, object arg1, object arg2, object arg3, object arg4) { }


    }
}
