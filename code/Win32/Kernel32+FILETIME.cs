namespace RJCP.Native.Win32
{
    using System;
    using System.Runtime.InteropServices;

    internal static partial class Kernel32
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct FILETIME
        {
            public uint DateTimeLow;
            public uint DateTimeHigh;

            public DateTime DateTime => DateTime.FromFileTime(((long)DateTimeHigh << 32) | DateTimeLow);
        }
    }
}
