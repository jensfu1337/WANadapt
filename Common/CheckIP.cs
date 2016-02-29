using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

// Credits to K3N @ http://stackoverflow.com/a/13493419

namespace Common
{
    public static class CheckIP
    {
        private static int timeOut = 1;
        private static int ttl = 5;
        private static byte[] data = Encoding.ASCII.GetBytes("PingTesterinho");
        private static int instances = 0;

        // used for GetLocalClassDRangeIPs()
        private static object @lock;
        private static List<IPAddress> ipAddresses;

        public static List<IPAddress> GetLocalClassDRangeIPs()
        {
            // get IP of local computer
            IPAddress baseIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1];
            // remove last part
            string workIP = baseIP.ToString().Replace(baseIP.ToString().Split('.')[3], "");
            // list with objects
            Dictionary<string, Ping> pingers = new Dictionary<string, Ping>();
            ipAddresses = new List<IPAddress>();
            // threading
            @lock = new object();            
            SpinWait wait = new SpinWait();
            // options
            PingOptions po = new PingOptions(ttl, true);

            // generate IP range
            for (int i = 1; i <= 255; i++)
            {
                Ping p = new Ping();
                // set event
                p.PingCompleted += Ping_completed;
                // add to dictionary
                pingers.Add(workIP + i.ToString(), p);
            }

            // start asynchronous pings
            foreach (KeyValuePair<string, Ping> p in pingers)
            {
                lock (@lock)
                {
                    instances += 1;
                }
                p.Value.SendAsync(p.Key, timeOut, data, po);
            }

            while (instances > 0)
            {
                wait.SpinOnce();
            }

            foreach (KeyValuePair<string, Ping> p in pingers)
            {
                p.Value.PingCompleted -= Ping_completed;
                p.Value.Dispose();
            }
            
            return ipAddresses;
        }

        private static void Ping_completed(object s, PingCompletedEventArgs e)
        {
            lock (@lock)
            {
                instances -= 1;
            }

            if (e.Reply.Status == IPStatus.Success)
            {
                ipAddresses.Add(e.Reply.Address);
            }
        }
    }
}