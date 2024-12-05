using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageServer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ChatServer.StartServer().Wait();
        }
    }
}
