//ДЕКЛАРАЦИЯ БИБЛИОТЕК
using System;
using System.IO;
using System.Text;

namespace Ping
{
    class Log
    {
        //декларация переменных
        public string Path { get; private set; }
        private FileStream writer { get; set; }
        public bool canWrite;
        
        public Log(string path)
        {
            //ИНИЦИАЛИЗАЦИЯ ПЕРЕМЕННЫХ
            Path = path;
            canWrite = false;
            writer = null;
        }
        public int checkLog(ref string data, ref int errorCode)
        {
            data += "Вход в checkLog\r\n";                      //DEBUG
            try
            {
                if (!File.Exists(Path))
                {
                    errorCode = 3;
                    data += "Выход из checkLog с кодом 1\r\n";  //DEBUG
                    return 1;
                }
                /*
                writer = File.Open(Path
                    ,FileMode.Truncate
                    ,FileAccess.Write
                    ,FileShare.Read);
                */
            }
            catch (Exception e)
            {
                errorCode = e.GetHashCode();
                data += "Файл журнала недоступен\r\n";
                return 1;
            }
            canWrite = true;
            data += "Выход из checkLog с кодом 0\r\n";          //DEBUG
            return 0;
        }
        public int createLog(ref int errorCode)
        {
            try
            {
                File.CreateText(Path).Close();
            }
            catch(Exception e)
            {
                errorCode = 1;
                //data += "Не удалось создать файл\r\n";          //DEBUG
                return 1;
            }
            return 0;
        }
        public int writeLog(ref string data, ref int errorCode)
        {
            data += "Запись в файл журнала\r\n";                //DEBUG
            try
            {
                writer = File.Open(Path
                    , FileMode.Truncate
                    , FileAccess.Write
                    , FileShare.Read);
                //writer = new StreamWriter(Path);
                canWrite = true;
            }
            catch(Exception e)
            {
                errorCode = 13;
                return 1;
            }
            if (writer == null)
            {
                errorCode = 10;
                data += "Запись в файл не удалась\r\n";         //DEBUG
                return 1;
            }
            else if ((writer != null && !canWrite))
            {
                errorCode = 11;
                data += "Запись в файл не возможна\r\n";        //DEBUG
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
                return 1;
            }
            return 0;
        }
        public void logDiag(ref int errorCode, ref string data)
        {
            data += String.Format("Причина: ошибка №{0}", errorCode);
        }
    }
}
