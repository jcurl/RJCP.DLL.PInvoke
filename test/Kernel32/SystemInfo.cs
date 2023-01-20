namespace RJCP.Native.Win32.Kernel
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    [Platform(Include = "Win32")]
    public class SystemInfo
    {
        [Test]
        public void GetProductInfo()
        {
            bool result = Kernel32.GetProductInfo(6, 1, 0, 0, out Kernel32.WinProductInfo productInfo);
            Assert.That(result, Is.True);

            Console.WriteLine($"Product Info: {productInfo}");
        }

        [Test]
        public void GetVersionInfo()
        {
            int version = Kernel32.GetVersion();
            Assert.That(version, Is.Not.EqualTo(0));

            bool winNT = (version & 0x80000000) == 0;
            int majorVersion = version & 0xFF;
            int minorVersion = (version & 0xFF00) >> 8;
            int buildNumber = (version & 0x7FFF0000) >> 16;

            Console.WriteLine($"Version: {version:x8}; WinNT={winNT}, {majorVersion}.{minorVersion}.{buildNumber}");
        }

        [Test]
        public void GetVersionEx_Simple()
        {
            Kernel32.OSVERSIONINFO versionInfo = new Kernel32.OSVERSIONINFO();
            bool result = Kernel32.GetVersionEx(versionInfo);
            Assert.That(result, Is.True);

            Console.WriteLine($"Version: {versionInfo.MajorVersion}.{versionInfo.MinorVersion}.{versionInfo.BuildNumber}");
            Console.WriteLine($"Platform: {versionInfo.PlatformId}");
            Console.WriteLine($"CSDVersion: {versionInfo.CSDVersion}");
        }

        [Test]
        public void GetVersionEx_Extended()
        {
            Kernel32.OSVERSIONINFOEX versionInfo = new Kernel32.OSVERSIONINFOEX();
            bool result = Kernel32.GetVersionEx(versionInfo);
            Assert.That(result, Is.True);

            Console.WriteLine($"Version: {versionInfo.MajorVersion}.{versionInfo.MinorVersion}.{versionInfo.BuildNumber}");
            Console.WriteLine($"Platform: {versionInfo.PlatformId}");
            Console.WriteLine($"CSDVersion: {versionInfo.CSDVersion}");
            Console.WriteLine($"Service Pack: {versionInfo.ServicePackMajor}.{versionInfo.ServicePackMinor}");
            Console.WriteLine($"Suite: {versionInfo.SuiteMask}");
            Console.WriteLine($"Product Type: {versionInfo.ProductType}");
        }

        [Test]
        public void GetSystemInfo()
        {
            Kernel32.GetNativeSystemInfo(out Kernel32.SYSTEM_INFO lpSystemInfo);

            Console.WriteLine($"Processor Mask: {lpSystemInfo.dwActiveProcessorMask:x8}");
        }

        [Test]
        public void IsWow64Process()
        {
            bool result = Kernel32.IsWow64Process(Kernel32.GetCurrentProcess(), out bool wow64);
            Assert.That(result, Is.True);

            Console.WriteLine($"Is WOW64: {wow64}");
        }

        [Test]
        public void IsWow64Process2()
        {
            bool result = Kernel32.IsWow64Process2(Kernel32.GetCurrentProcess(),
                out Kernel32.IMAGE_FILE_MACHINE machine, out Kernel32.IMAGE_FILE_MACHINE native);
            Assert.That(result, Is.True);

            Console.WriteLine($"Is WOW64: process {machine}; and native {native}");
        }
    }
}
