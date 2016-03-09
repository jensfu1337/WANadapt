using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Command
{
    public enum CommandType
    {
        Message,
        Popup,
        Exit,
        Null
    }

    public class Command
    {
        private byte[] _data;

        public CommandType Type { get; private set; } = CommandType.Null;
        public byte[] Data { get; private set; } = null;

        private Command()
        {
        }
    }
}
