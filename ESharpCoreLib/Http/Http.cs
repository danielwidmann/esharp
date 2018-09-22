using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ESharp.Annotations;

namespace ECSharp.Http
{
    [Uses(typeof(ScheduleHelper))]
    [CustomSourceFile("http.c")]
    public static class HttpClient
    {
        
        public static void Request(string server, string path, Action_String result)
        {
            var request = WebRequest.Create(server + path);

            WebResponse response = request.GetResponse();

            var rspStream = new StreamReader(response.GetResponseStream()).ReadToEnd();

            if(result != null)
            { 
                result(rspStream);
            }            
        }
    }
}
