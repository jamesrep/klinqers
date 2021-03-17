using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace klinqers
{
    class MainClass
    {




        public static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--assemblies")
                {
                    Console.WriteLine("[+] Creating list for all assemblies in appdomain");

                    System.Diagnostics.Process p = new System.Diagnostics.Process();
                    //string strAssembly = p.GetType().Assembly.FullName;

                   
                    Klinqers.printAssemblies();
                }
                else if ((i + 1) < args.Length)
                {
                    if (args[i] == "--type")
                    {
                        string strType = args[++i];

                        Console.WriteLine("[+] Creating list for " + strType);

                        Type type = Type.GetType(strType);

                        Klinqers.printMethodsForType(type);
                    }
                    else if (args[i] == "--call")
                    {
                        string strCall = args[++i];

                        Console.WriteLine("[+] Creating call for " + strCall);

                        string strCompiled = Klinqers.getCall(strCall);

                        if (strCompiled != null)
                        {
                            Console.WriteLine(strCompiled);
                        }
                    }

                }
            }
        }




    }
}
