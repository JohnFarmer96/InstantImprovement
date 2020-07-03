using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstantImprovement.SDKControl
{
    class FilePath
    {
        static public String GetClassifierDataFolder()
        {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            return appDir + "\\data";
        }

        static public String GetAffdexLicense()
        {
            string fileName = "affdex.license";
            return File.ReadAllText(fileName);
        }
    }
}
