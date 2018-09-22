using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Disassembler;
using ICSharpCode.Decompiler.IL;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ICSharpCode.Decompiler.IL;
using ESharp.Optimizations.ILAst;
using ICSharpCode.Decompiler.TypeSystem;

namespace ESharp.Helpers
{
	public class ILAstDebugPrinter
	{
		const string logsDir = "logs";

		static int s_totalCount = 1000;

		public static void ClearLogs()
		{
			Directory.CreateDirectory(logsDir);

			System.IO.DirectoryInfo di = new DirectoryInfo(logsDir);

			foreach (FileInfo file in di.GetFiles()) {
				file.Delete();
			}
		}

		public static void DebugIlAst(ILFunction ilMethod, string stage, Dictionary<ILInstruction, string> dict)
		{
			DebugIlAst(ilMethod, stage, x => dict.ContainsKey(x) ? dict[x] : "\t");
		}

		public static void DebugIlAst(ILFunction ilMethod, string stage, Func<ILInstruction, string> callback)
		{
			StringWriter w = new StringWriter();
			ITextOutput output = new PlainTextOutput(w);

			//if (context.CurrentMethodIsAsync)
			//	output.WriteLine("async/await");

			//var allVariables = ilMethod.GetSelfAndChildrenRecursive<ILExpression>().Select(e => e.Operand as ILVariable)
			//	.Where(v => v != null && !v.IsParameter).Distinct();
			//foreach (ILVariable v in allVariables) {
			//	if (v.Name == null)
			//		continue;

			//	output.WriteDefinition(v.Name, v);
			//	if (v.Type != null) {
			//		output.Write(" : ");
			//		if (v.IsPinned)
			//			output.Write("pinned ");
			//		v.Type.WriteTo(output, ILNameSyntax.ShortTypeName);

			//		if (v.Type.IsByReference)
			//			output.Write("*");

			//	}
			//	if (v.IsGenerated) {
			//		output.Write(" [generated]");
			//	}
			//	output.WriteLine();
			//}
			//output.WriteLine();

			//for (int idx = 0; idx < ilMethod.Body.Count; idx++) {
			//	if (info != null) {
			//		output.Write(info[idx]);
			//		output.Write("\t");
			//	}

			//	var node = ilMethod.Body[idx];
			//	node.WriteTo(output);


			//	output.WriteLine();
			//}

			WriteIlFunctionTo(ilMethod, output, new ILAstWritingOptions(), callback);


			var filename = s_totalCount++ + "_" + ilMethod.Method.Name + "_" + stage;
			if (filename.Length > 50) {
				filename = filename.Substring(0, 50);
			}
			File.WriteAllText(logsDir + "/" + filename + ".ilasm", w.ToString());

		}

		public static void WriteIlFunctionTo(ILFunction function, ITextOutput output, ILAstWritingOptions options, Func<ILInstruction, string> callback)
		{			
			//output.Write(function.OpCode);
			output.Write(originalOpCodeNames[(int)function.OpCode]);
			if (function.Method != null) {
				output.Write(' ');
				
				//function.Method.WriteTo(output);
				if (function.Method is IMethod method && method.IsConstructor)
					output.WriteReference(function.Method, method.DeclaringType?.Name + "." + method.Name);
				else
					output.WriteReference(function.Method, function.Method.Name);
			}
			if (function.IsExpressionTree) {
				output.Write(".ET");
			}
			if (function.DelegateType != null) {
				output.Write("[");
				//function.DelegateType.WriteTo(output);
				output.WriteReference(function.DelegateType, function.DelegateType.ReflectionName);
				output.Write("]");
			}
			output.WriteLine(" {");
			output.Indent();

			if (function.IsAsync) {
				output.WriteLine(".async");
			}
			if (function.IsIterator) {
				output.WriteLine(".iterator");
			}

			output.MarkFoldStart(function.Variables.Count + " variable(s)", true);
			foreach (var variable in function.Variables) {
				//variable.WriteDefinitionTo(output);
				WriteVarialbeDefinitionTo(variable, output);
				output.Write(" type: ");
				var ty = TypeInference.GetVariableType(variable);
				output.Write(ty?.IsReferenceType == false ? "(struct) ":"");
				output.Write(ty?.Name ?? "none");
				var alias = TypeInference.GetAliasSource(variable);
				output.Write(" alias of: ");
				output.Write(alias?.Name ?? "none");

				var pointers = TypeInference.GetPointers(variable);
				output.Write(" pointers: ");
				foreach(var p in pointers) {
					output.Write(p.Name + ",");
				}

				output.WriteLine();
			}
			output.MarkFoldEnd();
			output.WriteLine();

			foreach (string warning in function.Warnings) {
				output.WriteLine("//" + warning);
			}

			WriteBlockContainerTo((BlockContainer)function.Body, output, options, callback);
			output.WriteLine();

			//if (options.ShowILRanges) {
			//	var unusedILRanges = FindUnusedILRanges();
			//	if (!unusedILRanges.IsEmpty) {
			//		output.Write("// Unused IL Ranges: ");
			//		output.Write(string.Join(", ", unusedILRanges.Intervals.Select(
			//			range => $"[{range.Start:x4}..{range.InclusiveEnd:x4}]")));
			//		output.WriteLine();
			//	}
			//}

			output.Unindent();
			output.WriteLine("}");
		}

