using System;

namespace GreedyKid
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {

#if XBOXONE || PLAYSTATION4
        public const bool RunningOnConsole = true;
#else
        public static bool RunningOnConsole = false;
#endif

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var game = new GreedyKidGame())
                game.Run();
        }
    }
}
