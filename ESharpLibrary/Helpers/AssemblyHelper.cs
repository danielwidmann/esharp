using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcsCompilerService
{
    public class AssemblyHelper
    {
        static public ModuleDefinition LoadAssemblyWithResolver(string assemblyName, bool readWrite = false)
        {
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(Path.GetDirectoryName(assemblyName));

			var parameters = new ReaderParameters {
				ReadSymbols = true,
				AssemblyResolver = resolver,
				InMemory = readWrite,
				ThrowIfSymbolsAreNotMatching = false,		
				SymbolReaderProvider = new DefaultSymbolReaderProvider(false)
			};


            var module = ModuleDefinition.ReadModule(assemblyName, parameters);

            return module;
        }
    }
}
