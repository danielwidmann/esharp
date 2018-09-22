using ECSharp.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSharp.Task
{
    public delegate Task AsyncFunction();
    public delegate void Action_Task(Task task);
    public delegate Task Func_Task_Task(Task task);
    public delegate Task AsyncFunc();


    public class Task
    {   
        public static Task GetCompletedTask()
        {
            var task = new Task();
            task.Complete();
            return task;
        }

        public bool IsCompleted = false;
        public bool IsFaulted;       
        Action_Task m_continuation;
 

        public void Complete()
        {
            IsCompleted = true;
            if (m_continuation != null)
            {
                Scheduler.Dispatch(Continue);
            }
        }

        private void Continue()
        {
            m_continuation(this);
        }

        public Task ContinueWith(Func_Task_Task continuationAction)
        {
            Task task = new Task();
            m_continuation = (t) =>
            {
                var newtask = continuationAction(t);
                newtask.ContinueWith(x=>task.Complete());
            };

            if (IsCompleted)
            { // continuation can start directly
                Scheduler.Dispatch(Continue);
            }

            return task;
        }
        public Task ContinueWith(Action_Task continuationAction)
        {
            var task = new Task();
            
            m_continuation = (t)=> 
                {
                    continuationAction(task);
                    task.Complete();
                };

            if (IsCompleted)
            { // continuation can start directly
                Scheduler.Dispatch(Continue);
            }

            return task;
        }

        public static Task Delay(uint count)
        {
            var t = new Task();

            var delayTimer = new SingleShotTimer(count, () => { t.Complete(); });

            return t;
        }

    }

    public class PeriodicTimer
    {
        uint m_lastRun = 0;
        uint m_period;
        AsyncFunc m_taskToRun;

        public PeriodicTimer(uint periodInTicks, Action actionToRun)
        {
            SetupTimer(periodInTicks, () =>
            {
                actionToRun();
                return Task.GetCompletedTask();
            });
        }

        public PeriodicTimer(uint periodInTicks, AsyncFunc actionToRun)
        {
            SetupTimer(periodInTicks, actionToRun);
        }

        private void SetupTimer(uint periodInTicks, AsyncFunc actionToRun)
        {
            m_period = periodInTicks;
            m_taskToRun = actionToRun;

            Schedule();
        }

        void Loop()
        {
            uint diff = Target.GetTickCount() - m_lastRun;
            if (diff >= m_period)
            {
                m_lastRun = Target.GetTickCount();
                m_taskToRun()
                    .ContinueWith(x => Schedule());
            }
            else
            {
                Schedule();
            }
        }

        void Schedule()
        {
            Scheduler.Dispatch(Loop);
        }

        void Reschedule()
        {
            m_lastRun = Target.GetTickCount();
            Schedule();
        }
    }

    public class SingleShotTimer
    {
        uint m_lastRun = 0;
        uint m_period;
        Action m_taskToRun;

        public SingleShotTimer(uint ticks, Action actionToRun)
        {
            m_period = ticks;
            m_taskToRun = actionToRun;
            m_lastRun = Target.GetTickCount();
            Schedule();
        }

        void Loop()
        {
            uint diff = Target.GetTickCount() - m_lastRun;
            if (diff >= m_period)
            {
                m_taskToRun();
            }
            else
            {
                Schedule();
            }
        }

        void Schedule()
        {
            Scheduler.Dispatch(Loop);
        }

        void Reschedule()
        {
            m_lastRun = Target.GetTickCount();
            Schedule();
        }
    }
}
