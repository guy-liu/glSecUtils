using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace CustomRunspace
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                //Console.WriteLine("CustomRunspace.exe \"$ExecutionContext.SessionState.LanguageMode | Out-File -FilePath C:\\Temp\\test.txt\"");
                return;
            }

            Runspace rs = RunspaceFactory.CreateRunspace();
            rs.Open();

            PowerShell ps = PowerShell.Create();
            ps.Runspace = rs;

            ps.AddScript(args[0]);
            ps.Invoke();
            rs.Close();
        }
    }
}
