using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyLandBot
{
    public static class Logger
    {
        public delegate void Log(string text, ConsoleColor color = ConsoleColor.White);
        public static Log logAdd;

        private static object locker { get; set; }

        static Logger()
        {
            locker = new object();
            logAdd = LogAdd;
        }

        private static void LogAdd(string text, ConsoleColor color = ConsoleColor.White)
        {
            lock(locker)
            {
                Console.ForegroundColor = color;
                Console.WriteLine($"{DateTime.UtcNow}: {text}");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        public static void UpdateTitle(string title)
        {
            Console.Title = title;
        }
    }
}
