using JobManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Es.Helper
{
    public class BadReturnCodeExcpetion: Exception
    {
        public int Code;

        public BadReturnCodeExcpetion(string msg, int code): base(msg)
        {
            Code = code;
        }
    }

    public class ProcessRunner
    {

        public static int Run(string exe, string args, string workingDir)
        {
            string st;
            return Run(exe, args, (s) => Console.Write(s), workingDir, false, out st);
        }

		public static void RunWithException(string exe, string args, string workingDir, bool verbose = true)
		{
			string res = "";
			int returnCode;

			if (verbose) {
				returnCode = Run(exe, args, workingDir);
			} else {
				returnCode = RunAndStoreOutput(exe, args, out res, workingDir);
			}

			if (returnCode != 0) {
					throw new BadReturnCodeExcpetion("Error Running Command: \n" + res, returnCode);
			}
		}

        public static int RunAndStoreOutput(string exe, string args, out string result, string workingDir)
        {
            var sb = new StringBuilder();
            int exitCode = Run(exe, args, (s) => sb.Append(s), workingDir, true, out result);

            result = sb.ToString();
            return exitCode;
        }

        public static int Run(string exe, string args, Action<string> newData, string workingDir, bool store, out string result)
        {
            //Logger.Info("Run: " + exe + " " + args);

            result = null;

            var proc = new Process();
            proc.StartInfo.FileName = exe;
            proc.StartInfo.Arguments = args;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            if (workingDir != null)
            {
                proc.StartInfo.WorkingDirectory = workingDir;
            }

            if (true)
            {
                proc.EnableRaisingEvents = true;

                // You can pass any delegate that matches the appropriate 
                // signature to ErrorDataReceived and OutputDataReceived
                proc.ErrorDataReceived += (sender, errorLine) =>
                {
                    if (errorLine.Data != null)
                        newData(errorLine.Data + "\r\n");
                    if (errorLine.Data == "")
                    {
                        newData("\r\n");
                    }
                };

                proc.OutputDataReceived += (sender, outputLine) =>
                {

                    if (outputLine.Data != null)
                    {
                        //                    proc.StandardOutput.ReadLine()
                        newData(outputLine.Data + "\r\n");// 
                        //newData("\r\n");
                    }
                    if (outputLine.Data == "")
                    {
                        newData("\r\n");
                    }
                };


            }



            var j = new Job();

            // Do something here

            proc.Start();
            j.AddProcess(proc.Handle);

            if (true)
            {
                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();
            }


            if (false)
            {
                bool stopped = false;
                //var li = new List<String>();
                var t = new Thread(() =>
                {
                    while (!stopped)
                    {
                        var l = proc.StandardOutput.ReadLine();
                        if (l == null)
                        { continue; }
                        //li.Add(l);
                        newData(l + "\r\n");

                        if (l == "")
                        {
                            newData("\r\n");
                        }
                    }
                    var remainder = proc.StandardOutput.ReadToEnd();
                    //li.Add(remainder);
                    newData(remainder);

                });
                t.Start();

            }

            proc.WaitForExit();

            var exitCode = proc.ExitCode;
            proc.Close();

            return exitCode;
        }

    }
}
