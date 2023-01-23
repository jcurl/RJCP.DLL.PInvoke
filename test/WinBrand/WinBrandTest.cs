namespace RJCP.Native.Win32.WinBrand
{
    using System;
    using NUnit.Framework;
    using Win32;

    [TestFixture]
    [Platform(Include = "Win32")]
    internal class WinBrandTest
    {
        [Test]
        public void WinBrand_Long()
        {
            string release = WinBrand.BrandingFormatString("%WINDOWS_LONG%");
            Assert.That(release, Is.Not.Null.Or.Empty);
            Console.WriteLine($"Windows Brand: {release}");
        }
    }
}
