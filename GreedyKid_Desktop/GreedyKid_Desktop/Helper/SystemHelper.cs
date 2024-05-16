// Boo! Greedy Kid © 2017-2024 Flying Oak Games. All rights reserved.
using System;
using System.Diagnostics;
using System.IO;

namespace GreedyKid
{
    public static class SystemHelper
    {
        public static bool IsWindows { get; private set; }
        public static bool IsUnix { get; private set; }
        public static bool IsMac { get; private set; }
        public static bool IsLinux { get; private set; }
        public static bool IsUnknown { get; private set; }
        public static string Name { get; private set; }

        static SystemHelper()
        {
            IsWindows = Path.DirectorySeparatorChar == '\\';
            if (IsWindows)
            {
                Name = Environment.OSVersion.VersionString;

                Name = Name.Replace("  ", " ");
                Name = Name.Trim();

                if (Name.Contains("NT 5.1"))
                    Name = "Windows XP";
                else if (Name.Contains("NT 5.2"))
                    Name = "Windows XP 64 Bits";
                else if (Name.Contains("NT 6.0"))
                    Name = "Windows Vista";
                else if (Name.Contains("NT 6.1"))
                    Name = "Windows 7";
                else if (Name.Contains("NT 6.2"))
                    Name = "Windows 8";
                else if (Name.Contains("NT 6.3"))
                    Name = "Windows 8.1";
                else if (Name.Contains("NT 10.0"))
                {
                    if (Name.Contains("10.0.102"))
                        Name = "Windows 10";
                    else if (Name.Contains("10.0.105"))
                        Name = "Windows 10  (November 2015 Update)";
                    else if (Name.Contains("10.0.14"))
                        Name = "Windows 10  (Anniversary Update)";
                    else if (Name.Contains("10.0.15"))
                        Name = "Windows 10  (Creators Update)";
                    else if (Name.Contains("10.0.16"))
                        Name = "Windows 10  (Fall Creators Update)";
                    else if (Name.Contains("10.0.17"))
                        Name = "Windows 10  (Redstone 4)";
                    else
                        Name = "Windows 10 (Unknown version)";
                }

                if (Name.Contains("Service Pack 1"))
                    Name = Name + " (SP1)";
                else if (Name.Contains("Service Pack 2"))
                    Name = Name + " (SP2)";
                else if (Name.Contains("Service Pack 3"))
                    Name = Name + " (SP3)";
            }
            else
            {
                string UnixName = ReadProcessOutput("uname");
                if (UnixName.Contains("Darwin"))
                {
                    IsUnix = true;
                    IsMac = true;

                    Name = "MacOS " + ReadProcessOutput("sw_vers", "-productVersion");
                    Name = Name.Trim();
                }
                else if (UnixName.Contains("Linux"))
                {
                    IsUnix = true;
                    IsLinux = true;

                    Name = ReadProcessOutput("lsb_release", "-d");
                    Name = Name.Substring(Name.IndexOf(":") + 1);
                    Name = Name.Trim();
                }
                else if (UnixName != "")
                {
                    IsUnix = true;
                }
                else
                {
                    IsUnknown = true;
                }
            }
        }

        private static string ReadProcessOutput(string name)
        {
            return ReadProcessOutput(name, null);
        }

        private static string ReadProcessOutput(string name, string args)
        {
            try
            {
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                if (args != null && args != "") p.StartInfo.Arguments = " " + args;
                p.StartInfo.FileName = name;
                p.Start();
                // Do not wait for the child process to exit before
                // reading to the end of its redirected stream.
                // p.WaitForExit();
                // Read the output stream first and then wait.
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                if (output == null) output = "";
                output = output.Trim();
                return output;
            }
            catch
            {
                return "";
            }
        }
    }
}
