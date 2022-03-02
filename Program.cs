using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SevenZip;
using System.IO;
using System.Configuration;

namespace SevenZipPOC
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SevenZip());
        }
    }

    public class SevenZipOperation
    {
        readonly string nativeDll = ConfigurationManager.AppSettings["nativeDll"];
        public SevenZipOperation()
        {
            SevenZipCompressor.SetLibraryPath(nativeDll);

        }
    }
}
