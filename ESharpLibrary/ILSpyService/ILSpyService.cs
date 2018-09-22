//using ECSharp;
using EcsTarget;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ESharp.ILSpyService;
using ESharp.Optimizations.ILAst;
using ICSharpCode.NRefactory.CSharp;
using ESharp.Optimizations.Ast;
using ICSharpCode.Decompiler.IL.Transforms;
using ICSharpCode.Decompiler.Metadata;

namespace ESharp.ILSpyService
{


	public class DecompilerService
	{
		CSharpDecompiler m_decompiler;
		DecompilerSettings m_settings;


		CSharpDecompiler CreateDecompiler(string module, DecompilerSettings settings)
		{
			CSharpDecompiler decompiler = new CSharpDecompiler(module, settings);
			//decompiler.CancellationToken = options.CancellationToken;
			//while (decompiler.AstTransforms.Count > transformCount)
			//	decompiler.AstTransforms.RemoveAt(decompiler.AstTransforms.Count - 1);
			return decompiler;
		}

		public IEnumerable<EcsFile> Generate(IEnumerable<TypeDefinition> types, string assemblyName)
		{
			var files = new List<EcsFile>();

			m_settings = new DecompilerSettings();
			m_settings.ShowXmlDocumentation = false;
            m_settings.NamedArguments = false;
            m_settings.NonTrailingNamedArguments = false;
            m_settings.OptionalArguments = false;

			m_decompiler = CreateDecompiler(assemblyName, m_settings);
			m_decompiler.ILTransforms.Insert(0, new RefCounting());
			m_decompiler.ILTransforms.Remove(m_decompiler.ILTransforms.Single(x=>x.GetType() == typeof(AssignVariableNames)));
			m_decompiler.AstTransforms.Add(new AddBaseStruct());
			m_decompiler.AstTransforms.Add(new RemoveStaticMemberAccess());

			foreach (var t in types) {
                var syntaxTree = new Lazy<SyntaxTree>(() => m_decompiler.DecompileType(new FullTypeName(t.FullName)));
				files.Add(GenerateCodeFile(t, syntaxTree, OutputMode.Header));
				files.Add(GenerateCodeFile(t, syntaxTree, OutputMode.Source));
			}

			return files;
		}


		public EcsFile GenerateCodeFile(TypeDefinition type, Lazy<SyntaxTree> syntaxTree, OutputMode mode)
		{
			var outputName = type.Name + mode.extensions;
			//var sourceRes = type.CustomAttributes.SingleOrDefault(x => x.AttributeType.Name == "Custom" + mode.Name + "File");
			//if (sourceRes != null) {
			//	var sourceName = (string)sourceRes.ConstructorArguments[0].Value;

			//	if (sourceName == "") {
			//		// no filename provided, create empty output
			//		return new EcsFile(outputName, "");
			//	}				

			//	var sourceContent = EmbeddedFileLoader.GetResourceFile(sourceName, type.Module);
			//	return new EcsFile(sourceName, sourceContent);
			//}

			var sourceRes = type.CustomAttributes.SingleOrDefault(x => x.AttributeType.Name == "Custom" + mode.Name);
			if (sourceRes != null) {
				var sourceContent = (string)sourceRes.ConstructorArguments[0].Value;						
				return new EcsFile(outputName, sourceContent);
			}


			var code = WriteCode(m_settings, syntaxTree.Value , m_decompiler.TypeSystem, mode);
			return new EcsFile(outputName, code);
		}

		//private static EcsFile GenerateFile(TypeDefinition t, CSharpDecompiler codeDomBuilder, string attributeString, OutputMode mode, string ext)
		//{
		//	var sourceRes = t.CustomAttributes.SingleOrDefault(x => x.AttributeType.Name == "Custom" + attributeString + "File");
		//	var sourceAtt = t.CustomAttributes.SingleOrDefault(x => x.AttributeType.Name == "Custom" + attributeString);
		//	if (sourceRes != null) {
		//		var sourceName = t.Name + ext;
		//		sourceName = (string)sourceRes.ConstructorArguments[0].Value;
		//		var sourceContent = EmbeddedFileLoader.GetResourceFile(sourceName, t.Module);
		//		return new EcsFile(sourceName, sourceContent);
		//	}
		//	if (sourceAtt != null) {
		//		var sourceContent = (string)sourceAtt.ConstructorArguments[0].Value;
		//		return new EcsFile(t.Name + ext, sourceContent);
		//	}

		//	return new EcsFile(t.Name + ext, WriteType(codeDomBuilder, mode));
		//}

