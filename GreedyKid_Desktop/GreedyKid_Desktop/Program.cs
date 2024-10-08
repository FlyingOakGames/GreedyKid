﻿// Boo! Greedy Kid © 2017-2024 Flying Oak Games. All rights reserved.
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

#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
#endif

            using (var game = new GreedyKidGame())
                game.Run();
        }

#if !DEBUG
        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            Exception exception = unhandledExceptionEventArgs.ExceptionObject as Exception;
            if (exception != null)
            {

                SettingsManager.Instance.Delete();

                bool compat = false;
                // handle special errors
                if (exception.Message.Contains("GZIP") || (exception.InnerException != null && exception.InnerException.Message.Contains("GZIP")))
                {
                    compat = true;
                    SDL_ShowSimpleMessageBox((uint)SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Error", "It looks like some files are corrupted... Let's assume this is bad luck and try reinstalling the game (or removing the files in your save folder). :( If it happens again, feel free to drop by the Steam forum.", IntPtr.Zero);
                }
                else if (exception is Microsoft.Xna.Framework.Audio.NoAudioHardwareException || exception.Message.Contains("OpenAL") || exception.Message.Contains("No instance running") || exception.Message.Contains("audio context") || exception.StackTrace.Contains("OpenALSoundController"))
                {
                    compat = true;
                    SDL_ShowSimpleMessageBox((uint)SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Error", "Your sound driver is not compatible (or another software is blocking audio). Try updating it and verify that the audio output is well configured (and connected). In case of doubt, feel free to drop by the Steam forum.", IntPtr.Zero);
                }
                else if (exception is Microsoft.Xna.Framework.Graphics.NoSuitableGraphicsDeviceException || exception.Message.Contains("ARB_framebuffer_object") || exception.Message.Contains("Shader") || exception.Message.Contains("effect") || exception.Message.Contains("OpenGL") || exception.StackTrace.Contains("FramebufferHelperEXT") || (exception.InnerException != null && exception.InnerException.Message.Contains("OpenGL")))
                {
                    compat = true;
                    SDL_ShowSimpleMessageBox((uint)SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Error", "Your graphics driver is not compatible. Try updating it and verify that it meets the OpenGL 3.0 requirement. In case of doubt, feel free to drop by the Steam forum.", IntPtr.Zero);
                }

                ReportCrash(exception, compat);
            }
            Environment.Exit(0);
        }

        [System.Runtime.InteropServices.DllImport("SDL2.dll", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, EntryPoint = "SDL_ShowSimpleMessageBox")]
        public static extern int SDL_ShowSimpleMessageBox(uint flags, string title, string message, IntPtr window);

        [Flags]
        public enum SDL_MessageBoxFlags : uint
        {
            SDL_MESSAGEBOX_ERROR = 0x00000010,
            SDL_MESSAGEBOX_WARNING = 0x00000020,
            SDL_MESSAGEBOX_INFORMATION = 0x00000040
        }

        public static void ReportCrash(Exception exception, bool compatibilityIssue)
        {
            SettingsManager.Instance.Delete();

            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            // write a local dump
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(System.IO.Path.Combine(SettingsManager.Instance.SaveDirectory, "crash.log"), true))
            {
                writer.WriteLine("----------------------- Boo! Greedy Kid crash log -----------------------" + Environment.NewLine + Environment.NewLine);
                writer.WriteLine("Date: " + DateTime.Now.ToString() + Environment.NewLine + Environment.NewLine);
                writer.WriteLine("Game version: " + version + Environment.NewLine + Environment.NewLine);
                writer.WriteLine("Operating system: " + SystemHelper.Name + Environment.NewLine + Environment.NewLine);
                writer.WriteLine("Exception Type: " + exception.GetType().ToString() + Environment.NewLine + Environment.NewLine);
                writer.WriteLine("Message: " + exception.Message + Environment.NewLine + "StackTrace: " + Environment.NewLine + exception.StackTrace + Environment.NewLine + Environment.NewLine);
                if (exception.InnerException != null)
                {
                    writer.WriteLine("Inner exception Type: " + exception.InnerException.GetType().ToString() + Environment.NewLine + Environment.NewLine);
                    writer.WriteLine("Inner exception: " + exception.InnerException.Message + Environment.NewLine + "StackTrace: " + Environment.NewLine + exception.InnerException.StackTrace + Environment.NewLine + Environment.NewLine);
                }
                writer.WriteLine("---------------------------------------------------------------------" + Environment.NewLine + Environment.NewLine);
            }
        }
#endif
    }
}
