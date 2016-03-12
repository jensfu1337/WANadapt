using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Commands
{
    public sealed class Message : CommandBase
    {
        public Message()
        {
            this.Type = CommandType.Message;
        }
    }

    public sealed class Popup : CommandBase
    {
        public Popup()
        {
            this.Type = CommandType.Popup;
        }
    }
}
