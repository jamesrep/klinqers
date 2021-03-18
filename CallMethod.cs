// James Dickson 2021, License GPL 2.0 (see file in project)
using System;
using System.Collections.Generic;
using System.Reflection;

namespace klinqers
{
    public class CallMethod
    {
        public MethodInfo minf = null;
        public Type type = null;
        public Assembly assembly = null;
        public int methodPosition = -1;
        public string strMethodPart;
        public string strTypePart;
        public Type parameterType = null;
        public bool bRequirePosition = true; // If false then we have no polymorphism for the function

        static string findSuitableSplitter(List<string> lstValues)
        {
            // We rather want a distinct non letter divider since it is easier to read 
            List<string> strCandidates = new List<string>() { ";", ",", "|", "#", "/", "\\", "'", "!", "?", ":", ".", "$", "=", "-", "+", "*", "_", "<", ">", "@", "{", "}", "[", "]" };
            char startChar = 'a';
            char endChar = 'z';
            char startCharCaps = 'A';
            char endCharCaps = 'Z';


            for (char x = startChar; x < endChar; x++)
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

                    if (str.IndexOf(strCandidates[x]) >= 0)
                    {
                        bFound = false;
                        break;
                    }
                }

                if (bFound)
                {
                    return strCandidates[x];
                }
            }

            return null;
        }

        string getCall()
        {
            if (this.bRequirePosition)
            {
                return $"\"\".GetType().Assembly.GetType(\"{type.ToString()}\").GetMethods()[{methodPosition}]";
            }

            return $"\"\".GetType().Assembly.GetType(\"{type.ToString()}\").GetMethod(\"{strMethodPart}\")";
        }

        string getInvokeContent(List<string> lstValues)
        {
            string strSplitter = findSuitableSplitter(lstValues);

            if (strSplitter == null)
            {
                Console.WriteLine("[-] Error! Could not find a suitable splitter. Consider chosing another string.");

                return null;
            }

            string strArray = String.Join(strSplitter, lstValues);
            string strRetval = "null,\"" + strArray + "\".Split(\"" + strSplitter + "\".ToCharArray())";

            return strRetval;
        }

        public static CallMethod getMethodPosition(Type t, string strMethodName, int parameterCount, System.Type callType, string strAssemblyPath)
        {
            string strType = strMethodName.Substring(0, strMethodName.LastIndexOf('.'));
            string strMethodPart = strMethodName.Substring(strMethodName.LastIndexOf('.') + 1);
            MethodInfo[] minf = t.GetMethods();

            CallMethod callMethod = new CallMethod()
            {
                type = t,
                strMethodPart = strMethodPart,
                strTypePart = strType
            };

            // Check if we really need the method position
            try
            {
                MethodInfo minfSingle = t.GetMethod(strMethodPart);
                callMethod.bRequirePosition = false;

                Console.WriteLine($"[+] Method {strMethodName} can be called without knowing the exact position of it in the class");
            }
            catch(System.Reflection.AmbiguousMatchException ex)
            {
                Console.WriteLine($"[+] Method {strMethodName} is probably polymorph ({ex.Message}), hence, we require the exact position of the function to call it.");
            }

            for (int x = 0; x < minf.Length; x++)
            {
                MethodInfo m = minf[x];

                if (m.Name == strMethodPart)
                {
                    ParameterInfo[] p = m.GetParameters();

                    if (p.Length != parameterCount) continue;

                    bool bFoundCall = true;

                    for (int i = 0; i < p.Length; i++)
                    {
                        if (p[i].ParameterType != callType)
                        {
                            bFoundCall = false;

                            break;
                        }
                    }

                    if (bFoundCall)
                    {
                        callMethod.methodPosition = x;
                        callMethod.minf = m;
                        callMethod.parameterType = callType;
                        break;
                    }
                }
            }

            return callMethod;
        }

        public string buildParameterStringByCIAU(List<string> lstValues, string strMethodName)
        {
            string strFullname = "System, " + this.minf.Module.Assembly.FullName.Substring(this.minf.Module.Assembly.FullName.IndexOf(',')+1).Trim();
            string strType = strMethodName.Substring(0, strMethodName.LastIndexOf('.'));


            System.AppDomain app = System.AppDomain.CurrentDomain;
            object obj =  app.CreateInstanceAndUnwrap(strFullname, strType);

            CallMethod cm = getMethodPosition(obj.GetType(), strMethodName, lstValues.Count, typeof(System.String), null);

            string strInvokeContent = getInvokeContent(lstValues);
            string strRetval = $"\"\".GetType().Assembly.GetType(\"System.AppDomain\").GetMethods()[{this.methodPosition}].Invoke(\"\".GetType().Assembly.GetType(\"System.AppDomain\").GetProperty( \"CurrentDomain\").GetValue(null), \"{strFullname};{strType}\".Split(\";\".ToCharArray())).GetType().GetMethods()[{cm.methodPosition}].Invoke({strInvokeContent}).ToString()";


            return strRetval;
        }

        public string buildParameterString(List<string> lstValues)
        {
            if (parameterType == typeof(String))
            {
                // "".GetType().Assembly.GetType("System.IO.File").GetMethods()[37].Invoke(null,"fff;#ccc###".Split(";".ToCharArray()))).ToString()

                string strInvokeContent = getInvokeContent(lstValues);
                string strCall = getCall();

                string strRetval = $"{strCall}.Invoke({strInvokeContent})";

                return strRetval;
            }

            return null;
        }
    }
}
