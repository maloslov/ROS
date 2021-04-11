using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;

namespace Ping
{
    static class Log
    {
        private static string Path { get; set; }
        private static FileStream writer { get; set; }
        
        public static int checkLog(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    File.CreateText(path);
                }
                Path = path;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }
            
            return 0;
        }
        public static int writeLog(string data)
        {
            try
            {
                File.Open(Path,FileMode.Open, FileAccess.Write, FileShare.Read);
                File.WriteAllText(Path,data,Encoding.UTF8);
            }
            catch (IOException) { return 1; }
            return 0;
        }
        public static int logDiag()
        {

            return 1;
        }
    }
}
