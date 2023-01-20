namespace RJCP.Native.Win32
{
    using System.Runtime.InteropServices;

    internal static partial class Kernel32
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class OSVERSIONINFOEX
        {
            public int OSVersionInfoSize;
            public int MajorVersion;
            public int MinorVersion;
            public int BuildNumber;
            public WinPlatformId PlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x80)]
            public string CSDVersion;
            public ushort ServicePackMajor;
            public ushort ServicePackMinor;
            private ushort wSuiteMask;
            private byte bProductType;
            public byte Reserved;

            // The value here is defined as 16-bit, but there are up to 32-bits defined in NT. Not sure where they are
            // exposed (SharedUserData->SuiteMask).
            public WinSuite SuiteMask => (WinSuite)wSuiteMask;

            public WinProductType ProductType => (WinProductType)bProductType;

            public OSVERSIONINFOEX()
            {
                OSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX));
            }
        }
    }
}