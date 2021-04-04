using System;
using System.Net;
using System.Net.Sockets;

namespace Ping
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = IPAddress.Any;
            Console.WriteLine("Input ip:");
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
            var remoteEP = new IPEndPoint(IPAddress.Parse(Console.ReadLine()), 0);

            var pack = new ICMP();
            pack.Checksum = pack.getChecksum();
            Console.WriteLine(BitConverter.ToString(pack.getBytes()));
            socket.Bind(new IPEndPoint(host, 0));
            var timer = System.Diagnostics.Stopwatch.StartNew();
            socket.SendTo(pack.getBytes(), remoteEP);
            byte[] buffer = new byte[32];
            socket.Receive(buffer);
            timer.Stop();
            var pack2 = new ICMP(buffer, buffer.Length);
            var ipfrom = new byte[4];
            Buffer.BlockCopy(buffer, 12, ipfrom, 0, 4);
            Console.WriteLine("Received from " + new IPAddress(ipfrom).ToString());
            Console.WriteLine("Type: " + pack2.Type + " Code: " + pack2.Code);
            Console.WriteLine("Time: " + timer.ElapsedMilliseconds + " ms");
        }
    }
}
