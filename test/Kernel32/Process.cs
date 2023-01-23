namespace RJCP.Native.Win32.Kernel32
{
    using System;
    using NUnit.Framework;
    using Win32;

    [TestFixture]
    [Platform(Include = "Win32")]
    public class Process
    {
        [Test]
        public void GetCurrentProcess()
        {
            IntPtr result = Kernel32.GetCurrentProcess();
            Assert.That(result, Is.EqualTo(new IntPtr(-1)));
        }

        [Test]
        public void GetCurrentProcessId()
        {
            uint result = Kernel32.GetCurrentProcessId();
            Assert.That(result, Is.Not.EqualTo(0));
            Console.WriteLine($"Current Process ID = {result}");
        }

        [Test]
        public void GetCurrentThreadId()
        {
            uint result = Kernel32.GetCurrentThreadId();
            Assert.That(result, Is.Not.EqualTo(0));
            Console.WriteLine($"Current Thread ID = {result}");
        }
    }
}
