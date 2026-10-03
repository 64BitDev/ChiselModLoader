using ChiselModLoader.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiselModLoader.Runtime
{
    public class Logging
    {
        string Name = "Unkown";
        public Logging(string name)
        {
            Name = name;

        }
        public void LogDebug(string msg)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine($"[{Name}] {msg}");
        }

        public void LogError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine($"[{Name}] {msg}");
        }

        public void LogException(Exception exception)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine($"[{Name}] {exception.ToString()}");
        }
    }
}
