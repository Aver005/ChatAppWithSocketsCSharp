using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace ChatApp
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct MessagePacket
    {
        public string senderName;
        public string content;
        public long sendTimestamp;
    }

    class App
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);

            Console.Write("Server or Client? ");
            string action = Console.ReadLine();
            Console.Write("Your name? ");
            string userName = Console.ReadLine();

            if (action.Equals("Server"))
            {
                Server server = new Server(userName, "127.0.0.1", 1234);
                server.Start();
                return;
            }

            Client client = new Client(userName, "127.0.0.1", 1234);
            client.Connect();
        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("exit");
        }
    }
}