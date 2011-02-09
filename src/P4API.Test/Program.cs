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
                c.Connect();
            }

            return 0;
        }

    }

}
