namespace RJCP.Native.Win32.User32
{
    using System;
    using NUnit.Framework;
    using Win32;

    [TestFixture]
    [Platform(Include = "Win32")]
    public class SystemMetrics
    {
        [Test]
        public void GetSystemMetrics()
        {
            int xscreen = User32.GetSystemMetrics(User32.SYSTEM_METRICS.SM_CXSCREEN);
            int yscreen = User32.GetSystemMetrics(User32.SYSTEM_METRICS.SM_CYSCREEN);

            Console.WriteLine($"Screen: {xscreen}x{yscreen}");
        }
    }
}
