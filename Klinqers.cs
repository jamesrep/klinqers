using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CSharp;

namespace klinqers
{
    public class Klinqers
    {
        public Klinqers()
        {
        }

        static List <string> splitStrings(string str)
        {
            StringBuilder sb = new StringBuilder();
            char CHR_DIVIDER = ',';

            List<string> lstArray = new List<string>();

            int mode = 0; // 0=normal, 1=escape next

            for (int i = 0; i < str.Length; i++)
            {
                if (mode == 0)
                {
                    if (str[i] == '\\')
                    {
                        mode = 1;
                        continue;
                    }
                    else if(str[i] == CHR_DIVIDER)
                    {
                        lstArray.Add(sb.ToString());
                        sb = new StringBuilder();
                    }
                    else if (i == (str.Length -1))
                    {
                        sb.Append(str[i]);
                        lstArray.Add(sb.ToString());
                    }
                    else
                    {
                        sb.Append(str[i]);
                    }
                }
                else if(mode == 1)
                {
                    sb.Append(str[i]);
                }
            }

            return lstArray;
        }

        public class DummyClass
        {
            public virtual object getResult()
            {
                return null;
            }
        }


        public static void executeCSharp(string strInjection)
        {
#pragma warning disable CS0618
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            ICodeCompiler icc = codeProvider.CreateCompiler();

            AssemblyName[] references = Assembly.GetExecutingAssembly().GetReferencedAssemblies();

            System.CodeDom.Compiler.CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateExecutable = false;
            parameters.GenerateInMemory = true;

            for (int x = 0; x < references.Length; x++)
            {
                parameters.ReferencedAssemblies.Add(references[x].Name + ".dll");
            }

            parameters.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);


            string strSource = "class Dummy1 : klinqers.Klinqers.DummyClass { public override object getResult() { return " + strInjection + " ; }} ";


            CompilerResults results = icc.CompileAssemblyFromSource(parameters, strSource);


            // If compile errors
            for (int x = 0; x < results.Errors.Count; x++)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[-] Error: " + results.Errors[x].ErrorText);
                Console.ResetColor();
            }

            Klinqers.DummyClass obj =  (Klinqers.DummyClass ) results.CompiledAssembly.CreateInstance("Dummy1");


            Console.ForegroundColor = ConsoleColor.Yellow;
            object objResult = obj.getResult();

            Console.ResetColor();

            if (objResult != null)
            {
                Console.WriteLine("[+] Result: " + objResult.ToString());
            }


        }

        public static string getCall(string strNetCall)
        {
            string strCompiled = string.Empty;

            Match m = Regex.Match(strNetCall.Trim(), "\\s{0,}(?<call>[A-z|\\.|0-9|\\-|_]{1,})\\s{0,}\\((?<parameters>.+)\\)\\s{0,}");

            if (m.Success)
            {
                string strCall = m.Groups["call"].Value.Trim();
                string strParameters = m.Groups["parameters"].Value.Trim();

                List<string> lstValues = splitStrings(strParameters);
                Type parameterType = typeof(String);


                if (strParameters.IndexOf('(') == 0) // We have a different type than string or int
                {
                }
                else if (strParameters.IndexOf('"') == 0) // We have a string
                {
                    Match mStrings = Regex.Match(strParameters, "(?<input>[^,]{0,})[,]{0,}");

                    foreach (Capture c in mStrings.Captures)
                    {
                        lstValues.Add(c.Value);
                    }
                }

                string strType = strCall.Substring(0, strCall.LastIndexOf('.'));
                Type t = Type.GetType(strType);

                if (t == null)
                {
                    Console.WriteLine("[+] Ok. The type was not easily created. Lets try CreateInstanceAndUnwrap");

                    Type tTemp = Type.GetType("System.AppDomain");

                    CallMethod cm1 = CallMethod.getMethodPosition(tTemp, "System.AppDomain.CreateInstanceAndUnwrap", 2, typeof(System.String), null);

                    strCompiled = cm1.buildParameterStringByCIAU(lstValues, strCall);

                    return strCompiled;
                }

                CallMethod cm = CallMethod.getMethodPosition(t, strCall, lstValues.Count, parameterType, null);

                if (cm.methodPosition >= 0)
                {
                    strCompiled = cm.buildParameterString(lstValues);
                }
                else
                {
                    Console.WriteLine("[-] Error: Could not find method position for " + strNetCall);
                    return null;
                }

            }

            return strCompiled;
        }





        public static void printAssemblies()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            string strCurrent = Assembly.GetExecutingAssembly().FullName;

            Console.WriteLine("[+] Assembly count: " + assemblies.Length);

            foreach (Assembly a in assemblies)
            {
                string strFullName = a.GetName().FullName;

                if (strCurrent != strFullName)
                {
                    Module[] modules = a.GetModules();

                    foreach (Module m in modules)
                    {
                        Type[] types = m.GetTypes();


                        Console.WriteLine("[+] Assembly: " + strFullName + "\r\n-----------------");

                        foreach (Type t in types)
                        {
                            printMethodsForType(t);
                        }
                    }
                }
            }
        }

        static bool workableParameters(ParameterInfo[] pinfos, HashSet<string> lstTypesWeCanHandle)
        {
            if (pinfos == null || pinfos.Length < 1) return true; // Empty parameter-sets always work.
            Type parameterType = null;

            for (int i = 0; i < pinfos.Length; i++)
            {
                Type p = pinfos[i].ParameterType;

                if (p == null) return false;

                if (i == 0)
                {
                    parameterType = p;
                    if (!lstTypesWeCanHandle.Contains(p.ToString()))
                    {
                        return false;
                    }
                }
                else if (p.ToString() != parameterType.ToString())
                {
                    return false;
                }
            }

            return true;
        }



        public static void printMethodsForType(Type type)
        {
            if (!type.IsClass) return;

            MethodInfo[] minf = type.GetMethods();
            HashSet<string> lst = new HashSet<string>();
            Int32 dummyInt = 1;
            byte dummyByte = 1;
            System.Net.IPAddress ip = new System.Net.IPAddress(0);

            lst.Add(ip.GetType().ToString()); // System.Net.IPAddress[] System.Net.Dns.GetHostAddresses(String hostNameOrAddress)
            lst.Add(" ".GetType().ToString());
            lst.Add(' '.GetType().ToString());
            lst.Add(dummyInt.GetType().ToString()); // System.Int32[] System.Globalization.StringInfo.ParseCombiningCharacters(String str)
            lst.Add(dummyByte.GetType().ToString());    // System.Byte[] Internal.Cryptography.Helpers.DecodeHexString(String s)
                                                        // System.Byte[] System.Convert.FromBase64String(String 


            for (int i = 0; i < minf.Length; i++)
            {
                if (minf[i].IsFamily) continue;
                if (minf[i].CallingConvention.HasFlag(CallingConventions.HasThis)) continue;

                if (minf[i].IsPublic)
                {
                    ParameterInfo[] parameters = minf[i].GetParameters();

                    if (workableParameters(parameters, lst))
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (ParameterInfo pinf in parameters) sb.Append(pinf.ParameterType.Name + " " + pinf.Name + ",");
                        string strParameters = sb.ToString().Trim(new char[] { ',' });

                        Console.WriteLine(minf[i].CallingConvention.ToString() + " - " + (minf[i].IsStatic ? "static " : "") + minf[i].ReturnType.ToString() + " " + type.Namespace + "." + type.Name + "." + minf[i].Name + "(" + strParameters + ")   <" + i + ">");
                    }
                }

            }
        }
    }
}
