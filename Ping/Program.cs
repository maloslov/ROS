//ДЕКЛАРАЦИЯ БИБЛИОТЕК
using System;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace Ping
{
    class Program
    {
        //ДЕКЛАРАЦИЯ ПЕРЕМЕННЫХ
        static string logdata;      //текст для записи
        static Socket socket;       //сокет для приема и передачи
        static ICMP icmp;           //сборщик icmp-сообщений
        static IPEndPoint remoteEP; //проверяемый хост
        static byte[] buffer;       //буфер для icmp
        static Stopwatch timer;     //секундомер
        static Log log;             //журнал
        static int numReq;          //Количество запросов
        static int errorCode;       //переменная для записи ошибок
        static int logErrorCode;    //ошибки журнала
        
        //процедуры
        static void Main(string[] args)
        {
            //ИНИЦИАЛИЗАЦИЯ ПЕРЕМЕННЫХ
            logdata = "";                                    //переменная резульата
            remoteEP = null;                                 //проверяемый хост
            buffer = new byte[32];                           //буфер для icmp
            icmp = new ICMP();                               //сборщик icmp
            timer = new Stopwatch();                         //секундомер для подсчета времени
            log = new Log("c:\\Ping\\ping.log");             //модуль журнала
            socket = new Socket(                             //сокет для icmp
                AddressFamily.InterNetwork,
                SocketType.Raw,
                ProtocolType.Icmp);
            socket.Bind(new IPEndPoint(IPAddress.Any, 0));   //привязка адреса сокету
            socket.Ttl = 65;                                 //ТТЛ по умолчанию
            socket.ReceiveTimeout = 1000;                    //Время ожидания echoReply
            numReq = 2;                                      //количество запросов
            errorCode = 0;                                   //код ошибки
            logErrorCode = 0;                                //код ошибки журнала

            //ТЕЛО ПРОГРАММЫ
            switch (checkParams(args))                       //проверка параметров
            {
                case 0:                                      //параметры верные
                    switch (log.checkLog(ref logdata
                        ,ref logErrorCode))                  //проверка журнала
                    {
                        case 0:                              //журнал существует
                            for (int i = 0; i < numReq; i++)
                            {
                                switch (makeRequest())       //отправка запроса
                                {
                                    case 0:                  //успешно отправлен
                                        switch (makeReply()) //проверка ответа
                                        {
                                            case 0:          //ответ получен
                                                break;
                                            case 1:          //ошибка при получении
                                                Diag();      //диагностика
                                                break;
                                        }
                                        break;
                                    case 1:                  //ошибка отправки запроса
                                        Diag();              //диагностика
                                        break;
                                }
                            }                      
                            switch (log.writeLog(ref logdata
                                , ref logErrorCode))        //запись в журнал
                            {
                                case 0:
                                    Finish();               //завершение
                                    break;
                                case 1:
                                    log.logDiag(ref logErrorCode
                                        , ref logdata);     //диагностика журнала
                                    Finish();               //завершение
                                    break;
                            }                       
                            break;
                        case 1:                              //файл журнала отсутствует
                            log.logDiag(ref logErrorCode, ref logdata);
                            switch (log.createLog(
                                ref logErrorCode))           //создание файла
                            {
                                case 0:                      //файл существует
                                    for (int i = 0; i < numReq; i++)
                                    {
                                        switch (makeRequest())       //отправка запроса
                                        {
                                            case 0:                  //успешно отправлен
                                                switch (makeReply()) //проверка ответа
                                                {
                                                    case 0:          //ответ получен
                                                        break;
                                                    case 1:          //ошибка при получении
                                                        Diag();      //диагностика
                                                        break;
                                                }
                                                break;
                                            case 1:                  //ошибка отправки запроса
                                                Diag();              //диагностика
                                                break;
                                        }
                                    }
                                    switch (log.writeLog(ref logdata
                                        ,ref logErrorCode))  //запись в журнал
                                    {
                                        case 0:
                                            Finish();         //завершение
                                            break;
                                        case 1:
                                            log.logDiag(ref logErrorCode
                                                ,ref logdata);//диагностика журнала
                                            Finish();         //завершение
                                            break;
                                    }
                                break;
                                case 1:                       //ошибка создания файла
                                    log.logDiag(ref logErrorCode
                                        ,ref logdata);        //диагностика журнала 
                                    Finish();                 //завершение
                                break;
                            }                        
                            break;
                    }
                    break;
                case 1:                                       //ошибка параметров
                    Diag();                                   //диагностика
                    switch (log.checkLog(ref logdata
                        , ref logErrorCode))                  //проверка файла журнала
                    {
                        case 0:                               //файл существует
                            switch (log.writeLog(ref logdata
                                , ref logErrorCode))          //запись в журнал
                            {
                                case 0:
                                    Finish();                 //завершение
                                    break;
                                case 1:
                                    log.logDiag(ref logErrorCode
                                        ,ref logdata);        //диагностика журнала
                                    Finish();                 //завершение
                                    break;
                            }
                            break;
                        case 1:                               //файл журнала отсутствует
                            switch (log.createLog(
                                ref logErrorCode))            //создание файла
                            {
                                case 0:                       //файл создан
                                    switch (log.writeLog(ref logdata
                                        , ref logErrorCode))  //запись в журнал
                                    {
                                        case 0:
                                            Finish();         //завершение
                                            break;
                                        case 1:
                                            log.logDiag(ref logErrorCode
                                                ,ref logdata);//диагностика журнала
                                            Finish();         //завершение
                                            break;
                                    }
                                    break;
                                case 1:                       //ошибка создания файла
                                    log.logDiag(ref logErrorCode
                                        ,ref logdata);        //диагностика журнала 
                                    Finish();                 //завершение
                                    break;
                            }
                            break;
                    }
                    break;
            }
        }
        static int checkParams(string[] param)
        {
            Console.WriteLine("Файл журнала: {0}", log.Path);
            logdata += "Вход в checkParams\r\n";                    //DEBUG
            switch (param.Length)                                   //проверка параметра
            {
                case 0:                                             //параметра нет
                    logdata += "Отсутствует входной параметр\r\n";
                    logdata += "Выход из checkParams с кодом 1\r\n";//DEBUG
                    errorCode = 1;
                    return 1;
                case 1:                                             //параметр есть
                    try
                    {
                        IPAddress ip = IPAddress.Parse(param[0]);
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                            remoteEP = new IPEndPoint(ip, 0);
                        else throw new FormatException();
                    }
                    catch (FormatException e)
                    {
                        errorCode = 2;
                        logdata += //"Адрес "+param[0] + " не соответствует IPv4\r\n"+
                            "Выход из checkParams с кодом 1\r\n";  //DEBUG
                        return 1;
                    }
                    break;
                default:                                            //параметров много
                    logdata += "Слишком много аргументов\r\n";
                    logdata += "Выход из checkParams с кодом 1\r\n";//DEBUG
                    errorCode = 3;
                    return 1;
            }
            if (remoteEP != null)
                logdata += String.Format(param[0] + " корректный IPv4\r\n");
            logdata += String.Format("ТТЛ={0},Время ожидания ответа={1}ms\r\n", socket.Ttl, socket.ReceiveTimeout);
            logdata += "Выход из checkParams с кодом 0\r\n";        //DEBUG
            return 0;
        }
        static int makeRequest()
        {
            logdata += "Вход в makeRequest\r\n";                    //DEBUG
            timer.Start();
            try
            {
                socket.SendTo(icmp.getBytes(), SocketFlags.None, remoteEP);
            }
            catch (SocketException e)
            {
                errorCode = 5;
                switch (e.ErrorCode)
                {
                    case 10065:
                        break;
                    default:
                        logdata += e.Message+"\r\n";
                        break;
                }
                logdata += "Выход из makeRequest с кодом 1\r\n";    //DEBUG
                return 1;
            }
            logdata += "Запрос отправлен\r\n"
                +"Выход из makeRequest с кодом 0\r\n";              //DEBUG
            return 0;
        }
        static int makeReply()
        {
            logdata += "Вход в makeReply\r\n";                      //DEBUG
            //ИНИЦИАЛИЗАЦИЯ ЛОКАЛЬНЫХ ПЕРЕМЕННЫХ
            EndPoint ep = remoteEP;
            var pack = new ICMP(buffer, buffer.Length);
            var ipfrom = new byte[4];

            //ТЕЛО ПРОЦЕДУРЫ
            try
            {
                socket.ReceiveFrom(buffer, ref ep);
                timer.Stop();
            }
            catch (SocketException e)
            {
                errorCode = 4;
                switch (e.ErrorCode)
                {
                    case 10060:
                        //logdata+="Время ожидания ответа истекло\r\n";
                        break;
                    default:
                        logdata += e.Message + "\r\n";
                        break;
                }
                logdata+="Выход из makeReply с кодом 1\r\n";        //DEBUG
                return 1;
            }
            Buffer.BlockCopy(buffer, 12, ipfrom, 0, 4);
            logdata += String.Format("Ответ {0} получен за {1} мс\r\n",
                new IPAddress(ipfrom).ToString(),
                timer.ElapsedMilliseconds < 1 ? "<1" : timer.ElapsedMilliseconds.ToString());
            timer.Reset();
            logdata += "Выход из makeReply с кодом 0\r\n";          //DEBUG
            return 0;
        }
        static void Finish() 
        {
            Console.WriteLine(logdata);
        }
        static void Diag()
        {
            logdata += String.Format("Причина: ошибка №{0} - ", errorCode);
            switch (errorCode)
            {
                case 1:
                    logdata += "Отсутствует параметр\r\n";
                    break;
                case 2:
                    logdata += "Некорректный параметр\r\n";
                    break;
                case 3:
                    logdata += "Избыток параметров\r\n";
                    break;
                case 4:
                    logdata += "Время ожидания ответа истекло\r\n";
                    break;
                case 5:
                    logdata += "Ошибка отправки запроса\r\n";
                    break;
            }
        }
    }
}
