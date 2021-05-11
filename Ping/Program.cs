//LIBRARIES
using System;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace Ping
{
    class Program
    {
        //DECLARATION
        static string logdata;      //текст для записи
        static Socket socket;       //сокет для приема и передачи
        static ICMP icmp;           //сборщик icmp-сообщений
        static IPEndPoint remoteEP; //проверяемый хост
        static byte[] buffer;       //буфер для icmp
        static Stopwatch timer;     //секундомер
        static Log log;             //журнал
        static int numReq;          //Количество запросов
        static int outFlag;         //режим вывода: 1-файл, 2-оба("-b"), 3-консоль("-c")
        
        static void Main(string[] args)
        {
            //RESTRICTIONS & DEFAULTS
            logdata = "Start\r\n";
            remoteEP = null;
            buffer = new byte[32];
            icmp = new ICMP();
            timer = new Stopwatch();
            log = new Log("c:\\Ping\\ping.log");
            socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Raw,
                ProtocolType.Icmp);
            socket.Bind(new IPEndPoint(IPAddress.Any, 0));
            socket.Ttl = 65;
            socket.ReceiveTimeout = 1000;
            numReq = 2;
            outFlag = 1;

            //PROGRAM
            switch (checkParams(args))
            {
                case 0:
                    switch (log.checkLog())
                    {
                        case 0:
                            for (int i = 0; i < numReq; i++)
                            {
                                switch (makeRequest())
                                {
                                    case 0:
                                        switch (makeReply())
                                        {
                                            case 0:
                                                break;
                                            case 1:
                                                Diag();
                                                break;
                                        }
                                        break;
                                    case 1:
                                        Diag();
                                        break;
                                }
                            }
                            break;
                        case 1:
                            log.logDiag();
                            break;
                    }
                    break;
                case 1:
                    switch (log.checkLog())
                    {
                        case 1:
                            log.logDiag();
                            break;
                    }
                    break;
            }
            Finish();
        }
        static int checkParams(string[] param)
        {
            int errCount = 0;   //считает ошибки в аргументах
            Console.WriteLine("Log: {0}", log.Path);
            logdata += "Enter checkParams\r\n";
            switch (param.Length)
            {
                case 0:
                    logdata += "No IP input\r\nExit checkParams\r\n";
                    return 1;
                case 2:
                    switch (param[1])
                    {
                        case "-b":
                            outFlag = 2;
                            break;
                        case "-c":
                            outFlag = 3;
                            break;
                        default:
                            logdata += "Wrong output flag\r\n";
                            errCount++;
                            break;
                    }
                    goto case 1;
                case 1:
                    switch (param[0])
                    {
                        case "-b":
                            outFlag = 2;
                            errCount++;
                            break;
                        case "-c":
                            outFlag = 3;
                            errCount++;
                            break;
                        default:
                            try
                            {
                                IPAddress ip = IPAddress.Parse(param[0]);
                                if (ip.AddressFamily == AddressFamily.InterNetwork)
                                    remoteEP = new IPEndPoint(ip, 0);
                                else throw new FormatException();
                            }
                            catch (FormatException)
                            {
                                logdata += param[0] + " is wrong IPv4\r\n";
                                errCount++;
                            }
                            break;
                    }
                    break;
                default:
                    logdata += "Too many args\r\n";
                    break;
            }
            if (remoteEP != null)
                logdata += String.Format(param[0] + " is correct IPv4\r\n");
            else logdata += "No IP argument\r\n";
            if (errCount > 0) { logdata += "Exit checkParams\r\n"; return 1; }
            logdata += String.Format("TTl={0},Timeout={1}ms\r\n", socket.Ttl, socket.ReceiveTimeout);
            logdata += "Exit checkParams\r\n";
            return 0;
        }
        static int makeRequest()
        {
            logdata += "Enter makeRequest\r\n";
            timer.Start();
            try
            {
                socket.SendTo(icmp.getBytes(), SocketFlags.None, remoteEP);
            }
            catch (SocketException e)
            {
                switch (e.ErrorCode)
                {
                    case 10065:
                        logdata += "Host unreachable\r\n";
                        break;
                    default:
                        logdata += e.Message+"\r\n";
                        break;
                }
                logdata += "Exit makeRequest\r\n";
                return 1;
            }
            logdata += "Request done\r\nExit makeRequest\r\n";
            return 0;
        }
        static int makeReply()
        {
            logdata += "Enter makeReply\r\n";
            EndPoint ep = remoteEP;
            var pack = new ICMP(buffer, buffer.Length);
            var ipfrom = new byte[4];
            try
            {
                socket.ReceiveFrom(buffer, ref ep);
                timer.Stop();
            }
            catch (SocketException e)
            {
                switch (e.ErrorCode)
                {
                    case 10060:
                        logdata += "Reply timeout exceeded\r\n";
                        break;
                    default:
                        logdata += e.Message + "\r\n";
                        break;
                }
                logdata += "Exit makeReply\r\n";
                return 1;
            }
            Buffer.BlockCopy(buffer, 12, ipfrom, 0, 4);
            logdata += String.Format("From {0} received by {1} ms\r\n",
                new IPAddress(ipfrom).ToString(),
                timer.ElapsedMilliseconds < 1 ? "<1" : timer.ElapsedMilliseconds.ToString());
            timer.Reset();
            logdata += "Reply done\r\nExit makeReply\r\n";
            return 0;
        }
        static void Finish() 
        {
            logdata += "Finish\r\n";
            log.writeLog(ref logdata, outFlag);
            //Console.ReadKey();
        }
        static void Diag() { }
    }
}
