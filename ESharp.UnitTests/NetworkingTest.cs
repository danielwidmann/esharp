﻿using ECSharp.Tasks;
using ESharp.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Ecs.Test.ETests
{
    [TestClass]
    [ETestFixture]
    public class NetworkingTest
    {

        async Task test()
        {
            var socket = new UdpClient();
            socket.Connect("8.8.8.8", 53);
            //var r = new byte[] { 0x64, 0x66, 0xb3, 0x34, 0x2c, 0xb6, 0xf8, 0x16, 0x54, 0xec, 0x0d, 0x51, 0x08, 0x00, 0x45, 0x00, 0x00, 0x36, 0x69, 0x5d, 0x00, 0x00, 0x80, 0x11, 0x4d, 0x0f, 0xc0, 0xa8, 0x01, 0xf9, 0xc0, 0xa8, 0x01, 0x01, 0xc3, 0x48, 0x00, 0x35, 0x00, 0x22, 0x71, 0x41, 0x00, 0x02, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x68, 0x6, 0x56, 0x97, 0x36, 0x50, 0x26, 0x46, 0x50, 0x00, 0x00, 0x10, 0x00 }; 

            // 0x64, 0x66, 0xb3, 0x34, 0x2c, 0xb6, 0xf8, 0x16, /* df.4,... */
                    //0x54, 0xec, 0x0d, 0x51, 0x08, 0x00, 0x45, 0x00, /* T..Q..E. */
                    //0x00, 0x36, 0x5e, 0x1e, 0x00, 0x00, 0x80, 0x11, /* .6^..... */
                    //0x09, 0xe8, 0xc0, 0xa8, 0x01, 0xf9, 0x08, 0x08, /* ........ */
                    //0x08, 0x08, 0xf4, 0x06, 0x00, 0x35, 0x00, 0x22, /* .....5." */
                    //0xf2, 0x1c,

            var r = new byte[] {
                    0x00, 0x02, 0x01, 0x00, 0x00, 0x01, /* ........ */
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x68, /* .......h */
                    0x65, 0x69, 0x73, 0x65, 0x02, 0x64, 0x65, 0x00, /* eise.de. */
                    0x00, 0x01, 0x00, 0x01                          /* .... */
                    };

            
            await socket.SendAsync(r, r.Length);
            UdpReceiveResult res = await socket.ReceiveAsync();
            Console.WriteLine("received :");
            Console.WriteLine(res.Buffer.Length);
        }
        [TestMethod]
	    //[Ignore]
		public void TestUdp()
        {
            
            var task = test();
            //task.Wait();

            
            //task.GetAwaiter().OnCompleted(() =>
            //{
            //    Console.WriteLine("Done");
            //    //Scheduler.Stop();
            //});

            //Scheduler.MainLoop();
            while (!task.IsCompleted)
            {
                Scheduler.ProcessLoop();
            }


        }
    }
}
