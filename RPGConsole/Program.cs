using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RPGConsole
{
    class Program
    {
        private static Thread InputThread { get; set; }
        private static int Rows { get; set; }
        private static int Cols { get; set; }
        private static int Cells { get; set; }

        private static VecC Player { get; set; }
        private static VecC Food { get; set; }

        private static int OrigRow { get; set; }
        private static int OrigCol { get; set; }

        private static bool Running { get; set; }

        private static string blankChar = " ";
        private static string playerChar = "#";
        private static string foodChar = "*";
        private static bool wrapPlayer = true;

        static void Main(string[] args)
        {
        }
    }
}
