using System;

namespace Common
{
    public static class Utils
    {
        public static void SetTitle(string title)
        {
            if (title.Length < 1)
                return;
            
            Console.Title = title;
            Console.WriteLine(title);

            for (int i = 0; i < title.Length; i++)
                Console.Write("-");

            Console.WriteLine();
        }

        //The commands for interaction between the server and the client
        public enum Command 
        {
            //Log into the server
            Login,
            //Logout of the server
            Logout,
            //Send a text message to all the chat clients     
            Message,
            //Get a list of users in the chat room from the server
            List,
            //Empty
            Null
        }
    }
}
