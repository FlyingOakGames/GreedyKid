// Boo! Greedy Kid © 2017-2024 Flying Oak Games. All rights reserved.
using GreedyKid.Helper;
using System;

namespace GreedyKid
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        public static bool RunningOnConsole = false;
        public static bool ForceWindowed = false;
        public static bool UnlockFPS = false;
        //public static bool EditorMode = false;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // manage hidden settings
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-emulateConsole")
                    RunningOnConsole = true;
                else if (args[i] == "-forceWindowed")
                    ForceWindowed = true;
                else if (args[i] == "-unlockFPS")
                    UnlockFPS = true;
                //else if (args[i] == "-editorMode")
                //    EditorMode = true;
            }
#if STEAM
            SteamworksReturn steam = SteamworksHelper.Instance.Init();
            if (steam == SteamworksReturn.RestartingThroughSteam)
                return;
#if !DEBUG
            else if (steam == SteamworksReturn.CantInit)
            {
                SDL_ShowSimpleMessageBox((uint)SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Error", "Steam could not be found. Try starting Steam before starting the game. If the error persists, please visit the Steam forum.", IntPtr.Zero);
                return;
            }
#endif

#endif

            using (var game = new GreedyKidGame())
                game.Run();
        }
    }
}
