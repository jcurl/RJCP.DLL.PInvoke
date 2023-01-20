namespace RJCP.Native.Win32.Kernel
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using Microsoft.Win32.SafeHandles;
    using NUnit.Framework;

    [TestFixture]
    [Platform(Include = "Win32")]
    public class File
    {
        private static readonly string TextFileName = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestResource", "TextFile.txt");

        [Test]
        public void CreateFile()
        {
            SafeFileHandle file = Kernel32.CreateFile(TextFileName, 0,
                Kernel32.FileShare.FILE_SHARE_READ | Kernel32.FileShare.FILE_SHARE_WRITE | Kernel32.FileShare.FILE_SHARE_DELETE,
                IntPtr.Zero, Kernel32.CreationDisposition.OPEN_EXISTING,
                Kernel32.CreateFileFlags.FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);
            Assert.That(file.IsInvalid, Is.False);
            file.Close();
        }

        [Test]
        public void CreateFileAccessMaskGenericRead()
        {
            SafeFileHandle file = Kernel32.CreateFile(TextFileName, Kernel32.ACCESS_MASK.GenericRight.GENERIC_READ,
                Kernel32.FileShare.FILE_SHARE_READ | Kernel32.FileShare.FILE_SHARE_WRITE | Kernel32.FileShare.FILE_SHARE_DELETE,
                IntPtr.Zero, Kernel32.CreationDisposition.OPEN_EXISTING,
                Kernel32.CreateFileFlags.FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);
            Assert.That(file.IsInvalid, Is.False);
            file.Close();
        }

        [Test]
        public void GetFileInformationByHandle()
        {
            SafeFileHandle file = Kernel32.CreateFile(TextFileName, Kernel32.ACCESS_MASK.GenericRight.GENERIC_READ,
                Kernel32.FileShare.FILE_SHARE_READ | Kernel32.FileShare.FILE_SHARE_WRITE | Kernel32.FileShare.FILE_SHARE_DELETE,
                IntPtr.Zero, Kernel32.CreationDisposition.OPEN_EXISTING,
                Kernel32.CreateFileFlags.FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);
            Assert.That(file.IsInvalid, Is.False);

            try {
                bool result = Kernel32.GetFileInformationByHandle(file, out Kernel32.BY_HANDLE_FILE_INFORMATION fileInfoByHandle);
                Assert.That(result, Is.True);

                Assert.That(fileInfoByHandle.NumberOfLinks, Is.EqualTo(1));
                Console.WriteLine($"Links: {fileInfoByHandle.NumberOfLinks}");
                Console.WriteLine($"Volume Serial Number: {fileInfoByHandle.VolumeSerialNumber:x8}");
                Console.WriteLine($"Attributes: {fileInfoByHandle.FileAttributes:x8}");
                Console.WriteLine($"Index: {fileInfoByHandle.FileIndexHigh:x8}:{fileInfoByHandle.FileIndexLow:x8}");
                Console.WriteLine($"Size: {fileInfoByHandle.FileSizeHigh:x8}:{fileInfoByHandle.FileSizeLow:x8}");
                Console.WriteLine($"Creation: {fileInfoByHandle.CreationTime.DateTime}");
                Console.WriteLine($"Modify: {fileInfoByHandle.LastWriteTime.DateTime}");
                Console.WriteLine($"Access: {fileInfoByHandle.LastAccessTime.DateTime}");
            } finally {
                file.Close();
            }
        }

        [Test]
        public void GetFileInformationByHandleEx_FileIdInfo()
        {
            SafeFileHandle file = Kernel32.CreateFile(TextFileName, Kernel32.ACCESS_MASK.GenericRight.GENERIC_READ,
                Kernel32.FileShare.FILE_SHARE_READ | Kernel32.FileShare.FILE_SHARE_WRITE | Kernel32.FileShare.FILE_SHARE_DELETE,
                IntPtr.Zero, Kernel32.CreationDisposition.OPEN_EXISTING,
                Kernel32.CreateFileFlags.FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);
            Assert.That(file.IsInvalid, Is.False);

            try {
#if NET40
                bool result = Kernel32.GetFileInformationByHandleEx(file, Kernel32.FILE_INFO_BY_HANDLE_CLASS.FileIdInfo,
                    out Kernel32.FILE_ID_INFO fileInfoEx, Marshal.SizeOf(typeof(Kernel32.FILE_ID_INFO)));
#else
                bool result = Kernel32.GetFileInformationByHandleEx(file, Kernel32.FILE_INFO_BY_HANDLE_CLASS.FileIdInfo,
                    out Kernel32.FILE_ID_INFO fileInfoEx, Marshal.SizeOf<Kernel32.FILE_ID_INFO>());
#endif
                Assert.That(result, Is.True);

                // This serial number is not the same as with GetFileInformationByHandle
                Console.WriteLine($"Volume Serial Number: {fileInfoEx.VolumeSerialNumber:x8}");

                // These numbers are 64-bit, as opposed to the other which is 32-bit
                Console.WriteLine($"Index: {fileInfoEx.FileIdHigh:x16}:{fileInfoEx.FileIdLow:x16}");
            } finally {
                file.Close();
            }
        }
    }
}
