using ECSharp.Task;
using ECSharp.Tasks;
using ESharp;
//using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ContextType = System.Byte;

namespace ESharp.Task
{
    public interface IAsyncStateMachine
    {
        void MoveNext();
    }

    public struct EAsyncTaskMethodBuilder_obj 
    {
		ETask_obj m_task;		

		public ETask_obj get_Task()
		{
			return m_task;
		}

		public void AwaitUnsafeOnCompleted(ref TaskAwaiter_obj awaiter, ref IAsyncStateMachine statemachine)
		{
			awaiter.UnsafeOnCompleted(statemachine.MoveNext);						
		}


		public void Start(ref IAsyncStateMachine stateMachine)
        {
            // do the first bit of work syncronously
            stateMachine.MoveNext();
        }

        static public EAsyncTaskMethodBuilder_obj Create()
        {
            var builder = new EAsyncTaskMethodBuilder_obj();
			builder.m_task = ETask_obj.Create();
            // capture current sync context
            builder.m_task.m_syncContext = ESyncronizationContext.Current;
            return builder;
        }

        public void SetResult(object result)
		{
            m_task.m_result = result;
			m_task.m_state = (byte)State.Completed;
            call_continuation();
        }

		public void SetResult()
		{
			SetResult(null);
		}

		public void SetException(EException exception)
        {
			m_task.m_exception = exception;
			m_task.m_state = (byte)State.Faulted;
            call_continuation();
        }

        void call_continuation()
        {
            if (m_task.m_continuation == null)
            {
                return;
            }

            if (m_task.m_syncContext != null)
            {
				m_task.m_syncContext.Post(m_task.m_continuation);
            }
            else
            {
				m_task.m_continuation();
            }
        }


    }    

    public class ESyncronizationContext
    {
        static ESyncronizationContext m_currentContext;

        public virtual void Post(ECSharp.Action d)
        {
            Scheduler.Dispatch(d);
        }        

        static public ESyncronizationContext Current
        {
            get
            {
                if (m_currentContext == null)
                {
                    m_currentContext = new ESyncronizationContext();
                }
                // todo return null if this is called on a interrupt
                return m_currentContext;
            }

        }
    }

    public enum State
    {
        Started, Running, Completed, Faulted
    }

    public class ETask_obj : ESharp.Task.INotifyCompletion
    {                
        internal ESyncronizationContext m_syncContext;

		internal byte m_state;

		// the following can be a union
		internal EException m_exception;
        internal object m_result;	
		internal ECSharp.Action m_continuation;       

        public void SetResult(object obj)
        {
            m_result = obj;
            m_state = (byte)State.Completed;
            call_continuation();
        }

        public object GetResult()
        {
            // todo throw exceptions
            return m_result;
        }

        protected void SetException(EException e)
        {
            m_exception = e;
            m_state = (byte)State.Faulted;
            call_continuation();
        }

        void call_continuation()
        {
            if (m_continuation == null)
            {
                return;
            }

            if (m_syncContext != null)
            {
                m_syncContext.Post(m_continuation);
            }
            else
            {
                m_continuation();
            }

            // cleanup to avoid memory leaks
            m_continuation = null;
        }

        public void OnCompleted(ECSharp.Action continuation)
        {
            if (m_state == (byte)State.Faulted
                || m_state == (byte)State.Completed)
            {
                continuation();
            }
            else
            {
                m_continuation = continuation;
            }
        }


		public bool get_IsCompleted()
        {
            return m_state == (byte)State.Completed;
        }        

        public ETask_obj ConfigureAwait(bool useContext)
        {
            if (!useContext)
            {
                m_syncContext = null;
            }

            return this;
        }

        public TaskAwaiter_obj GetAwaiter()
        {
            TaskAwaiter_obj awaiter;
            awaiter.m_task = this;
            return awaiter;
        }

        // Method Builder 
        public static ETask_obj Create()
        {
            return new ETask_obj();
        }

        public ETask_obj get_Task()
        {
            return this;
        }

        public void Start(IAsyncStateMachine stateMachine)
        {
            stateMachine.MoveNext();
        }

        // AsyncMethodBuilder
        public void AwaitOnCompleted(INotifyCompletion awaiter, IAsyncStateMachine stateMachine)
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        public void AwaitUnsafeOnCompleted(INotifyCompletion awaiter, IAsyncStateMachine stateMachine)
        {
            AwaitOnCompleted(awaiter, stateMachine);
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            // ingnore for now
        }

    }  

    public class ETask : ETask_obj, INotifyCompletion
    {

        static ETask Delay(int ms)
        {
            var task = new ETask();
            var timer = new SingleShotTimer((uint)ms, () => { task.SetResult(); });
            return task;
        }

        public new TaskAwaiter GetAwaiter()
        {
			TaskAwaiter awaiter;
			awaiter.m_task = this;
			return awaiter;
		}
        public bool get_IsCompleted()
        {
            return base.get_IsCompleted();
        }

        // Method Builder 
        public static new ETask Create()
        {
            return new ETask();
        }

        public ETask get_Task()
        {
            return this;
        }

        public void SetResult()
        {
            SetResult(null);
        }

        void SetException(EException exception)
        {
            base.SetException(exception);
        }

        void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            base.SetStateMachine(stateMachine);
        }

        public void Start(IAsyncStateMachine stateMachine)
        {
            base.Start(stateMachine);
        }


        // TaskAwaiter
        public void GetResult()
        {
            base.GetResult();
        }

        public void OnCompleted(ECSharp.Action continuation)
        {
            base.OnCompleted(continuation);
        }

        void AwaitUnsafeOnCompleted(INotifyCompletion awaiter, IAsyncStateMachine stateMachine)
        {
            base.AwaitUnsafeOnCompleted(awaiter, stateMachine);
        }
    }

    public interface INotifyCompletion
    {
        void OnCompleted(ECSharp.Action continuation);
    } 

    public struct TaskAwaiter_obj
    {
        public ETask_obj m_task;

        public void UnsafeOnCompleted(ECSharp.Action continuation)
        {
            m_task.OnCompleted(continuation);
        }

		public void OnCompleted(ECSharp.Action continuation)
		{
			UnsafeOnCompleted(continuation);
		}
		public bool get_IsCompleted()
        { 
            return m_task.get_IsCompleted(); 
        }
        public object GetResult()
        {
            return m_task.GetResult();
        }
	}

	public struct TaskAwaiter_generic<T>
	{
		public ETask_obj m_task;

		public void UnsafeOnCompleted(ECSharp.Action continuation)
		{
			m_task.OnCompleted(continuation);
		}

		public void OnCompleted(ECSharp.Action continuation)
		{
			UnsafeOnCompleted(continuation);
		}
		public bool get_IsCompleted()
		{
			return m_task.get_IsCompleted();
		}		

		public T GetResult()
		{
			return (T)m_task.GetResult();
		}

	}


	public struct TaskAwaiter
	{
		public ETask m_task;

		public void UnsafeOnCompleted(ECSharp.Action continuation)
		{
			m_task.OnCompleted(continuation);
		}

		public void OnCompleted(ECSharp.Action continuation)
		{
			UnsafeOnCompleted(continuation);
		}
		public bool get_IsCompleted()
		{
			return m_task.get_IsCompleted();
		}
		public void GetResult()
		{
			m_task.GetResult();
		}

	}

	public class ValueType
    {

    }
}
