namespace RJCP.Native.Win32.DbgHelp
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using NUnit.Framework;
    using Win32;

    [TestFixture]
    [Platform(Include = "Win32")]
    public class MiniDumpTest
    {
        private static readonly string DumpPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "minidump.dmp");

        private static FileStream OpenFile(string path)
        {
            if (File.Exists(path)) {
                return File.Open(path, FileMode.Append);
            } else {
                return File.Create(path);
            }
        }

#if !NETFRAMEWORK
        private static IntPtr GetExceptionPointers()
        {
            Type marshal = typeof(Marshal);
            MethodInfo pointers = marshal.GetMethod("GetExceptionPointers");
            if (pointers != null)
                return (IntPtr)pointers.Invoke(null, null);

            return IntPtr.Zero;
        }
#endif

        [Test]
        public void WriteMiniDump()
        {
            bool result;
            using (FileStream fsToDump = OpenFile(DumpPath)) {
#if NETFRAMEWORK
                if (Environment.OSVersion.Version.Major <= 5) {
                    // Windows XP - no exception information, as this otherwise results in zero sized core dumps
                    result = DbgHelp.MiniDumpWriteDump(
                        Kernel32.GetCurrentProcess(),
                        Kernel32.GetCurrentProcessId(),
                        fsToDump.SafeFileHandle,
                        DbgHelp.MINIDUMP_TYPE.MiniDumpNormal,
                        IntPtr.Zero,
                        IntPtr.Zero,
                        IntPtr.Zero);
                } else {
                    DbgHelp.MINIDUMP_EXCEPTION_INFORMATION miniDumpInfo =
                    new DbgHelp.MINIDUMP_EXCEPTION_INFORMATION {
                        ClientPointers = 0,
                        ExceptionPointers = Marshal.GetExceptionPointers(),
                        ThreadId = Kernel32.GetCurrentThreadId()
                    };
                    result = DbgHelp.MiniDumpWriteDump(
                        Kernel32.GetCurrentProcess(),
                        Kernel32.GetCurrentProcessId(),
                        fsToDump.SafeFileHandle,
                        DbgHelp.MINIDUMP_TYPE.MiniDumpNormal,
                        ref miniDumpInfo,
                        //IntPtr.Zero,
                        IntPtr.Zero,
                        IntPtr.Zero);
                }
#else
                DbgHelp.MINIDUMP_EXCEPTION_INFORMATION miniDumpInfo =
                    new DbgHelp.MINIDUMP_EXCEPTION_INFORMATION {
                        ClientPointers = 0,
                        ExceptionPointers = GetExceptionPointers(),
                        ThreadId = Kernel32.GetCurrentThreadId()
                    };

                result = DbgHelp.MiniDumpWriteDump(
                    Kernel32.GetCurrentProcess(),
                    Kernel32.GetCurrentProcessId(),
                    fsToDump.SafeFileHandle,
                    DbgHelp.MINIDUMP_TYPE.MiniDumpNormal,
                    ref miniDumpInfo,
                    IntPtr.Zero,
                    IntPtr.Zero);
#endif
            }
            Assert.That(result, Is.True);
        }
    }
}
