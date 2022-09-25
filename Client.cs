using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatApp
{
    internal class Client : CommandExecutor
    {
        // 1. Создание подключения
        // 2. Обработка данных от сервера
        // 3. Отправка нового сообщения

        private int id = -1;
        private string host = "127.0.0.1";
        private int port = 123321;
        private Socket socket;

        private Task messageInputTask;
        private Task dataReceiveTask;

        public int ID { get { return id; } }



        public Client(string name, string host="127.0.0.1", int port=123321)
        {
            this.host = host;
            this.port = port;
            this.name = name;
        }

        // Создание подключения
        public void Connect()
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(host), port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipPoint);
            SendFirstData();

            messageInputTask = Task.Factory.StartNew(() => MessageInputProccess());
            dataReceiveTask = Task.Factory.StartNew(() => DataReceiveProccess());
            Task.WaitAll(messageInputTask, dataReceiveTask);
        }

        public void SendFirstData()
        {
            MessagePacket welcomePacket = new MessagePacket();
            welcomePacket.senderName = name;
            welcomePacket.content = "/connect";
            welcomePacket.sendTimestamp = Utils.GetTimeNow();
            byte[] data = new byte[1024];
            data = Converter.ToByteArray<MessagePacket>(welcomePacket);
            socket.Send(data);
            Console.WriteLine("[Client] Успешное подключение!");
        }

        public void MessageInputProccess()
        {
            while (true)
            {
                string text = Console.ReadLine();

                MessagePacket msg = new MessagePacket();
                msg.senderName = name;
                msg.content = text;

                byte[] data = Converter.ToByteArray<MessagePacket>(msg);
                socket.Send(data);

                Thread.Yield();
            }
        }

        public void DataReceiveProccess()
        {
            while (true)
            {
                byte[] data = new byte[1024];
                int bytes = 0;

                if (socket.Available <= 0) { Thread.Yield(); continue; }
                do { bytes = socket.Receive(data); }
                while (socket.Available > 0);

                MessagePacket newMessage = Converter.ToStruct<MessagePacket>(data);
                if (newMessage.content.StartsWith("/"))
                {
                    CommandExecutor sender = this;
                    if (!newMessage.senderName.Equals(name)) { sender = new Client(newMessage.senderName); }
                    CommandSerealizer commandSerealizer = new CommandSerealizer(this, sender, newMessage.content);
                    commandSerealizer.Parse();
                }
                else
                {
                    PrintMessage(newMessage);
                }

                Thread.Yield();
            }
        }
        
        // Переименование пользователя
        public override void RenameClient(CommandExecutor ex, string newName)
        {
            if (newName.Equals(name))
            {
                Console.WriteLine("[Error] You already have such this name");
                return;
            }

            Console.WriteLine("[Client] Client '" + ex.Name + "' renamed to '" + newName + "'");
            ex.Name = newName;
        }

        // "Действие"
        public override void SendClientAction(CommandExecutor ex, string action)
        {
            Console.WriteLine("[*] " + action + " by " + ex.Name);
        }
    }
}
