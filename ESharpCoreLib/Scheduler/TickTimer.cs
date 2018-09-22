using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESharp.Annotations;

namespace ECSharp.Tasks
{

    interface ITickProvider
    {
        int GetTicks();
    }

    class Target
    {
        [ExternC]
        static public uint GetTickCount() { return (uint)Environment.TickCount; } 
    }

    public class TickTimer
    {
        uint m_lastRun = 0;
        uint m_period;
        AsyncFunc m_taskToRun;
       
        public TickTimer(uint ticks, AsyncFunc actionToRun)
        {
            m_period = ticks;
            m_taskToRun = actionToRun;

            Schedule();
        }

        void Loop()
        {
            uint diff = Target.GetTickCount() - m_lastRun;
            if (diff >= m_period)
            {
                m_lastRun = Target.GetTickCount();
                m_taskToRun((e)=>Schedule());
            } else
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
