using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace Common.Network
{
    public static class Utils
    {
        /// <summary>
        /// Checking wheter given port is in registered client port range
        /// To be enhanced: check whether port in use
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool IsPortValid(ushort port)
        {
            // to be enhanced: check wheter port is already in use / blocked
            if (port >= Constants.PortMin && port <= Constants.PortMax)
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
            var pingers = new Ping[255];
            var pingOpt = new PingOptions(Constants.TimeToLive, true);
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
                pingers[i].SendAsync(baseIP + (i+1).ToString(), Constants.TimeOut, Encoding.ASCII.GetBytes("huehue"), pingOpt);
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
            var pingOpt = new PingOptions(Constants.TimeToLive, true);
            IPAddress pingIP;
            PingReply reply;

            try
            {
                pingIP = IPAddress.Parse(ipAddress);
                reply = ping.Send(pingIP, Constants.TimeOut, Encoding.ASCII.GetBytes("huehue"), pingOpt);

                return (reply.Status == IPStatus.Success);
            }
            catch(Exception)
            {
                return false;
            }
        }
    }
}