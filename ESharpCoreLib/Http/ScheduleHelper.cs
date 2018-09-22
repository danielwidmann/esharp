using ECSharp.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSharp.Http
{
    public class ScheduleHelper
    {
        static public void ScheduleActionString(Action_String a, string s)
        {
            Scheduler.Dispatch(()=>a(s));
        }
    }
}
