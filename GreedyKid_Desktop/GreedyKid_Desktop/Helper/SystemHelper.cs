using System;
using System.Runtime.InteropServices;

namespace GreedyKid
{
    public static class SystemHelper
    {
        [DllImport("libc")]
        static extern int uname(IntPtr buf);

        public static bool IsRunningOnMac
        {
            get
            {
                IntPtr buf = IntPtr.Zero;
                try
                {
                    buf = Marshal.AllocHGlobal(8192);
                    // This is a hacktastic way of getting sysname from uname ()
                    if (uname(buf) == 0)
                    {
                        string os = Marshal.PtrToStringAnsi(buf);
                        if (os == "Darwin")
                            return true;
                    }
                }
                catch
                {
                }
                finally
                {
                    if (buf != IntPtr.Zero)
                        Marshal.FreeHGlobal(buf);
                }
                return false;
            }
        }
    }
}
