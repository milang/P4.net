using System;

namespace P4API.Test
{

    /// <summary>
    /// </summary>
    public static class Program
    {

        /// <summary>
        /// </summary>
        public static int Main(string[] args)
        {
            using (var c = new P4Connection())
            {
                try
                {
                    c.Connect();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(ex.GetType().FullName);
                    Console.Write(": ");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    Console.ResetColor();
                }
            }
            return 0;
        }

    }

}
