using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace klinqers
{
    class MainClass
    {
        class CallMethod
        {
            public MethodInfo minf = null;
            public Type type = null;
            public Assembly assembly = null;
            public int methodPosition = -1;
            public string strMethodPart;
            public string strTypePart;
        }



        public static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--assemblies")
                {
                    Console.WriteLine("[+] Creating list for all assemblies in appdomain");

                    System.Diagnostics.Process p = new System.Diagnostics.Process();
                    //string strAssembly = p.GetType().Assembly.FullName;

                   
                    printAssemblies();
                }
                else if ((i + 1) < args.Length)
                {
                    if (args[i] == "--type")
                    {
                        string strType = args[++i];

                        Console.WriteLine("[+] Creating list for " + strType);

                        Type type = Type.GetType(strType);

                        printMethodsForType(type);
                    }
                    else if (args[i] == "--call")
                    {
                        string strCall = args[++i];

                        Console.WriteLine("[+] Creating call for " + strCall);


                    }

                }
            }
        }



        static string getCall(string strNetCall)
        {
            string strCompiled = string.Empty;

            Match m = Regex.Match(strNetCall.Trim(), "\\s{0,}(?<call>[A-z|\\.|0-9|\\-|_]{1,})\\s{0,}\\((?<parameters>.+)\\)\\s{0,}");

            if(m.Success)
            {
                string strCall = m.Groups["call"].Value.Trim();
                string strParameters = m.Groups["parameters"].Value.Trim();

                List<string> lstValues = new List<string>();
                Type parameterType = typeof(String);


                if (strParameters.IndexOf('(') == 0) // We have a different type than string or int
                {
                }
                else if (strParameters.IndexOf('"') == 0) // We have a string
                {
                    Match mStrings = Regex.Match(strParameters, "\\\"(?<input>[^\"]{0,})\\\"");

                    foreach(Capture c in mStrings.Captures)
                    {
                        lstValues.Add(c.Value);
                    }
                }

                CallMethod cm = getMethodPosition(strNetCall, lstValues.Count, parameterType, null);

                if(cm.methodPosition >= 0)
                {

                }

            }

            return strCompiled;
        }

        static string findSuitableSplitter(List<string> lstValues)
        {
            // We rather want a distinct non letter divider since it is easier to read 
            List <string> strCandidates = new List <string>() { ";",",","|","#","/","\\","'","!","?",":",".","$","=","-","+","*","_","<",">","@","{","}","[","]" };
            char startChar = 'a';
            char endChar = 'z';
            char startCharCaps = 'A';
            char endCharCaps = 'Z';


            for(char x=startChar; x < endChar; x++)
            {
                strCandidates.Add(x.ToString());
            }

            for (char x = startCharCaps; x < endCharCaps; x++)
            {
                strCandidates.Add(x.ToString());
            }

            for (int x = 0; x < strCandidates.Count; x++)
            {
                bool bFound = true;

                // Check preferred
                for (int i = 0; i < lstValues.Count; i++)
                {
                    string str = lstValues[i];

                    if(str.IndexOf(strCandidates[x]) >= 0)
                    {
                        bFound = false;
                        break;
                    }
                }

                if(bFound)
                {
                    return strCandidates[x];
                }
            }

            return null;
        }

        static string buildParameterString(List<string> lstValues, Type type)
        {
            string strSplitter = findSuitableSplitter(lstValues);

            if(strSplitter == null)
            {
                Console.WriteLine("[-] Error! Could not find a suitable splitter");

                return null;
            }

            if (type == typeof(String))
            {
                string strArray = String.Join(strSplitter, lstValues);

                string strRetval = "\"" + strArray + "\".Split(\"" + strSplitter + "\".ToCharArray()";

                return strRetval;

                //"ping;213.115.26.222".Split(";".ToCharArray()))
            }

            return null;
        }



        static CallMethod getMethodPosition(string strMethodName, int parameterCount, System.Type callType, string strAssemblyPath)
        {
            string strType = strMethodName.Substring(0, strMethodName.LastIndexOf('.'));
            Assembly a = Assembly.GetExecutingAssembly();
            Type t = a.GetType(strType);
            string strMethodPart = strMethodName.Substring(strMethodName.LastIndexOf('.'));
            MethodInfo [] minf = t.GetMethods();

            CallMethod callMethod = new CallMethod()
            {
                assembly = a,
                type = t,
                strMethodPart = strMethodPart,
                strTypePart = strType
            };

            for(int x=0; x < minf.Length; x++)
            {
                MethodInfo m = minf[x];

                if(m.Name == strMethodPart)
                {
                    ParameterInfo[] p = m.GetParameters();

                    if (p.Length != parameterCount) continue;

                    bool bFoundCall = true;

                    for(int i=0; i < p.Length; i++)
                    {
                        if(p[i].GetType() != callType)
                        {
                            bFoundCall = false;
                            break;
                        }
                    }

                    if(bFoundCall)
                    {
                        callMethod.methodPosition = x;
                        break;
                    }
                }
            }

            return callMethod;
        }

        static void printAssemblies()
        {
            Assembly [] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            string strCurrent = Assembly.GetExecutingAssembly().FullName;

            Console.WriteLine("[+] Assembly count: " + assemblies.Length);

            foreach(Assembly a in assemblies)
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

        static bool workableParameters(ParameterInfo [] pinfos, HashSet <string> lstTypesWeCanHandle)
        {
            if (pinfos == null || pinfos.Length < 1) return true; // Empty parameter-sets always work.
            Type parameterType = null;

            for(int i =0; i < pinfos.Length; i++)
            {
                Type p = pinfos[i].ParameterType;

                if (p == null) return false;

                if(i == 0)
                {
                    parameterType = p;
                    if (!lstTypesWeCanHandle.Contains(p.ToString()))
                    {
                        return false;
                    }
                }
                else if(p.ToString() != parameterType.ToString())
                {
                    return false;
                }
            }

            return true;
        }



        static void printMethodsForType(Type type)
        {
            if (!type.IsClass) return;
                       
            MethodInfo [] minf = type.GetMethods();
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


            for (int i=0; i < minf.Length; i++)
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

                        Console.WriteLine(minf[i].CallingConvention.ToString() + " - " +(minf[i].IsStatic ? "static " : "") + minf[i].ReturnType.ToString() + " " + type.Namespace + "." + type.Name + "." + minf[i].Name + "(" + strParameters + ")   <" + i + ">");
                    }
                }

            }
        }
    }
}
