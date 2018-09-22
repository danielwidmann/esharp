using ECSharp.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ESharp.Annotations;

namespace ECSharp.Tasks
{

    public static class ScheduleHelper
    {
        static void ScheduleCallback(StringCallback c, string s)
        {
            Scheduler.Dispatch(() => c(null, s));
        }
    }

    
    
    [CustomSourceFile("scheduler.c")]
    [CustomHeaderFile("scheduler.h")]
    [Uses(typeof(Fifo))]
    public static class Scheduler
    {
        // reference to inculde c file
        //static Fifo fifo;
        
        //[Skip]
        //static Queue<Action> m_pendingTasks = new Queue<Action>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        [ExternC]
        static public void Dispatch(Action t)
        {
            //if(t == null)
            //{
            //    throw new Exception("Task cannot be null");
            //}
            //m_pendingTasks.Enqueue(t);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        [ExternC]
        static public void ProcessLoop()
        {
            //if (m_pendingTasks.Count > 0)
            //{
            //    var task = m_pendingTasks.Dequeue();

            //    task.Invoke();
            //}
        }


        public static void Stop()
        {

        }
        public static void MainLoop()
        {
            //while(true)
            //{
            //    ProcessLoop();
            //}

        }
    }
}
