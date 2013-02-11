using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace IW4MCACPatcher
{
    class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(
          IntPtr hProcess,
          IntPtr lpBaseAddress,
          [Out] byte[] lpBuffer,
          int dwSize,
          out int lpNumberOfBytesRead
         );

        [DllImport("kernel32.dll")]
        static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress,
           UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("-----------------------------------");
                Console.WriteLine("      IW4M Ingame CAC Patcher      ");
                Console.WriteLine("             by zxz0O0             ");
                Console.WriteLine("-----------------------------------\n");

                Console.WriteLine("Trying to start LaunchIW4M.exe...");

                string path = string.Empty;
                if (File.Exists("LaunchIW4M.exe"))
                    path = "LaunchIW4M.exe";
                else if (args.Length > 0 && args[0].Length > 0)
                {
                    if (File.Exists(args[0]))
                        path = args[0];
                    else
                    {
                        Console.WriteLine("Can not find " + args[0]);
                        Console.ReadKey();
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("Can not find LaunchIW4M.exe");
                    Console.ReadKey();
                    return;
                }

                using (Process proc = Process.Start(path))
                {
                    Console.WriteLine("LaunchIW4M.exe started successfully");
                    Console.WriteLine("Waiting for iw4m.exe to start...");
                    if (!(args.Length > 1 && args[1] == "0"))
                        proc.WaitForExit();
                }
                //Wait for iw4m to start
                System.Threading.Thread.Sleep(4000);

                Process[] iw4mproc = Process.GetProcessesByName("iw4m");
                if (iw4mproc.Length < 1) iw4mproc = Process.GetProcessesByName("iw4m.dat");

                switch (iw4mproc.Length)
                {
                    case 0:
                        Console.WriteLine("IW4M Process not found!");
                        Console.ReadKey();
                        return;
                    case 1:
                        Console.WriteLine("IW4M Process found");
                        break;
                    default:
                        Console.WriteLine("Multiple processes for IW4M found!");
                        Console.WriteLine("Using first process");
                        break;
                }

                uint outBuf;
                VirtualProtectEx(iw4mproc[0].Handle, (IntPtr)0x4351C9, (UIntPtr)3, (uint)0x40, out outBuf);

                int outBuf2;
                byte[] readBuf = new byte[] { 0, 0, 0 };
                ReadProcessMemory(iw4mproc[0].Handle, (IntPtr)0x4351C9, readBuf, 3, out outBuf2);
                if (readBuf[0] == 0xF && readBuf[1] == 0x9C)
                {
                    Console.WriteLine("Patching IW4M...");
                    readBuf = new byte[] { 0xB0, 0x01, 0x90 };
                    WriteProcessMemory(iw4mproc[0].Handle, (IntPtr)0x4351C9, readBuf, 3, out outBuf2);
                    Console.WriteLine("Patched successfully. Have fun!");
                }
                else
                    Console.WriteLine("Error reading memory. Are you using a different version?");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
                Console.ReadKey();
            }
        }
    }
}
