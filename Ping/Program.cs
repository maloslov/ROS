using System;
using System.Net;
using System.Net.Sockets;

namespace Ping
{
    class Program
    {
        static void Main(string[] args)
        {
        Begin:
            Console.CursorLeft = 0;
            var host = IPAddress.Any;
            Console.Write("Введите адрес хоста: ");
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
            var remoteEP = new IPEndPoint(IPAddress.Parse(Console.ReadLine()), 0);

            var pack = new ICMP();
            pack.Checksum = pack.getChecksum();
            Console.WriteLine("Обмен пакетами:");
            socket.Bind(new IPEndPoint(host, 0));
            for (int i = 0; i < 4; i++)
            {
                var timer = System.Diagnostics.Stopwatch.StartNew();
                socket.SendTo(pack.getBytes(), remoteEP);
                byte[] buffer = new byte[32];
                socket.Receive(buffer);
                timer.Stop();
                var pack2 = new ICMP(buffer, buffer.Length);
                var ipfrom = new byte[4];
                Buffer.BlockCopy(buffer, 12, ipfrom, 0, 4);
                Console.Write("Ответ от " + new IPAddress(ipfrom).ToString());
                //Console.WriteLine("Type: " + pack2.Type + " Code: " + pack2.Code);
                Console.WriteLine(" за " + timer.ElapsedMilliseconds + " мс.");
            }
            if(Console.ReadKey().Key==ConsoleKey.Spacebar)
                goto Begin;
        }
    }
}
