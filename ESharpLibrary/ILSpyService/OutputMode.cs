using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESharp.ILSpyService
{
	public class OutputMode
	{

		public static OutputMode Header = new OutputMode { extensions = ".h", Name="Header" };
		public static OutputMode Source = new OutputMode { extensions = ".c", Name = "Source" };

		public string extensions { get; set; }
		public string Name { get; set; }

	}
}
