using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelAccessLib
{
    public class ExcelFilePath
    {
        private string fileName;
        
        /// <summary>
        /// ファイルが存在しかつそれがexcelファイルの拡張子である
        /// </summary>
        public bool IsExcelFile { get { return System.IO.File.Exists(fileName) && (fileName.EndsWith(".xls") || fileName.EndsWith(".xlsx")); } }
        public ExcelFilePath(string fileName)
        {
            this.fileName = fileName;
        }

        public void LaunchExcelWithProcess()
        {
            if (!File.Exists(fileName)) return;

            Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = true
            });
        }
    }
}
