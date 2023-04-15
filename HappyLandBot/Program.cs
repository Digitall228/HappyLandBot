using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace HappyLandBot
{
    public class Program
    {
        public static HLClient client;
        static void Main(string[] args)
        {
            client = new HLClient("login", "password");

            HLBot bot1 = new HLBot(client);
            Thread.Sleep(5000);

            Task.Run(() => bot1.Monitoring());

            Console.ReadLine();
        }
    }
}
