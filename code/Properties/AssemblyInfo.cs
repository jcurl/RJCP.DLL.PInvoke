using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Setting ComVisible to false makes the types in this assembly not visible to COM components. If you need to access a
// type in this assembly from COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]

// It is not intended that this project will be consumed directly, so all the classes are internal. We expose testing
// only for our unit test project.
[assembly: InternalsVisibleTo("RJCP.Native.PInvokeTest")]
