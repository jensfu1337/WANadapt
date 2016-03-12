using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Network
{
    public static class Constants
    {
        // Registered client port range
        public const ushort PortMin = 1024;
        public const ushort PortMax = 49151;
        public const ushort DefaulPort = 8889;
        public const byte MaxClients = 10;
        public const ushort BufferSize = 1024;

        // Settings for ping operations
        public const int TimeOut = 1000;
        public const int TimeToLive = 50;
    }
}
