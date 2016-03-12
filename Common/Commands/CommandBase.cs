using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Commands
{
    public enum CommandType
    {
        Message,
        Popup,
        Null
    }

    [Serializable]
    public abstract class CommandBase
    {
        public CommandType Type { get; protected set; } = CommandType.Null;
        protected byte[] Data { get; set; }

        public CommandBase()
        {
        }
    }
}
