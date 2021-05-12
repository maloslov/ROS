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
        public int checkLog(ref string data, ref int errorCode)
        {
            data += "Вход в checkLog\r\n";
            try
            {
                if (!File.Exists(Path))
                {
                    errorCode = 3;
                    data += "Выход из checkLog с кодом 1\r\n";
                    return 1;
                }
                writer = File.Open(Path,FileMode.Truncate,FileAccess.Write,FileShare.Read);
            }
            catch (Exception e)
            {
                errorCode = e.GetHashCode();
                data += "Файл журнала недоступен\r\n";
                return 1;
            }
            canWrite = true;
            data += "Выход из checkLog с кодом 0\r\n";
            return 0;
        }
        public int createLog()
        {
            File.CreateText(Path);
            return 0;
        }
        public int writeLog(ref string data, ref int errorCode)
        {
                Console.WriteLine(data);
            if (writer == null)
            {
                errorCode = 1;
                return 1;
            }
            else if ((writer != null && !writer.CanWrite))
            {
                errorCode = 1;
                return 1;
            }
            try
            {
                var bdata = Encoding.UTF8.GetBytes(data);
                writer.Write(bdata, 0, bdata.Length);
            }
            catch (IOException e)
            {
                errorCode = e.GetHashCode();
                Console.WriteLine("Запись в файл журнала недоступна\r\n" + data);
                return 1;
            }
            return 0;
        }
        public void logDiag(ref int errorCode)
        {
            Console.WriteLine("Причина: ошибка №{0}", errorCode);
            //switch (errorCode)
            {

            }
        }
    }
}
