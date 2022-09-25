using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp
{
    internal class CommandSerealizer
    {
        private CommandExecutor executor;
        private CommandExecutor sender;
        private string command;
        private string[] args;

        public CommandSerealizer(CommandExecutor executor, CommandExecutor sender, string command)
        {
            this.executor = executor;
            this.sender = sender;
            this.command = RemoveSpaces(command.ToLower().Substring(1));
            this.args = this.command.Split(' ');
        }

        public string RemoveSpaces(string str)
        {
            str = str.Trim().Replace("\n", "").Replace("\t", "");
            while (str.Contains("  ")) { str = str.Replace("  ", " "); }
            return str;
        }

        public void Parse()
        {
            int count = args.Length;

            if (count >= 2)
            {
                if (command.StartsWith("setname") 
                    || command.StartsWith("name") 
                    || command.StartsWith("newname") 
                    || command.StartsWith("rename"))
                {
                    string newName = args[1];
                    executor.RenameClient(sender, newName);
                    return;
                }

                if (command.StartsWith("action") || command.StartsWith("me"))
                {
                    string action = String.Join(" ", args.Skip(1));
                    executor.SendClientAction(sender, action);
                    return;
                }
            }
        }
    }
}