		static internal void WriteVarialbeDefinitionTo(ILVariable v, ITextOutput output)
		{
			switch (v.Kind) {
				case VariableKind.Local:
					output.Write("local ");
					break;
				case VariableKind.PinnedLocal:
					output.Write("pinned local ");
					break;
				case VariableKind.Parameter:
					output.Write("param ");
					break;
				case VariableKind.Exception:
					output.Write("exception ");
					break;
				case VariableKind.StackSlot:
					output.Write("stack ");
					break;
				case VariableKind.InitializerTarget:
					output.Write("initializer ");
					break;
				case VariableKind.ForeachLocal:
					output.Write("foreach ");
					break;
				case VariableKind.UsingLocal:
					output.Write("using ");
					break;
				case VariableKind.NamedArgument:
					output.Write("named_arg ");
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			output.Write(v.Name);            
			output.Write(" : ");
            //v.Type.WriteTo(output);
            //WriteVarialbeDefinitionTo(v, output);
            output.Write(v.Type.Name);
			output.Write('(');
			if (v.Kind == VariableKind.Parameter || v.Kind == VariableKind.Local || v.Kind == VariableKind.PinnedLocal) {
				output.Write("Index={0}, ", v.Index);
			}
			output.Write("LoadCount={0}, AddressCount={1}, StoreCount={2})", v.LoadCount, v.AddressCount, v.StoreCount);
			if (v.HasInitialValue && v.Kind != VariableKind.Parameter) {
				output.Write(" init");
			}
			if (v.CaptureScope != null) {
				output.Write(" captured in " + v.CaptureScope.EntryPoint.Label);
			}
			if (v.StateMachineField != null) {
				output.Write(" from state-machine");
			}
		}

		public static void WriteBlockContainerTo(BlockContainer b, ITextOutput output, ILAstWritingOptions options, Func<ILInstruction, string> callback)
		{
			//ILRange.WriteTo(output, options);
			output.Write("BlockContainer");
			output.Write(' ');
			switch (b.Kind) {
				case ContainerKind.Loop:
					output.Write("(while-true) ");
					break;
				case ContainerKind.Switch:
					output.Write("(switch) ");
					break;
				case ContainerKind.While:
					output.Write("(while) ");
					break;
				case ContainerKind.DoWhile:
					output.Write("(do-while) ");
					break;
				case ContainerKind.For:
					output.Write("(for) ");
					break;
			}
			output.MarkFoldStart("{...}");
			output.WriteLine("{");
			output.Indent();
			foreach (var inst in b.Blocks) {
				if (inst.Parent == b) {
					WriteBlockTo(inst, output, options, callback);
				} else {
					output.Write("stale reference to ");
					output.Write(inst.Label);
				}
				output.WriteLine();
				output.WriteLine();
			}
			output.Unindent();
			output.Write("}");
			output.MarkFoldEnd();
		}

		public static void WriteBlockTo(Block b, ITextOutput output, ILAstWritingOptions options, Func<ILInstruction, string> callback)
		{
			//ILRange.WriteTo(output, options);
			output.Write("Block ");
			output.Write(b.Label);
			if (b.Kind != BlockKind.ControlFlow)
				output.Write($" ({b.Kind})");
			if (b.Parent is BlockContainer)
				output.Write(" (incoming: {0})", b.IncomingEdgeCount);
			output.Write(' ');
			output.MarkFoldStart("{...}");
			output.WriteLine("{");
			output.Indent();
			foreach (var inst in b.Instructions) {
				WriteInstTo(inst, output, options, callback);
				output.WriteLine();
			}
			if (b.FinalInstruction.OpCode != OpCode.Nop) {
				output.Write("final: ");
				b.FinalInstruction.WriteTo(output, options);
				output.WriteLine();
			}
			output.Unindent();
			output.Write("}");
			output.MarkFoldEnd();
		}

		public static void WriteInstTo(ILInstruction i, ITextOutput output, ILAstWritingOptions options, Func<ILInstruction, string> callback)
		{
			output.Write(callback.Invoke(i));
			i.WriteTo(output, options);
		}

			public static void DebugIl(TypeDefinition type, string stage)
		{
			foreach (var m in type.Methods) {
				DebugIl(m, stage);
			}
		}

		public static void DebugIl(MethodDefinition method, string stage)
		{
			StringWriter w = new StringWriter();
			ITextOutput output = new PlainTextOutput(w);

			CancellationTokenSource source = new CancellationTokenSource();

			//var dis = new ReflectionDisassembler(output, true, source.Token);
			//dis.DisassembleMethod(method);



			var mdis = new MethodBodyDisassembler(output, source.Token);
			//MethodDebugSymbols debugSymbols = new MethodDebugSymbols(method);
			
            // todo: do we still need that having ilspy to look at the disassembly?
            //if (method.Body != null)
			//	mdis.Disassemble(method.Body);

			//DisassemblerHelpers.WriteTo(method, output);

			//foreach (var inst in method.Body.Instructions)
			//{
			//    output.WriteLine();
			//    DisassemblerHelpers.WriteTo(inst, output);
			//}

			var filename = logsDir + "/" + s_totalCount++ + "_" + method.Name + "_" + stage + ".il";
			filename = filename.Replace('<', '_').Replace('>', '_');
			File.WriteAllText(filename, w.ToString());

		}

		static readonly string[] originalOpCodeNames = {
			"invalid.branch",
			"invalid.expr",
			"nop",
			"ILFunction",
			"BlockContainer",
			"Block",
			"PinnedRegion",
			"binary",
			"numeric.compound",
			"user.compound",
			"dynamic.compound",
			"bit.not",
			"arglist",
			"br",
			"leave",
			"if",
			"if.notnull",
			"switch",
			"switch.section",
			"try.catch",
			"try.catch.handler",
			"try.finally",
			"try.fault",
			"lock",
			"using",
			"debug.break",
			"comp",
			"call",
			"callvirt",
			"calli",
			"ckfinite",
			"conv",
			"ldloc",
			"ldloca",
			"stloc",
			"addressof",
			"3vl.logic.and",
			"3vl.logic.or",
			"nullable.unwrap",
			"nullable.rewrap",
			"ldstr",
			"ldc.i4",
			"ldc.i8",
			"ldc.f4",
			"ldc.f8",
			"ldc.decimal",
			"ldnull",
			"ldftn",
			"ldvirtftn",
			"ldtypetoken",
			"ldmembertoken",
			"localloc",
			"cpblk",
			"initblk",
			"ldflda",
			"ldsflda",
			"castclass",
			"isinst",
			"ldobj",
			"stobj",
			"box",
			"unbox",
			"unbox.any",
			"newobj",
			"newarr",
			"default.value",
			"throw",
			"rethrow",
			"sizeof",
			"ldlen",
			"ldelema",
			"array.to.pointer",
			"string.to.int",
			"expression.tree.cast",
			"dynamic.binary.operator",
			"dynamic.unary.operator",
			"dynamic.convert",
			"dynamic.getmember",
			"dynamic.setmember",
			"dynamic.getindex",
			"dynamic.setindex",
			"dynamic.invokemember",
			"dynamic.invokeconstructor",
			"dynamic.invoke",
			"dynamic.isevent",
			"mkrefany",
			"refanytype",
			"refanyval",
			"yield.return",
			"await",
			"AnyNode",
		};
	}
}
