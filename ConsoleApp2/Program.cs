using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ConsoleApp2
{
    internal class Program
    {
        private const string MutexName = "Global\\ThPanelCloserSingleRun";
        private static readonly string appPath = Path.Combine(AppContext.BaseDirectory, @"D:\Thunder Master\ThPanel");
        private static readonly string[] TargetProcesses = { "ThPanel", "GCC" };
        private static int _startupCleanupDone;
        private static int _wakeUpCleanupDone;

        static void Main(string[] args)
        {
            using (var mutex = new Mutex(false, MutexName))
            {
                Console.WriteLine("Monitoring startup and wake up events...");

                if (!mutex.WaitOne(0))
                {
                    return;
                }

                SystemEvents.PowerModeChanged += (obj, eventArgs) => 
                {
                    if (eventArgs.Mode == PowerModes.Resume)         
                    {
                        if (Interlocked.Exchange(ref _wakeUpCleanupDone, 1) == 0)   // ----------- выход из сна
                        {
                            RunCleanup("System wake‑up");
                        }
                    }
                };

                if (Interlocked.Exchange(ref _startupCleanupDone, 1) == 0)          // ----------- при старте 
                {
                    RunCleanup("Startup system");
                }
                Console.ReadLine();
            }
        }

        private static void RunCleanup(string state)
        {
            Process[] processes = Process.GetProcessesByName(processName: "ThPanel");
            var timer = new System.Timers.Timer(5000);
            timer.AutoReset = false;
            if (processes.Length == 0)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = appPath,
                        UseShellExecute = true,
                        Verb = "runas"
                    });
                    Console.WriteLine($"[{state}] Started ThPanel.");

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{state}] Failed to start ThPanel: {ex.Message}");
                    return;
                }
            }
            timer.Start();
            timer.Elapsed += (obj, eventArgs) =>
            {
                foreach (var processName in TargetProcesses)
                {
                    foreach (var process in Process.GetProcessesByName(processName))
                    {
                        try
                        {
                            process.Kill();
                            Console.WriteLine($"[{state}] Closed: [{processName}]");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[{state}] Unable to close [{processName}] \n\rError Message: {ex.Message}");
                        }
                    }
                }
                Environment.Exit(0);
            };
        }
    }
}