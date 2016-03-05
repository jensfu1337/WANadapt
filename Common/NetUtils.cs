using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

// Credits to K3N @ http://stackoverflow.com/a/13493419

namespace Common
{
    public static class NetUtils
    {
        // Registered client port range
        private const ushort PORT_MIN = 1024;
        private const ushort PORT_MAX = 49151;

        private const ushort PING_RANGE = 255;
        private static int timeOut = 500;
        private static int timeToLive = 5;
        private static byte[] data = Encoding.ASCII.GetBytes("PingMe");
        
        public static bool IsPortValid(ushort port)
        {
            // To be enhanced...
            if (port >= PORT_MAX && port <= PORT_MAX)
                return true;

            return false;
        }   

        /// <summary>
        /// Get available IP addresses of local network in class D range
        /// based on own local IP address using Ping
        /// </summary>
        /// <returns>List of available IP addresses</returns>
        public static List<IPAddress> GetLocalClassDRangeIPs()
        {
            // Get local IP and prepare base IP
            string localIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1].ToString();
            string baseIP = localIP.Replace(localIP.ToString().Split('.')[3], "");
            // Ping object and output list for available addresses
            var pingers = new Ping[PING_RANGE];
            var pingOpt = new PingOptions(timeToLive, true);
            var adrList = new List<IPAddress>();
            // Used for threading
            var @lock = new object();
            var instances = 0;
            var wait = new SpinWait();           

            // Start pinging
            for (int i = 0; i < 255; i++)
            {
                pingers[i] = new Ping();
                // Set event using lamda
                pingers[i].PingCompleted += (s, e) =>
                {
                    lock (@lock)
                    {
                        instances -= 1;
                    }
                    // Add available IPs to list
                    if (e.Reply.Status == IPStatus.Success)
                    {
                        adrList.Add(e.Reply.Address);
                    }
                };
                // Increment instances and start asynchronous Ping
                lock (@lock)
                {
                    instances += 1;
                }
                pingers[i].SendAsync(baseIP + (i+1).ToString(), timeOut, data, pingOpt);
            }
            // Wait for all instances to be finished
            while (instances > 0)
            {
                wait.SpinOnce();
            }
            
            return adrList;
        }

        /// <summary>
        /// Check availability of single IP address
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns>True = available</returns>
        public static bool IsAlive(string ipAddress)
        {
            var ping = new Ping();
            var pingOpt = new PingOptions(timeToLive, true);
            IPAddress pingIP;
            PingReply reply;
            var isAlive = false;

            try
            {
                pingIP = IPAddress.Parse(ipAddress);
                reply = ping.Send(pingIP, timeOut, data, pingOpt);
                isAlive = (reply.Status == IPStatus.Success);
            }
            catch(Exception)
            {
                Console.WriteLine("Invalid IP address entered...");
            }            

            return isAlive;
        }
    }
}