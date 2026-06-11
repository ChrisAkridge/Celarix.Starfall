// See https://aka.ms/new-console-template for more information
using Celarix.Starfall.Playground.Presentations;

namespace Celarix.Starfall.Playground
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Please provide a presentation name as an argument.");
                return;
            }

            var presentationName = args[0].ToLowerInvariant();

            if (presentationName == "skiatk")
            {
                SkiaTkCurrent.Run();
            }
            else if (presentationName == "atria")
            {
                AtriaCurrent.Run();
            }
            else if (presentationName == "atriapng")
            {
                AtriaPngCurrent.Run();
            }
            else
            {
                Console.WriteLine("Unknown presentation name.");
            }
        }
    }
}