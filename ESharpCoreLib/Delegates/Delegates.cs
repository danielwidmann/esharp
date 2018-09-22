using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ECSharp.Tasks;

namespace ECSharp
{
    public delegate void Action();

    public delegate void Callback(object error);
    public delegate void StringCallback(object error, string result);
    public delegate void Action_String(String t);

    public delegate void AsyncFunc(Callback callback);
}
