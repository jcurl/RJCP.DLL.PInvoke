namespace RJCP.Native.Win32
{
    internal static partial class Kernel32
    {
        /// <summary>
        /// The processor architecture on which the machine is running.
        /// </summary>
        /// <remarks>
        /// This value is obtained from <c>SYSTEM_INFO.uProcessorInfo.wProcessorArchitecture</c>.
        /// </remarks>
        public enum OSArchitecture : ushort
        {
            /// <summary>
            /// Intel x86 32-bit.
            /// </summary>
            x86 = 0,

            /// <summary>
            /// MIPS architecture.
            /// </summary>
            Mips = 1,

            /// <summary>
            /// Alpha architecture (obsolete).
            /// </summary>
            Alpha = 2,

            /// <summary>
            /// Power PC architecture.
            /// </summary>
            PPC = 3,

            /// <summary>
            /// SH4 architecture.
            /// </summary>
            SHX = 4,

            /// <summary>
            /// ARM architecture.
            /// </summary>
            ARM = 5,

            /// <summary>
            /// Intel IA64 architecture (this is not x64).
            /// </summary>
            IA64 = 6,

            /// <summary>
            /// Alpha 64-bit architecture.
            /// </summary>
            Alpha64 = 7,

            /// <summary>
            /// Microsoft Intermediate Language.
            /// </summary>
            MSIL = 8,

            /// <summary>
            /// AMD64 bit instructions.
            /// </summary>
            x64 = 9
        }
    }
}
