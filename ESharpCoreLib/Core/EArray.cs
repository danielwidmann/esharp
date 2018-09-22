using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESharp.Annotations;

namespace ECSharp.Core
{

    //[Skip]
    //class GenericArray
    //{
    //    public int Size;
    //    public int Alignment;

    //    public object Get(int idx) { return null; }
    //    public void Set(object o, int idx) { }
    //}



    [CustomHeaderFile("")]
    [CustomSourceFile("")]
    class Array_4
    {
        public Array_4(int Length) { }
		static public Array_4 @new(int Length) { return null; }
		public int Length;

        public int Get(int idx) { return 0; }
        public void Set(int o, int idx) { }
        public int GetLength() { return 0; }

        //finalizer
    }

    //[Skip]
    [CustomHeaderFile("")]
    [CustomSourceFile("")]
	[Uses(typeof(Array_ref))] // todo split to seperate files
    public class Array_1
    {
        public Array_1(int Length) { }
		static public Array_1 @new(int Length) { return null; }
		public int Length;

        public byte Get(int idx) { return 0; }
        public void Set(int idx, byte o) { }
        public int GetLength() { return 0; }

        //finalizer
    }

    [CustomHeaderFile("")]
    //[CustomHeaderRessource("EArray.h")]
    [CustomSourceFile("EArray.c")]
    public class Array_ref
    {

        public Array_ref(int Length) { }
		static public Array_ref @new(int Length) { return null; }

		public int Length;

        public object Get(int idx) { return null; }
        public void Set(int idx, object o) { }
        public int GetLength() { return 0; }

        public void Finalize()
        {

        }
        //finalizer
    }
}
