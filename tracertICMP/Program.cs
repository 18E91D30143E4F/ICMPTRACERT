using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace tracertICMP
{
    class Program
    {
        static void Main(string[] args)
        {
            const int MaxHop = 30;
            bool needNames = true;
            string strHost = "";
            byte[] buffer = new byte[1024];
            byte[] data = new byte[64]; Array.Clear(data, 0, data.Length);


            if (args.Length > 0)
                strHost = args[0];

            if (args.Length > 1)
                needNames = !(args[1] == "-p");

            var myIcmp = new ICMP(data);

            IPHostEntry iphe = Dns.GetHostEntry(strHost);

            Socket host = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
            IPEndPoint iep = new IPEndPoint(iphe.AddressList[0], 0);
            EndPoint ep = (EndPoint)iep;
            host.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 10000);

            int recv, responceTimeout;
            TimeSpan timestop;  DateTime timestart;

            Console.WriteLine("Трассировка маршрута к {0} [{1}]\nс максимальным числом прыжков {2}:\n", strHost, iphe.AddressList[0], MaxHop);

            for (int i = 1; i < MaxHop; i++)
            {
                host.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, i);

                string ipNotPort = "NF";
                responceTimeout = 0;

                Console.Write("{0, 3} ", i);

                for (int j = 0; j < 3; j++)
                {
                    try
                    {
                        timestart = DateTime.Now;
                            host.SendTo(myIcmp.Message, myIcmp.PacketSize, SocketFlags.None, iep);
                            recv = host.ReceiveFrom(buffer, ref ep);
                        timestop = DateTime.Now - timestart;

                        ipNotPort = ep.ToString().Substring(0, ep.ToString().LastIndexOf(':'));

                        Console.Write("{0, 5} ms  ", timestop.Milliseconds.ToString());
                    }
                    catch (SocketException)
                    {
                        responceTimeout++;
                        Console.Write("{0, 10}", '*');
                    }

                    // Inc sequence
                    myIcmp.UpdateSequence();               
                }
                if (needNames)
                {
                    try
                    {
                        IPHostEntry name = Dns.GetHostEntry(ipNotPort);
                        Console.WriteLine(string.Format("{1} [{0}]", ipNotPort, name.HostName));
                    }
                    catch (SocketException)
                    {
                        if (responceTimeout == 3)
                        {
                            Console.WriteLine("Timeout");
                        }
                        else
                        {
                            Console.WriteLine(string.Format("{0} ", ipNotPort));
                        }
                    }
                }
                else
                {
                    if (responceTimeout == 3)
                    {
                        Console.WriteLine("Timeout");
                    }
                    else
                    {
                        Console.WriteLine(string.Format("{0} ", ipNotPort));
                    }
                }

                // isResponce code = 0
                if (buffer[20] == 0)
                {
                    Console.WriteLine("\nТрассировка завершена");
                    break;
                }
            }
            Console.ReadLine();
        }
    }
}
