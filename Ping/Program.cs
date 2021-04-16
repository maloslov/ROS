using System;
using System.Net;
using System.Net.Sockets;

namespace Ping
{
    class Program
    {
        static string logdata;
        static Socket socket;
        static ICMP icmp;
        static IPEndPoint remoteEP;
        static byte[] buffer;
        static System.Diagnostics.Stopwatch timer;
        static string logPath;
        /*
        static void Main(string[] args)
        {
        Begin:
            -Console.CursorLeft = 0;
            -var host = IPAddress.Any;
            -Console.Write("Введите адрес хоста: ");
            \/var socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
            \/var remoteEP = new IPEndPoint(IPAddress.Parse(Console.ReadLine()), 0);
            \/socket.ReceiveTimeout = 1000;
            \/socket.Ttl = Convert.ToInt16(Console.ReadLine());
            --var pack = new ICMP();
            --pack.Checksum = pack.getChecksum();
            --Console.WriteLine("Обмен пакетами:");
            \/socket.Bind(new IPEndPoint(host, 0));
            --EndPoint point = remoteEP;
            for (int i = 0; i < 1; i++)
            {
                var timer = System.Diagnostics.Stopwatch.StartNew();
                socket.SendTo(pack.getBytes(), SocketFlags.None, remoteEP);
                byte[] buffer = new byte[32];
                try
                {
                    socket.ReceiveFrom(buffer, ref point);
                    Console.WriteLine(BitConverter.ToString(buffer));
                }
                catch (SocketException e)
                {
                    Console.WriteLine("{0} Error code: {1}", e.Message, e.ErrorCode);
                }
                timer.Stop();
                var pack2 = new ICMP(buffer, buffer.Length);
                var ipfrom = new byte[4];
                Buffer.BlockCopy(buffer, 12, ipfrom, 0, 4);
                Console.Write("Ответ от " + new IPAddress(ipfrom).ToString());
                Console.WriteLine(" за " + timer.ElapsedMilliseconds + " мс.");
                }
            if(Console.ReadKey().Key==ConsoleKey.Spacebar)
                goto Begin;
        }
        */
        static void Main(string[] args)
        {
            logdata = "hhujikuhiujin\n";
            remoteEP = null;
            buffer = new byte[32];
            icmp = new ICMP();
            socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Raw,
                ProtocolType.Icmp);
            socket.Bind(new IPEndPoint(IPAddress.Any, 0));
            socket.Ttl = 65;
            socket.ReceiveTimeout = 1000;
            timer = new System.Diagnostics.Stopwatch();
            logPath = "c:\\Ping\\ping.log";

            switch (checkParams(args))
            {
                case 0:
                    switch (Log.checkLog(logPath))
                    {
                        case 0:
                            switch (makeRequest())
                            {
                                case 0:
                                    makeReply();
                                    switch (makeRequest())
                                    {
                                        case 0:
                                            makeReply();
                                            switch (Log.writeLog(ref logdata))
                                            {
                                                case 0:
                                                    Finish();
                                                    break;
                                                case 1:
                                                    Log.logDiag();
                                                    Finish();
                                                    break;
                                            }
                                            break;
                                        case 1:
                                            Diag();
                                            switch (Log.writeLog(ref logdata))
                                            {
                                                case 0:
                                                    Finish();
                                                    break;
                                                case 1:
                                                    Log.logDiag();
                                                    Finish();
                                                    break;
                                            }
                                            break;
                                        case 2:
                                            switch (Log.writeLog(ref logdata))
                                            {
                                                case 0:
                                                    Finish();
                                                    break;
                                                case 1:
                                                    Log.logDiag();
                                                    Finish();
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                                case 1:
                                    Diag();
                                    switch (Log.writeLog(ref logdata))
                                    {
                                        case 0:
                                            Finish();
                                            break;
                                        case 1:
                                            Log.logDiag();
                                            Finish();
                                            break;
                                    }
                                    break;
                                case 2:
                                    switch (Log.writeLog(ref logdata))
                                    {
                                        case 0:
                                            Finish();
                                            break;
                                        case 1:
                                            Log.logDiag();
                                            Finish();
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case 1:
                            Log.logDiag();
                            Finish();
                            break;
                    }
                    break;
                case 1:
                    Finish();
                    break;
            }
        }
        static int checkParams(string[] param)
        {
            switch (param.Length)
            {
                case 0:
                    return 1;
                case 1:
                    try
                    {
                        remoteEP = new IPEndPoint(IPAddress.Parse(param[0]), 0);
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("Wrong IP!");
                        return 1;
                    }
                    break;
            }

            Console.WriteLine("Обмен пакетами:");
            return 0;
        }
        static int makeRequest()
        {
            timer.Start();
            try
            {
                socket.SendTo(icmp.getBytes(), SocketFlags.None, remoteEP);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return 1;
            }
            return 0;
        }
        static void makeReply()
        {
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
                Console.WriteLine(e.Message);
                return;
            }
            Buffer.BlockCopy(buffer, 12, ipfrom, 0, 4);
            Console.WriteLine("Ответ от {0} получен за {1} мс.",
                new IPAddress(ipfrom).ToString(),
                timer.ElapsedMilliseconds < 1 ? "<1" : timer.ElapsedMilliseconds.ToString());
        }
        static void Finish() { Console.WriteLine("Передача завершена."); }
        static void Diag() { }
    }
}