		string WriteCode(DecompilerSettings settings, SyntaxTree syntaxTree, IDecompilerTypeSystem typeSystem, OutputMode mode)
		{
			//syntaxTree.AcceptVisitor(new InsertParenthesesVisitor { InsertParenthesesForReadability = true });
		
			var ms = new MemoryStream();
			using (StreamWriter w = new StreamWriter(ms)) {
				//codeDomBuilder.GenerateCode(new PlainTextOutput(w), mode);
				TokenWriter tokenWriter = new TextTokenWriter(new PlainTextOutput(w), settings, typeSystem) { FoldBraces = settings.FoldBraces, ExpandMemberDefinitions = settings.ExpandMemberDefinitions };
				//syntaxTree.AcceptVisitor(new CSharpOutputVisitor(tokenWriter, settings.CSharpFormattingOptions));

				syntaxTree.AcceptVisitor(new COutputVisitor(tokenWriter, settings.CSharpFormattingOptions, mode));

				w.Flush();
				ms.Position = 0;
				var sr = new StreamReader(ms);
				var myStr = sr.ReadToEnd();
				return myStr;
			}			
		}
	}

  //  class ILSpyService_old
  //  {
  //      static AstBuilder CreateAstBuilder(CancellationToken cancelToken, DecompilerSettings settings, ModuleDefinition currentModule = null, TypeDefinition currentType = null, bool isSingleMember = false)
		//{
		//	if (currentModule == null)
		//		currentModule = currentType.Module;
		//	//DecompilerSettings settings = options.DecompilerSettings;
		//	if (isSingleMember) {
		//		settings = settings.Clone();
		//		settings.UsingDeclarations = false;
		//	}
		//	return new AstBuilder(
		//		new DecompilerContext(currentModule) {
		//			CancellationToken = cancelToken,
		//			CurrentType = currentType,
		//			Settings = settings
		//		});
		//}

  //      public static IEnumerable<EcsFile> Generate(IEnumerable<TypeDefinition> types)
  //      {
  //          var files = new List<EcsFile>();
  //          var cancelToken = new CancellationToken();

  //          AstMethodBodyBuilder.ClearUnhandledOpcodes();
  //         // Parallel.ForEach(
  //         //     files,
  //         //     new ParallelOptions { MaxDegreeOfParallelism = 1 },
  //         //     delegate(IGrouping<string, TypeDefinition> file)

  //          var settings = new DecompilerSettings ();
  //          settings.UseDebugSymbols = true;
            
  //          foreach(var t in types)
  //          {
  //              cancelToken = GenerateClass(files, cancelToken, settings, t);
  //          }
  //          return files;
  //      }

  //      private static CancellationToken GenerateClass(List<EcsFile> files, CancellationToken cancelToken, DecompilerSettings settings, TypeDefinition t)
  //      {
  //          AstBuilder codeDomBuilder;
  //          codeDomBuilder = CreateCodeCom(ref cancelToken, settings, t);

  //          files.Add(GenerateFile(t, codeDomBuilder, "Source", OutputMode.Source,".c"));
  //          files.Add(GenerateFile(t, codeDomBuilder, "Header", OutputMode.Header, ".h"));
                      
  //          return cancelToken;
  //      }

  //      private static EcsFile GenerateFile(TypeDefinition t, AstBuilder codeDomBuilder, string attributeString, OutputMode mode, string ext)
  //      {
  //          var sourceRes = t.CustomAttributes.SingleOrDefault(x => x.AttributeType.Name == "Custom" + attributeString + "File");
  //          var sourceAtt = t.CustomAttributes.SingleOrDefault(x => x.AttributeType.Name == "Custom" + attributeString);
  //          if (sourceRes != null)
  //          {
  //              var sourceName = t.Name + ext;
  //              sourceName = (string)sourceRes.ConstructorArguments[0].Value;
  //              var sourceContent = EmbeddedFileLoader.GetResourceFile(sourceName, t.Module);
  //              return new EcsFile(sourceName, sourceContent);
  //          }
  //          if (sourceAtt != null)
  //          {
  //              var sourceContent = (string)sourceAtt.ConstructorArguments[0].Value;
  //              return new EcsFile(t.Name + ext, sourceContent);
  //          }

  //          return new EcsFile(t.Name + ext, WriteType(codeDomBuilder, mode));
  //      }

  //      private static AstBuilder CreateCodeCom(ref CancellationToken cancelToken, DecompilerSettings settings, TypeDefinition t)
  //      {
  //          AstBuilder codeDomBuilder;
  //          codeDomBuilder = CreateAstBuilder(cancelToken, settings, t.Module, t);
  //          codeDomBuilder.AddType(t);
  //          codeDomBuilder.RunTransformations(null);
  //          return codeDomBuilder;
  //      }

  //      private static string WriteType(AstBuilder codeDomBuilder, OutputMode mode)
  //      {
  //          var ms = new MemoryStream();
  //          using (StreamWriter w = new StreamWriter(ms))
  //          {
  //              codeDomBuilder.GenerateCode(new PlainTextOutput(w), mode);

  //              w.Flush();
  //              ms.Position = 0;
  //              var sr = new StreamReader(ms);
  //              var myStr = sr.ReadToEnd();
  //              return myStr;
  //          }
            
  //      }
  //  }
}
