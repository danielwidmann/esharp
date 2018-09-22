using ECSharp.Tasks;
using ESharp.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Ecs.Test.ETests
{
	[TestClass]
	[ETestFixture]
	public class AsyncTest
	{
		//public int ADD(int a)
		//{
		//    a += 1;
		//    return a;
		//}

		//public void ADD(ref int a)
		//{
		//    a += 1;
		//}


		public async Task<object> DoAsync()
		{
			Console.WriteLine("Start Async");
			for (int i = 0; i < 10; i++) {
				await Task.Delay(100);
				Console.WriteLine(i);
			}
			Console.WriteLine("Done Async");
			return null;
		}

		//     public async Task<object> DoAsyncWithParam(int p)
		//     {
		//         Console.WriteLine("Start Async");
		//         for (int i = 0; i < 10; i++)
		//         {
		//             await Task.Delay(100);
		//             Console.WriteLine(i + p);
		//         }
		//         Console.WriteLine("Done Async");
		//Console.WriteLine(p);
		//         return null;
		//     }

		[TestMethod]
		public void SimpleAsyncTest()
		{

			var task = DoAsync();
			task.GetAwaiter().OnCompleted(() => {
				Console.WriteLine("Done");
				Scheduler.Stop();
			});

			Scheduler.MainLoop();

		}

	}


	class StateMachine : IAsyncStateMachine
	{
		public AsyncTaskMethodBuilder builder;

		public int state = 0;

		public Task t = new Task(()=>{});

		public void MoveNext()
		{
			if(state == 0) {

				state = 1;
				var awaiter = t.GetAwaiter();
				var _me = this;
				builder.AwaitUnsafeOnCompleted(ref awaiter, ref _me);
				return;
			} else {
				state = 2;
				builder.SetResult();
			}

		}

		public void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			
		}
	}
	[TestClass]
	public class TaskTest
	{
		[TestMethod]
		public void SimpleTaskTest()
		{
			var sm = new StateMachine();			
			sm.builder = AsyncTaskMethodBuilder.Create();
			sm.builder.Start(ref sm);
			var task = sm.builder.Task;

			
			Assert.IsTrue(sm.state == 1);
			Assert.IsTrue(task.IsCompleted == false);

			sm.t.RunSynchronously();
					
			Debug.Assert(sm.state == 2);
			Assert.IsTrue(task.IsCompleted == true);
		}

	}


	class EStateMachine : ESharp.Task.IAsyncStateMachine
	{
		public ESharp.Task.EAsyncTaskMethodBuilder_obj builder;

		public int state = 0;

		public ESharp.Task.ETask_obj m_task = new ESharp.Task.ETask_obj();

		public void MoveNext()
		{
			if (state == 0) {

				state = 1;
				var awaiter = m_task.GetAwaiter();
				var _me = (ESharp.Task.IAsyncStateMachine)this;
				builder.AwaitUnsafeOnCompleted(ref awaiter, ref _me);
				return;
			} else {
				state = 2;
				builder.SetResult(null);
			}

		}

		public void SetStateMachine(IAsyncStateMachine stateMachine)
		{

		}
	}
	[TestClass]
	[ETestFixture]
	public class ETaskTest
	{
		[TestMethod]
		public void SimpleTaskTest()
		{
			var sm = new EStateMachine();
			sm.builder = ESharp.Task.EAsyncTaskMethodBuilder_obj.Create();
			var intf = (ESharp.Task.IAsyncStateMachine)sm;
			sm.builder.Start(ref intf);
			var task = sm.builder.get_Task();


			Assert.IsTrue(sm.state == 1);
			Assert.IsTrue(task.get_IsCompleted() == false);

			sm.m_task.SetResult(null);

			Assert.IsTrue(sm.state == 2);
			Assert.IsTrue(task.get_IsCompleted() == true);
		}

	}
}
