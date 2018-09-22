using ECSharp.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESharp.Task;
using ESharp.Annotations;

namespace ECSharp.Net
{
    public struct EUdpReceiveResult 
    {
        byte[] m_buffer;
   //     public EUdpReceiveResult()
   //     {
			//m_buffer = null;
   //     }

        static public EUdpReceiveResult FormBytes(byte[] buffer) {
            var res = new EUdpReceiveResult();
            res.m_buffer = buffer;
            return res;
        }

        public byte[] get_Buffer()
        {
            return m_buffer;
        }
    }

    [CustomSourceFile("EUdpClient.c")]
    public class EUdpClient
    {
        ETask_obj m_receiveResult;
        int socket;

        public EUdpClient()
        {
            m_receiveResult = null;
            socket = 0;
        }

        [ExternC]
        public void Connect(string destination, int port)
        {
            Console.WriteLine(socket);
        }

        [ExternC]
        ETask_obj SendAsync(byte[] data, int bytes)
        {
            return null;
        }
        
        [ExternC]
        ETask_obj ReceiveAsync()
        {
            return null;
        }
    }
}
