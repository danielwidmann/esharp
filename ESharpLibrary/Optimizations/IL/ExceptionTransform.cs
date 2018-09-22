using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESharp.Annotations;
using ESharp.Helpers;
using ICSharpCode.Decompiler.ECS;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ESharp.Optimizations.IL
{
	class ExceptionTransform : IILTranform
	{
		ReferenceResolver m_resolver;
		ExceptionHelper m_helper = new ExceptionHelper();


		//todo move
		Instruction CopyInstruction(Instruction org)
		{
			var inst = Instruction.Create(OpCodes.Nop);
			inst.OpCode = org.OpCode;
			inst.Operand = org.Operand;
			return inst;
		}

		public void TransformIL(IEnumerable<TypeDefinition> types)
        {

			m_resolver = new ReferenceResolver(types);
			var mod = ModuleDefinition.CreateModule("t", ModuleKind.Dll);

			foreach (var t in types) {
				foreach (var m in t.Methods) {
					if (!m.HasBody)
						continue;

                    var exceptionVariable = new VariableDefinition(m_resolver.GetTypeReference(typeof(EException)));
                    m.Body.Variables.Add(exceptionVariable);

                    var ilp = m.Body.GetILProcessor();
					var insts = m.Body.Instructions;


                    for (int idx = 0; idx < m.Body.Instructions.Count; idx++)
                    {
                        var i = m.Body.Instructions[idx];
                        var next = i.Next;

                        //store thrown exceptions in variable, this is how the rest of this function expects it
                        if (i.OpCode == OpCodes.Throw)
                        {
                            ilp.InsertBefore(i, ilp.Create(OpCodes.Stloc, exceptionVariable));
                            idx++;
                        }
                    }


                    RethrowCallException(m, exceptionVariable);                    

					// remember that this method might throw.
					// todo, base this on the outcome of this routine??
					if (m_helper.CanThrow(m))
						m.CustomAttributes.Add(new CustomAttribute(mod.ImportReference(typeof(Throws).GetConstructor(new Type[] { })), new byte[] { 1, 0, 0, 0 }));



					if (m.Body.Instructions.Any(x=>x.OpCode == OpCodes.Throw))
						AddUncaugtCatch(m, exceptionVariable);



					foreach (var handlerGroup in m.Body.ExceptionHandlers.GroupBy(x => Tuple.Create(x.TryStart, x.TryEnd)).ToArray()) { //x.TryStart.Offset << 16 + x.TryEnd.Offset


						// todo support multiple catch handlers
						var catchHandler = handlerGroup.SingleOrDefault(x => x.HandlerType == ExceptionHandlerType.Catch);
						var finallyHandler = handlerGroup.SingleOrDefault(x => x.HandlerType == ExceptionHandlerType.Finally);

						if (catchHandler != null) { // catch handler
							ReplaceCatch(ilp, catchHandler, exceptionVariable);

							// remove old catch
							var block = insts.SkipWhile(x => x != catchHandler.HandlerStart).TakeWhile(x => x != catchHandler.HandlerEnd).ToArray();
							foreach (var bi in block) {
								ilp.Remove(bi);
							}
							m.Body.ExceptionHandlers.Remove(catchHandler);

							// todo remove handler
						}
						//{ // finally handler

						//}



					}

					
					Debug.Assert(!m.Body.Instructions.Any(x => x.OpCode == OpCodes.Throw), "No throw insturction should be remaining");
					Debug.Assert(!m.Body.HasExceptionHandlers, "No Exception Handlers should be remaining");

				}
			}
		}

		private void ReplaceCatch(ILProcessor ilp, ExceptionHandler catchHandler, VariableDefinition exceptionVariable)
		{
			var insts = ilp.Body.Instructions;
			var tryStart = catchHandler.TryStart;
			var tryEnd = catchHandler.TryEnd;

			var catchStart = ilp.Create(OpCodes.Nop);
			var catchEnd = ilp.Create(OpCodes.Nop);
			var finallyStart = ilp.Create(OpCodes.Nop);

			bool thrown = false;

			// replace throw with jump to catch handler
			for (int idx = insts.IndexOf(tryStart); idx <= insts.IndexOf(tryEnd); idx++) {
				var i = insts[idx];
				if (i.OpCode == OpCodes.Throw) {
                    //ilp.Replace(i, ilp.Create(OpCodes.Br, catchStart));
                    // need a leave here as there might still be things on the stack
                    ilp.Replace(i, ilp.Create(OpCodes.Leave, catchStart));
                    thrown = true;
				}
			}

			// only use catch block if there is a chance of reaching it
			if (thrown) {
				// add labels
				ilp.InsertBefore(tryEnd, catchStart);
				ilp.InsertBefore(tryEnd, catchEnd);

				// add catch check
				var next = ilp.Create(OpCodes.Nop);
				if (catchHandler.CatchType.Name != "EException") {
					ilp.InsertBefore(catchEnd, ilp.Create(OpCodes.Ldloc, exceptionVariable));
					ilp.InsertBefore(catchEnd, ilp.Create(OpCodes.Call, m_resolver.GetMethodReference(catchHandler.CatchType, "isInstance", after_rename: true)));
					ilp.InsertBefore(catchEnd, ilp.Create(OpCodes.Brfalse, next));
				}

                // copy catch handler (Excpetion is expected on top of stack)
                ilp.InsertBefore(catchEnd, ilp.Create(OpCodes.Ldloc, exceptionVariable));
                var block = insts.SkipWhile(x => x != catchHandler.HandlerStart).TakeWhile(x => x != catchHandler.HandlerEnd).ToArray();
				foreach (var bi in block) {
					ilp.InsertBefore(catchEnd, CopyInstruction(bi));
				}
								
				ilp.InsertBefore(catchEnd, next);


				// rethrow uncaught exception
				if (catchHandler.CatchType.Name != "EException") {
					ilp.InsertBefore(next, ilp.Create(OpCodes.Throw));
				}
			}
		}


		public void RethrowCallException(MethodDefinition m, VariableDefinition exceptionVariable)
		{
			if (!m.HasBody)
				return;

			var ilp = m.Body.GetILProcessor();
			for (int idx = 0; idx < m.Body.Instructions.Count; idx++) {
				var i = m.Body.Instructions[idx];
				var next = i.Next;

                if (i.OpCode != OpCodes.Call && i.OpCode != OpCodes.Callvirt)
				continue;

				var target = (i.Operand as MethodReference).Resolve();

				if (!m_helper.CanThrow(target))
					continue;

				// load exception
				var fieldref = m_resolver.GetField("ESharp.EException", "S_EException_LastException");
				ilp.InsertBefore(next, ilp.Create(OpCodes.Ldsfld, fieldref));
                ilp.InsertBefore(next, ilp.Create(OpCodes.Stloc, exceptionVariable));

                //clear global variable S_EException_LastException
                ilp.InsertBefore(next, ilp.Create(OpCodes.Ldnull));
				ilp.InsertBefore(next, ilp.Create(OpCodes.Stsfld, fieldref));

				// throw exception if != null
				var after = ilp.Create(OpCodes.Nop);
                ilp.InsertBefore(next, ilp.Create(OpCodes.Ldloc, exceptionVariable));
                ilp.InsertBefore(next, ilp.Create(OpCodes.Brfalse, after));
				ilp.InsertBefore(next, ilp.Create(OpCodes.Throw));
				ilp.InsertBefore(next, after);

			}
		}

		// This logic will catch uncaught exceptions when leaving a function
		public void AddUncaugtCatch(MethodDefinition m, VariableDefinition excpetionVariable)
		{
			if (!m.HasBody || m.Body.Instructions.Count == 0)
				return;

			var insts = m.Body.Instructions;
			var ilp = m.Body.GetILProcessor();

			var retType = m.ReturnType.Resolve();
			var useRetType = retType.Name != "Void";

			var catchLabel = ilp.Create(OpCodes.Nop);
			var retunLabel = ilp.Create(OpCodes.Nop);


			if(!insts.Any(x=>x.OpCode == OpCodes.Ret)) {
				// no ret, eg because the function throws; need dummy ret

				if (retType.Name != "Void") {
					if (retType.IsValueType) {
						var variable = new VariableDefinition(retType);
						m.Body.Variables.Add(variable);
						ilp.Emit(OpCodes.Ldloca, variable);
						ilp.Emit(OpCodes.Initobj, retType);
						ilp.Emit(OpCodes.Ldloc, variable);
					} else {
						ilp.Emit(OpCodes.Ldnull);
					}
				}

				ilp.Emit(OpCodes.Ret);
			}

			var old_ret = insts.Last();
			Debug.Assert(old_ret.OpCode == OpCodes.Ret, "Ret instruction expected at the end of each function");

			// No need for leave here as stack is always correct.
			// Actually don't use leave as a return code might be on the stack.
			ilp.InsertBefore(old_ret, ilp.Create(OpCodes.Br, retunLabel));

            ilp.InsertBefore(old_ret, catchLabel);
			ilp.InsertBefore(old_ret, retunLabel);

			// create catch block (store exception, exception should be on the top of the stack)			
			var fieldref = m_resolver.GetField("ESharp.EException", "S_EException_LastException");
			ilp.InsertBefore(retunLabel, ilp.Create(OpCodes.Stsfld, fieldref));

			// load default return value 
			if (useRetType) {
				if (retType.IsValueType) {
					var variable = new VariableDefinition(retType);
					m.Body.Variables.Add(variable);
					ilp.InsertBefore(retunLabel, ilp.Create(OpCodes.Ldloca, variable));
					ilp.InsertBefore(retunLabel, ilp.Create(OpCodes.Initobj, retType));
					ilp.InsertBefore(retunLabel, ilp.Create(OpCodes.Ldloc, variable));
				} else {
					ilp.InsertBefore(retunLabel, ilp.Create(OpCodes.Ldnull));
				}
                // fall through to return label
                
            }

			var handler = new ExceptionHandler(ExceptionHandlerType.Catch) {
				TryStart = insts.First(),
				TryEnd = catchLabel,
				HandlerStart = catchLabel,
				HandlerEnd = retunLabel,
				CatchType = m_resolver.GetTypeReference("EException")
			};
			m.Body.ExceptionHandlers.Add(handler);			
		}
	}
}
