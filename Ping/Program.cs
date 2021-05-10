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
        
        static void Main(string[] args)
        {
            //RESTRICTIONS & DEFAULTS
            logdata = "Start\n";
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

            //PROGRAM
            switch (log.checkLog())
            {
                case 0:
                    switch (checkParams(args))
                    {
                        case 0:
                            for(int i = 0; i < numReq; i++)
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
                            break;
                    }
                    break;
                case 1:
                    log.logDiag();
                    break;
            }
            Finish();
        }
        static int checkParams(string[] param)
        {
            Console.WriteLine("Log: {0}", log.Path);
            logdata += "Enter checkParams\n";
            switch (param.Length)
            {
                case 0:
                    logdata += "No IP input\nExit checkParams\n";
                    return 1;
                case 1:
                    try
                    {
                        IPAddress ip = IPAddress.Parse(param[0]);
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                            remoteEP = new IPEndPoint(ip, 0);
                        else throw new FormatException();
                    }
                    catch (FormatException)
                    {
                        logdata += param[0]+" is wrong IPv4\nExit checkParams\n";
                        return 1;
                    }
                    break;
            }
            logdata += String.Format(param[0]+" is correct IPv4\n");
            logdata += String.Format("TTl={0},Timeout={1}ms\n", socket.Ttl, socket.ReceiveTimeout);
            logdata += "Exit checkParams\n";
            return 0;
        }
        static int makeRequest()
        {
            logdata += "Enter makeRequest\n";
            timer.Start();
            try
            {
                socket.SendTo(icmp.getBytes(), SocketFlags.None, remoteEP);
            }
            catch (Exception e)
            {
                logdata+=e.StackTrace+"\nExit makeRequest\n";
                return 1;
            }
            logdata += "Request done\nExit makeRequest\n";
            return 0;
        }
        static int makeReply()
        {
            logdata += "Enter makeReply\n";
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
                logdata += (e.Message) + '\n';
                return 1;
            }
            Buffer.BlockCopy(buffer, 12, ipfrom, 0, 4);
            logdata += String.Format("From {0} received by {1} ms\n",
                new IPAddress(ipfrom).ToString(),
                timer.ElapsedMilliseconds < 1 ? "<1" : timer.ElapsedMilliseconds.ToString());
            timer.Reset();
            logdata += "Reply done\nExit makeReply\n";
            return 0;
        }
        static void Finish() 
        {
            logdata += "Finish\n";
            log.writeLog(ref logdata);
            Console.ReadKey();
        }
        static void Diag() { }
    }
}
