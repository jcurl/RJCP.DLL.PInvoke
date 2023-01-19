namespace RJCP.Native.Win32
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;
    using Microsoft.Win32.SafeHandles;
    using SafeHandles;

    [SuppressUnmanagedCodeSecurity]
    internal static partial class Kernel32
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool CancelIo(SafeFileHandle hFile);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, EntryPoint = "CreateFileW")]
        public static extern SafeFileHandle CreateFile(string fileName,
            ACCESS_MASK access, FileShare share, IntPtr securityAttributes, CreationDisposition creationDisposition,
            CreateFileFlags flagsAndAttributes, IntPtr templateFile);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool GetFileInformationByHandle(SafeFileHandle hFile, out BY_HANDLE_FILE_INFORMATION fileInfo);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool GetFileInformationByHandleEx(SafeFileHandle hFile, FILE_INFO_BY_HANDLE_CLASS infoClass, out FILE_ID_INFO fileIdInfo, int dwBufferSize);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern FileType GetFileType(SafeFileHandle hFile);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern void GetNativeSystemInfo(out SYSTEM_INFO lpSystemInfo);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool GetOverlappedResult(SafeFileHandle hFile,
           [In] ref NativeOverlapped lpOverlapped, out uint lpNumberOfBytesTransferred, bool bWait);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, EntryPoint = "GetVersionExW")]
        public static extern bool GetVersionEx([In, Out] OSVERSIONINFO osVersionInfo);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, EntryPoint = "GetVersionExW")]
        public static extern bool GetVersionEx([In, Out] OSVERSIONINFOEX osVersionInfoEx);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern bool IsWow64Process(IntPtr hProcess, out bool wow64);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, EntryPoint = "LoadLibraryW")]
        public static extern SafeLibraryHandle LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, EntryPoint = "LoadLibraryExW")]
        public static extern SafeLibraryHandle LoadLibraryEx(string lpFileName, IntPtr hReservedNull, LoadLibraryFlags dwFlags);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool ReadFile(SafeFileHandle hFile, IntPtr lpBuffer,
            uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, ref NativeOverlapped lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool WriteFile(SafeFileHandle hFile, IntPtr lpBuffer,
            uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, ref NativeOverlapped lpOverlapped);

        #region Serial Port
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool ClearCommBreak(SafeFileHandle hFile);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool ClearCommError(SafeFileHandle hFile, out ComStatErrors lpErrors, IntPtr lpStat);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool ClearCommError(SafeFileHandle hFile, out ComStatErrors lpErrors, out COMSTAT lpStat);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool EscapeCommFunction(SafeFileHandle hFile, ExtendedFunctions dwFunc);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool GetCommMask(SafeFileHandle hFile, out SerialEventMask lpEvtMask);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool GetCommModemStatus(SafeFileHandle hFile, out ModemStat lpModemStat);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool GetCommProperties(SafeFileHandle hFile, out CommProp lpCommProp);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool GetCommState(SafeFileHandle hFile, ref DCB lpDCB);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool GetCommTimeouts(SafeFileHandle hFile, out COMMTIMEOUTS lpCommTimeouts);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool PurgeComm(SafeFileHandle hFile, PurgeFlags dwFlags);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool SetCommBreak(SafeFileHandle hFile);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool SetCommMask(SafeFileHandle hFile, SerialEventMask dwEvtMask);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool SetCommState(SafeFileHandle hFile, [In] ref DCB lpDCB);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool SetCommTimeouts(SafeFileHandle hFile, [In] ref COMMTIMEOUTS lpCommTimeouts);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool SetupComm(SafeFileHandle hFile, int dwInQueue, int dwOutQueue);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool WaitCommEvent(SafeFileHandle hFile, out SerialEventMask lpEvtMask,
            [In] ref NativeOverlapped lpOverlapped);
        #endregion
    }
}
