using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace PolReader
{
    class AutoRunInfo
    {
        public string AutoPath { get; set; }        
        public bool AutoCompareFile { get; set; }
        public bool LogToDB { get; set; }
        public bool CheckPreviousRelease { get; set; }
        
        private DateTime StartDate;

        public AutoRunInfo(string[] CommandLineArguments)
        {
            AutoPath = "";
            StartDate = DateTime.Now;

            foreach (string cmdLineOption in CommandLineArguments)
            {
                if (cmdLineOption.ToUpper().StartsWith("/?") || cmdLineOption.ToUpper().StartsWith("-?"))
                {
                    MessageBox.Show(GetHelpParameters());
                    AutoPath = "";
                    break;
                }
                else if (cmdLineOption.ToUpper().StartsWith("/AUTOPATH:"))
                {
                    AutoPath = cmdLineOption.ToUpper().Replace("/AUTOPATH:", "");

                    if(!AutoPath.EndsWith("\\"))
                        AutoPath+="\\";

                }
                else if (cmdLineOption.ToUpper().StartsWith("/AUTOCOMPARE"))
                {
                    AutoCompareFile = true;
                }
                else if (cmdLineOption.ToUpper().StartsWith("/LOGTODB"))
                {
                    LogToDB = true;
                }
                else if (cmdLineOption.ToUpper().StartsWith("/CHECKPREVIOUSRELEASE"))
                {
                    CheckPreviousRelease = true;
                }

            }         
            
        }

        public bool AutoRun()
        {
            return Directory.Exists(AutoPath) || LogToDB;
        }

        public bool LogToAutoPath()
        {
            return Directory.Exists(AutoPath);
        }

        public bool CompareToYesterday()
        {
            return AutoCompareFile && File.Exists(GetYesterdaysFile());
        }

        public string GetYesterdaysFile()
        {
            DateTime dt = StartDate.AddDays(-1.0);

            return AutoPath + dt.Day + "_" + dt.Month + "_" + dt.Year + ".Epl";

        }

        public string GetSaveFilename()
        {
            return AutoPath + StartDate.Day + "_" + StartDate.Month + "_" + StartDate.Year + ".Epl";
        }

        public string GetSaveCompareFilename()
        {
            return AutoPath + StartDate.Day + "_" + StartDate.Month + "_" + StartDate.Year + ".Cpf";
        }

        public string GetHelpParameters()
        {
            return "/AUTOPATH:Path to filestore\n\n\tThis will create a snapshot of the current GPOs" +
                    "\n\n/AUTOCOMPARE\n\n\tThis will create an additional compare against yesterdays Polcies"+
                    "\n\n/LOGTODB\n\n\tThis will log results to the SQL DB";
        }

    }
}
