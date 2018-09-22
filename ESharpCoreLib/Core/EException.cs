using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESharp
{
    public class EException
    {
        public static EException LastException;

        /// <summary>
        /// This method is called 
        /// </summary>
        /// <param name="e"></param>
        public static void Throw(EException e)
        {
            LastException = e;
        }


        public string Description;
        public int Code;
    }
}
