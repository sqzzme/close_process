using System;
using System.IO;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security.Principal;

namespace ConsoleApp2
{
    internal class Program
    {
        private static readonly string appPath = Path.Combine(AppContext.BaseDirectory, @"D:\Thunder Master\ThPanel");
        Process[] processes = Process.GetProcessesByName(processName: "ThPanel");
        void StartAutoClose(int seconds)
        {
           
            if (processes.Length == 0)
            {
                 Console.WriteLine("Not running");
                 Process.Start(new ProcessStartInfo
                 {
                     FileName = appPath,
                     UseShellExecute = true,
                     Verb = "runas"
                 });
            }
            else
            {
                if (processes.Length != 0)
                {
                    Console.WriteLine("Running");
                }
                Console.WriteLine("Auto closing the application after {0} seconds.", seconds);
            }
        }

        static void Main(string[] args)
        {
            Program program = new Program();
            program.StartAutoClose(10);

            string[] names = { "ThPanel", "GCC" };
            var nameProc = names.SelectMany(proc => Process.GetProcessesByName(proc)).ToArray();

            var timer = new System.Timers.Timer(5000);
                    timer.Start();
                    timer.Elapsed += (sender, eventArgs) =>
                    {
                        foreach (Process processes in nameProc)
                        {
                            processes.Kill();

                            try
                            {
                                if (processes.HasExited)
                                {
                                    Console.WriteLine("Process closed successfully.");
                                }
                                else
                                {
                                    Console.WriteLine("Failed to close the process.");
                                }
                            }

                            catch (System.ComponentModel.Win32Exception ex)
                            {
                                Console.WriteLine("Cannot inspect the process. Run this application as administrator. " + ex.Message);
                            }
                            timer.Stop();
                        }

                        Environment.Exit(0);
                    };
                    Console.ReadLine();
        }
    }
}
