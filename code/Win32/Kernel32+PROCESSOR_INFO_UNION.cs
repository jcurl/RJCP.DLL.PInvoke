namespace RJCP.Native.Win32
{
    using System.Runtime.InteropServices;

    internal partial class Kernel32
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct PROCESSOR_INFO_UNION
        {
            [FieldOffset(0)]
            internal uint dwOemId;
            [FieldOffset(0)]
            internal OSArchitecture wProcessorArchitecture;
            [FieldOffset(2)]
            internal ushort wReserved;
        }
    }
}
