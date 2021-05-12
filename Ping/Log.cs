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
            if (!File.Exists(Path))
            {
                errorCode = 1;
                data += "Выход из checkLog с кодом 1\r\n";      //DEBUG
                return 11;
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
                errorCode = 12;
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
                errorCode = 14;
                data += "Запись в файл не удалась\r\n";         //DEBUG
                return 1;
            }
            else if ((writer != null && !canWrite))
            {
                errorCode = 14;
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
                errorCode = 15;
                return 1;
            }
            return 0;
        }
        public void logDiag(ref int errorCode, ref string data)
        {
            data += String.Format("Причина: ошибка №{0}:\r\n", errorCode);
            switch (errorCode)
            {
                case 11:
                    data += "Файл отсутствует\r\n";
                    break;
                case 12:
                    data += "Не удалось создать файл\r\n";
                    break;
                case 13:
                    data += "Не удалось открыть файл\r\n";
                    break;
                case 14:
                    data += "Запись в файл недоступна\r\n";
                    break;
                case 15:
                    data += "Ошибка при записи в файл\r\n";
                    break;
            }
        }
    }
}
