using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;

namespace Ping
{
    class Log
    {
        public string Path { get; private set; }
        private FileStream writer { get; set; }
        public bool canWrite = false;
        
        public Log(string path)
        {
            Path = path;
        }
        public int checkLog()
        {
            try
            {
                if (!File.Exists(Path))
                {
                        File.CreateText(Path);
                }
                writer = File.Open(Path,FileMode.Truncate,FileAccess.Write,FileShare.Read);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }
            canWrite = true;
            return 0;
        }
        public int writeLog(ref string data)
        {
            if (writer != null || !writer.CanWrite)
                Console.WriteLine(data);
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
