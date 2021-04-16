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
                writer = File.Open(path,FileMode.Truncate,FileAccess.Write,FileShare.Read);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }
            return 0;
        }
        public static int writeLog(ref string data)
        {
            try
            {
                var bdata = Encoding.UTF8.GetBytes(data);
                writer.Write(bdata, 0, bdata.Length);
            }
            catch (IOException e) {
                Console.WriteLine(e.Message);
                return 1; 
            }
            data = "";
            return 0;
        }
        public static int logDiag()
        {

            return 1;
        }
    }
}
