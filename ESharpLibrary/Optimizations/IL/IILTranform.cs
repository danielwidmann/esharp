﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace ESharp.Optimizations.IL
{
	interface IILTranform
	{
		void TransformIL(IEnumerable<TypeDefinition> types);
	}
}
