using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp
{
    abstract class CommandExecutor
    {
        protected string name;
        public string Name 
        { 
            get { return name; } 
            set { name = value; }
        }

        public abstract void RenameClient(CommandExecutor sender, string newName);
        public abstract void SendClientAction(CommandExecutor sender, string action);

        public void PrintMessage(MessagePacket msg)
        {
            DateTime time = Utils.UnixTimeStampToDateTime(msg.sendTimestamp);
            Console.WriteLine("[" + time.ToString("h:mm.ss") + "] " + msg.senderName + ": " + msg.content);
        }
    }
}
