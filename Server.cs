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
    internal class Server : CommandExecutor
    {
        private string host = "127.0.0.1";
        private int port = 1234;
        private int visitors = 0;
        private Socket socket;

        private Task receiveDataTask;
        private Task messageInputTask;
        private Task newConnectionsTask;

        private bool isWorking = false;

        private List<MessagePacket> messages = new List<MessagePacket>();
        private Dictionary<int, Socket> tempClients = new Dictionary<int, Socket>();
        private Dictionary<string, ServerClient> clients = new Dictionary<string, ServerClient>();


        public bool IsWorking { get { return isWorking; } }

        public Server(string name, string host = "127.0.0.1", int port = 123321)
        {
            this.host = host;
            this.port = port;
            this.name = name;

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        // Запуск сервера
        public void Start()
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse(host), port);
            socket.Bind(ip);
            socket.Listen(100);
            isWorking = true;

            Console.WriteLine("[Server] Сервер запущен!");

            newConnectionsTask = Task.Factory.StartNew(() => NewConnectionProccess());
            receiveDataTask = Task.Factory.StartNew(() => ReceiveMessageProccess());
            messageInputTask = Task.Factory.StartNew(() => MessageInputProccess());
            Task.WaitAll(newConnectionsTask, receiveDataTask, messageInputTask);
        }

        // Обработка нового подключения
        // Ожидание в бесконечном цикле
        public void NewConnectionProccess()
        {
            while (isWorking)
            {
                Socket handler = socket.Accept();
                Console.WriteLine("[Server] New connection!");
                tempClients.Add(visitors, handler);
                visitors++;

                Thread.Yield();
            }
        }

        // Обработка новых сообщений
        // Ожидание в бесконечном цикле
        public void ReceiveMessageProccess()
        {
            while (isWorking)
            {
                List<int> tempClientKeysForRemove = new List<int>();
                List<CommandSerealizer> commandQueue = new List<CommandSerealizer>();

                for (int i = 0; i < tempClients.Count; i++)
                {
                    int id = tempClients.Keys.ElementAt(i);
                    Socket handler = tempClients[id];
                    if (handler.Available <= 0) { continue; }

                    int bytes = 0;
                    byte[] data = new byte[1024];

                    do { bytes = handler.Receive(data); }
                    while (handler.Available > 0);

                    MessagePacket newMessage = Converter.ToStruct<MessagePacket>(data);
                    if (newMessage.content == "/connect")
                    {
                        ServerClient newClient = new ServerClient(id, handler, newMessage.senderName);
                        clients.Add(newMessage.senderName, newClient);
                        tempClientKeysForRemove.Add(id);
                        SendAllMessages(newClient);
                    }
                }

                foreach (var client in clients.Values)
                {
                    Socket handler = client.socket;
                    if (handler.Available <= 0) { continue; }

                    int bytes = 0;
                    byte[] data = new byte[1024];

                    do { bytes = handler.Receive(data); }
                    while (handler.Available > 0);

                    MessagePacket newMessage = Converter.ToStruct<MessagePacket>(data);
                    if (!clients.ContainsKey(newMessage.senderName)) { continue; }
                    if (newMessage.senderName != client.Name) { continue; }

                    if (newMessage.content.StartsWith("/"))
                    {
                        CommandSerealizer commandSerealizer = new CommandSerealizer(this, client, newMessage.content);
                        commandQueue.Add(commandSerealizer);
                    }
                    else
                    {
                        PrintMessage(newMessage);
                    }

                    messages.Add(newMessage);
                    data = Converter.ToByteArray<MessagePacket>(newMessage);
                    SendToAll(newMessage);
                }

                foreach (int key in tempClientKeysForRemove) { tempClients.Remove(key); }
                foreach (CommandSerealizer cmd in commandQueue) { cmd.Parse(); }

                tempClientKeysForRemove.Clear();
                commandQueue.Clear();

                Thread.Yield();
            }
        }

        // Обработка ввода сообщения с клавиатуры
        // Ожидание в бесконечном цикле
        public void MessageInputProccess()
        {
            while (isWorking)
            {
                string text = Console.ReadLine();

                MessagePacket msg = new MessagePacket();
                msg.senderName = name;
                msg.content = text;
                msg.sendTimestamp = Utils.GetTimeNow();
                PrintMessage(msg);
                messages.Add(msg);
                SendToAll(msg);

                Thread.Yield();
            }
        }

        // Отправка нового сообщения всем
        public void SendToAll(MessagePacket msg)
        {
            foreach (ServerClient client in clients.Values) 
            { 
                SendToClient(msg, client); 
            }
        }

        public void SendToClient(MessagePacket msg, ServerClient client)
        {
            byte[] data = Converter.ToByteArray<MessagePacket>(msg);
            client.socket.Send(data);
        }

        // Отправка НОВОМУ пользователю старых сообщений
        public void SendAllMessages(ServerClient client)
        {
            foreach(MessagePacket msg in messages)
            {
                SendToClient(msg, client);
            }
        }

        // Переименование пользователя
        public override void RenameClient(CommandExecutor ex, string newName)
        {
            ServerClient client = (ServerClient) ex;
            if (newName.Equals(client.Name)) { return; }

            Console.WriteLine("[Server] " + client.Name + " renamed to " + newName);

            clients.Remove(client.Name);
            client.Name = newName;
            clients.Add(newName, client);
        }

        // "Действие"
        public override void SendClientAction(CommandExecutor ex, string action)
        {
            Console.WriteLine("[*] " + action + " by " + ex.Name);
        }
    }


    internal class ServerClient : CommandExecutor
    {
        private int id = -1;


        public int ID { get { return id; } }

        public Socket socket { get; set; }

        public ServerClient(int id, Socket socket, string name)
        {
            this.id = id;
            this.socket = socket;
            this.name = name;
        }

        public override void RenameClient(CommandExecutor sender, string newName) {}
        public override void SendClientAction(CommandExecutor sender, string action) {}
    }
}
