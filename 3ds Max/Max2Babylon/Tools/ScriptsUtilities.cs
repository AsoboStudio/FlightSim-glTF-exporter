using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.IO;

namespace Max2Babylon
{
    static class ScriptsUtilities
    {
        public static void ExecutePythonFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                string cmd = $@"python.ExecuteFile ""{filePath}""";
                ExecuteMaxScriptCommand(cmd);
            }
        }

        public static void ExecutePythonCommand(string pythonCmd)
        {
            string cmd = $@"python.Execute ""{pythonCmd}""";
            ExecuteMaxScriptCommand(cmd);
        }

        public static void ExecuteMaxScriptCommand(string maxScriptCmd)
        {
            if (!string.IsNullOrEmpty(maxScriptCmd))
            {
#if MAX2015 || MAX2016 || MAX2017 || MAX2018 || MAX2019 || MAX2020 || MAX2021
                ManagedServices.MaxscriptSDK.ExecuteMaxscriptCommand(maxScriptCmd);
#else
                ManagedServices.MaxscriptSDK.ExecuteMaxscriptCommand(maxScriptCmd, 0);
#endif
            }
            }

        public static void ExecuteMAXScriptScript(string mxs, bool quietErrors, IFPValue fpv) 
        {
#if MAX2015 || MAX2016 || MAX2017 || MAX2018
            Loader.Global.ExecuteMAXScriptScript(mxs, quietErrors, fpv);
#elif MAX2019 || MAX2020 || MAX2021
            Loader.Global.ExecuteMAXScriptScript(mxs, quietErrors, fpv, true);
#else
            Loader.Global.ExecuteMAXScriptScript(mxs, 0, quietErrors, fpv, true);
#endif
        }



        public static void ExecuteMaxScriptFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                string maxScriptCmd = File.ReadAllText(filePath);
                ExecuteMaxScriptCommand(maxScriptCmd);
            }
        }

        public static string ExecuteMaxScriptQuery(string mxsCode)
        {
#if MAX2015 || MAX2016 || MAX2017 || MAX2018 || MAX2019 || MAX2020 || MAX2021
             return ManagedServices.MaxscriptSDK.ExecuteStringMaxscriptQuery(mxsCode);           
#else
            return ManagedServices.MaxscriptSDK.ExecuteStringMaxscriptQuery(mxsCode,0);
#endif
                      
        }
    }
}
