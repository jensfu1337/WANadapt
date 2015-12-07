using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class CInit
    {
        public static void InitConsole(string title)
        {
            if(title.Length > 0)
            {
                Console.Title = "test";
            }
        }        
    }
}
