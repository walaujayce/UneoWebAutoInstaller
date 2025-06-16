using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UneoWebApplicationAutoInstaller.Utilities
{
    public class Log
    {
        public static readonly Log Instance = new Log();

        // Private constructor to prevent external instantiation
        private Log()
        {
        }

        //Debug Information
        public static void Y(string tag, string msg)
        {
#if DEBUG
            Debug.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} Y {tag}, {msg}");
            //WriteLogs(tag + " " + msg);
#endif
        }

        //Device
        public static void D(string tag, string msg)
        {
            Debug.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} D {tag}, {msg}");
            //WriteLogs(tag + " " + msg);
        }

        //Information
        public static void I(string tag, string msg)
        {
            Debug.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} I {tag}, {msg}");
            //WriteLogs(tag + " " + msg);
        }

        //Error
        public static void E(string tag, string msg)
        {
            Debug.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} E {tag}, {msg}");
            //WriteLogs(tag + " " + msg);
        }

        //System
        public static void S(string tag, string msg)
        {
            Debug.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} S {tag}, {msg}");
            //WriteLogs(tag + " " + msg);
        }

        public static void WriteLogs(string message)
        {
            string path = $"{Directory.GetCurrentDirectory()}/logs/";
            string logFileName = $"{DateTime.Today.ToString("yyyyMMdd")}.log";
            Directory.CreateDirectory(path);
            string logContent = string.Join("|", DateTime.Now.ToString("HH:mm:ss"), message);
            File.AppendAllText(string.Concat(path, logFileName), logContent + "\r\n");
        }
    }
}
