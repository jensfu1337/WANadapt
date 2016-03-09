using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace Common
{
    public static class Utils
    {
        /// <summary>
        /// Set window title and draw it in a box
        /// </summary>
        /// <param name="title"></param>
        public static void SetConsoleTitle(string title)
        {
            if (title.Length < 1)
                return;
            
            Console.Title = title;

            int width = Console.WindowWidth - 1;
            string paddingFull = string.Empty.PadLeft(width, '-');
            string paddingTitle = title.PadLeft(width / 2, '-');
            string paddingRest = string.Empty.PadLeft((width - paddingTitle.Length), '-');

            Console.WriteLine(paddingFull);
            Console.WriteLine(paddingTitle + paddingRest);
            Console.WriteLine(paddingFull);       
            Console.WriteLine();
        }
    }
}
