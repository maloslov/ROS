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
        static Log log;
        
        static void Main(string[] args)
        {
            logdata = "Start\n";
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
            log = new Log("c:\\Ping\\ping.log");

            switch (log.checkLog())
            {
                case 0:
                    switch (checkParams(args))
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
                                            Finish();
                                            break;
                                        case 1:
                                            Diag();
                                            Finish();
                                            break;
                                        case 2:
                                            Finish();
                                            break;
                                    }
                                    break;
                                case 1:
                                    Diag();
                                    Finish();
                                    break;
                                case 2:
                                    Finish();
                                    break;
                            }
                            break;
                        case 1:
                            Finish();
                            break;
                    }
                    break;
                case 1:
                    Log.logDiag();
                    Finish();
                    break;
            }
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
        static void makeReply()
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
                return;
            }
            Buffer.BlockCopy(buffer, 12, ipfrom, 0, 4);
            logdata += String.Format("From {0} received by {1} ms\n",
                new IPAddress(ipfrom).ToString(),
                timer.ElapsedMilliseconds < 1 ? "<1" : timer.ElapsedMilliseconds.ToString());
            timer.Reset();
            logdata += "Reply done\nExit makeReply\n";
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
