# Guidelines to Marshalling with C# .NET 4.x to .NET Core 6.x <!-- omit in toc -->

This is a collection of documentation I've found on the web when writing my own
P/Invoke interop code.

Some text in this document may be copied from other sources verbatim, to try and
collect everything into a single source. A second reason for copying data
verbatim is to keep context in this document, as well as preserve the text at
the time it was copied (the Web is notorious for stale links and documentation
changes regularly as technology changes).

It is intended that the P/Invoke work from .NET 4.0 upwards using the
`[DllImport]` attribute. This may change in the future as .NET moves to AOT
technology.

There is a lot of information, finding it in one place is difficult. This
document will be updated over time as information is found and used.

- [1. About this Document](#1-about-this-document)
- [2. Code Style](#2-code-style)
  - [2.1. Compiler Version](#21-compiler-version)
- [3. Declaring P/Invoke Methods](#3-declaring-pinvoke-methods)
  - [3.1. Cross Platform P/Invoke Library Resolution](#31-cross-platform-pinvoke-library-resolution)
  - [3.2. Identifying a Windows 32-bit or 64-bit library](#32-identifying-a-windows-32-bit-or-64-bit-library)
    - [3.2.1. .NET 3.1 Library Resolution](#321-net-31-library-resolution)
  - [3.3. Win32 specific attributes](#33-win32-specific-attributes)
- [4. Marshalling Input and Output Parameters](#4-marshalling-input-and-output-parameters)
  - [4.1. Blittable Types](#41-blittable-types)
  - [4.2. Directional Parameters](#42-directional-parameters)
  - [4.3. Structs and Classes for In/Out parameters](#43-structs-and-classes-for-inout-parameters)
    - [4.3.1. Structs and In/Out parameters](#431-structs-and-inout-parameters)
    - [4.3.2. Classes and In/Out parameters](#432-classes-and-inout-parameters)
  - [4.4. Value Type Marshalling](#44-value-type-marshalling)
    - [4.4.1. Different Architectures](#441-different-architectures)
    - [4.4.2. Windows Value Types](#442-windows-value-types)
    - [4.4.3. Strings](#443-strings)
      - [4.4.3.1. Strings as Input Parameter](#4431-strings-as-input-parameter)
      - [4.4.3.2. Strings Copied into Output Buffers](#4432-strings-copied-into-output-buffers)
  - [4.5. Marshalling Structs](#45-marshalling-structs)
    - [4.5.1. Field Alignment](#451-field-alignment)
      - [4.5.1.1. Using C++ To Test Alignment (Example DbgHelp)](#4511-using-c-to-test-alignment-example-dbghelp)
    - [4.5.2. Blittable Strings (actually char\[\])](#452-blittable-strings-actually-char)
    - [4.5.3. Strings in Structs](#453-strings-in-structs)
    - [4.5.4. Fixed Size Buffers](#454-fixed-size-buffers)
  - [4.6. Memory Pinning](#46-memory-pinning)
    - [4.6.1. Memory Buffers for Asynchronous I/O](#461-memory-buffers-for-asynchronous-io)
      - [4.6.1.1. Native Overlapped](#4611-native-overlapped)
    - [4.6.2. Memory Buffers for Callbacks](#462-memory-buffers-for-callbacks)
    - [4.6.3. Garbage Collection](#463-garbage-collection)
  - [4.7. Safe Handles](#47-safe-handles)
- [5. Callbacks and Function Pointers](#5-callbacks-and-function-pointers)
- [6. Appendix](#6-appendix)
  - [6.1. References](#61-references)
  - [6.2. Abbreviations](#62-abbreviations)

## 1. About this Document

This document is maintained using Visual Studio Code, with help of the following
plugins:

- *Markdown All In One* - to maintain the contents tables and formatting.
- *Rewrap* to keep lines at 80 characters
- *Markdown Lint* to identify common problems with formatting
- Spell checkers for common English errors, using British English.
- Trailing Whitespace

## 2. Code Style

The code in this repository tries to adhere to [Native interoperability best
practices](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/best-practices)

- DO use the same naming and capitalisation for your methods and parameters as
  the native method you want to call.
  - This will require various global suppressions in code to suppress warnings
    about code style. These are maintained in the `GlobalSuppressions.cs` file.
- CONSIDER using the same naming and capitalisation for constant values.
  - Not everything will be identical, but close and human readable (for
    historical purposes with my other projects). If I update them in a project,
    I may update them here.
- DO use .NET types that map closest to the native type. For example, in C#, use
  `uint` when the native type is `unsigned int`.
- DO only use `[In]` and `[Out]` attributes when the behaviour you want differs
  from the default behaviour. See the section below on [Structs and In/Out
  Parameters](#InOutStructs)
- CONSIDER using `System.Buffers.ArrayPool<T>` to pool your native array
  buffers.
  - This is only available in .NET Standard 2.1 and later. Conditional
    compilation is needed for older frameworks.
- CONSIDER wrapping your P/Invoke declarations in a class with the same name and
  capitalisation as your native library.
  - This allows your `[DllImport]` attributes to use the C# `nameof` language
    feature to pass in the name of the native library and ensure that you didn't
    misspell the name of the native library.

### 2.1. Compiler Version

Not all features are available in all versions, here's a table from [Language
Versions](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/configure-language-version#defaults)

| Target         | Version | C# language version default |
| -------------- | ------- | --------------------------- |
| .NET           | 7.x     | C# 11                       |
| .NET           | 6.x     | C# 10                       |
| .NET           | 5.x     | C# 9.0                      |
| .NET Core      | 3.x     | C# 8.0                      |
| .NET Core      | 2.x     | C# 7.3                      |
| .NET Standard  | 2.1     | C# 8.0                      |
| .NET Standard  | 2.0     | C# 7.3                      |
| .NET Standard  | 1.x     | C# 7.3                      |
| .NET Framework | all     | C# 7.3                      |

## 3. Declaring P/Invoke Methods

### 3.1. Cross Platform P/Invoke Library Resolution

Described at [Cross-Platform
P/Invoke](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/cross-platform),
the behaviour of the following method is:

```csharp
[DllImport("nativedep")]
static extern int ExportedFunction();
```

When running on Windows, the DLL is searched for in the following order:

- As an already loaded image, If file name does not include a path and there is
  more than one loaded module with the same base name and extension, the
  function returns a handle to the module that was loaded first. See
  [LoadLibraryW](https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-loadlibraryw)
- `nativedep`
- `nativedep.dll` (if the library name does not already end with .dll or .exe)

When running on Linux or macOS, the runtime will try prepending lib and
appending the canonical shared library extension. On these OSes, library name
variations are tried in the following order:

- `nativedep.so` / `nativedep.dylib`
- `libnativedep.so` / `libnativedep.dylib` †
- `nativedep`
- `libnativedep` †

On Linux, the search order is different if the library name ends with `.so` or
contains `.so.` (note the trailing `.`). Consider the following example:

```csharp
[DllImport("nativedep.so.6")]
static extern int ExportedFunction();
```

In this case, the library name variations are tried in the following order:

- `nativedep.so.6`
- `libnativedep.so.6` †
- `nativedep.so.6.so`
- `libnativedep.so.6.so` †

† Path is checked only if the library name does not contain a directory
separator character (`/`).

### 3.2. Identifying a Windows 32-bit or 64-bit library

On Windows, the .NET runtime may run either 32-bit or 64-bit. It is not possible
to predict before the program has started which architecture it runs as. For
library writers, this is even more important.

We observe that Windows only searches for the library on disk if the image is
not already loaded into the current process space. We can use this to preload
the library, dependent if it is 32-bit or 64-bit.

For example, my [CPUID](https://github.com/jcurl/RJCP.DLL.CpuId/) library uses
this technique:

1. Ensure that the library exists twice, both for 32-bit and 64-bit, but it has
   the same name for both architecturs, but in different directories.
2. Determine the architecture, if it is 32-bit or 64-bit at run-time within the
   .NET project.
3. Load, using Kernel32 `LoadLibrary` routines, the correct library into the
   process space.

The reference to the library is automatically the same, no matter 32-bit or
64-bit, and so long as the library uses the same guidelines and types as
standard Win32 libraries, the same rules for 32-bit and 64-bit apply.

For example, the DLL declaration for both architectures is the same:

```csharp
[DllImport("cpuid.dll", ExactSpelling = true)]
public static extern int hascpuid();
```

The code to load the correct library into memory:

```csharp
private static SafeLibraryHandle m_CpuIdHandle;

private static void LoadLibrary() {
  if (m_CpuIdHandle == null)
    m_CpuIdHandle = Win32.LoadLibrary<WindowsCpuIdFactory>("cpuid.dll");

  if (m_CpuIdHandle.IsInvalid)
    throw new PlatformNotSupportedException("Cannot load platform specific libraries");
}

private static SafeLibraryHandle LoadLibrary<T>(string fileName) {
  Uri assemblyLocation = new Uri(typeof(T).Assembly.Location);
  string libraryPath = Path.GetDirectoryName(assemblyLocation.LocalPath);

  if (!Environment.Is64BitProcess) {
    libraryPath = Path.Combine(libraryPath, "x86", fileName);
  } else {
    libraryPath = Path.Combine(libraryPath, "x64", fileName);
  }

  return LoadLibraryEx(libraryPath, IntPtr.Zero, LoadLibraryFlags.None);
}
```

with support libraries:

```csharp
using Microsoft.Win32.SafeHandles;

internal class SafeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid {
  protected SafeLibraryHandle() : base(true) { }

  protected override bool ReleaseHandle() {
    return FreeLibrary(handle);
  }
}
```

Note, that the handle must be a `private static` that the `SafeLibraryHandle
m_CpuIdHandle` may not be finalized, resulting in a call to
[`FreeLibrary`](https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-freelibrary)
which would otherwise unmap the loaded library and remove it from the process
space memory.

#### 3.2.1. .NET 3.1 Library Resolution

With .NET Core, there's a new class `NativeLibrary` that can help resolve
loading particular libraries, as described by [Custom Import
Resolver](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/cross-platform#custom-import-resolver)

```csharp
namespace PInvokeSamples {
  using System;
  using System.Reflection;
  using System.Runtime.InteropServices;

  public static class Program {
    [DllImport("nativedep")]
    private static extern int ExportedFunction();

    public static void Main(string[] args) {
      // Register the import resolver before calling the imported function.
      // Only one import resolver can be set for a given assembly.
      NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), DllImportResolver);

      int value = ExportedFunction();
      Console.WriteLine(value);
    }

    private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath) {
      if (libraryName == "nativedep") {
        // On systems with AVX2 support, load a different library.
        if (System.Runtime.Intrinsics.X86.Avx2.IsSupported) {
          return NativeLibrary.Load("nativedep_avx2", assembly, searchPath);
        }
      }

      // Otherwise, fallback to default import resolver.
      return IntPtr.Zero;
    }
  }
}
```

using:

- [NativeLibrary.SetDllImportResolver](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.nativelibrary.setdllimportresolver)
- [NativeLibrary.Load](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.nativelibrary.load)

### 3.3. Win32 specific attributes

This section covers a check list to use when writing a `[DllImport]` attribute
declaration.

Generally consider as needed if the error code is needed.

- `SetLastError = true`. The default is `false`.

All the Win32 P/Invoke `[DllImport]` should use

- `ExactSpelling = true` - A minor performance optimisation, specific for Win32
  only where APIs are either ANSI or UTF-16.

For UTF-16 and ASCII APIs

- `EntryPoint = "xxxW"`
  - In case `ExactSpelling` is used, the P/Invoke method declaration uses the
    "normal" name without the -A or -W suffix. As such, the entry point must be
    defined.
  - This is only needed if the P/Invoke method declaration is different to the
    library entry point.
- `CharSet = CharSet.Unicode`
  - Don't use `CharSet.Auto`, which was originally designed for compatibility
    between Win9x and WinNT kernels. Unexpected behaviour may occur. On .NET
    Core 2.2 and earlier on Unix, it was UTF-16, on .NET Core 3.0 and later on
    Unix, it is UTF8. See [Charsets and
    Marshalling](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/charset)
  - By using `CharSet.Unicode` explicitly on Windows, it ensures the correct
    marshalling format for UTF-16 on input and output strings.
  - `CharSet = CharSet.Ansi` - Should be used for Unix platforms, which default
    to UTF-8 multi-byte sequences.

## 4. Marshalling Input and Output Parameters

### 4.1. Blittable Types

See [Native Interop Best Practices - Blittable
Types](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/best-practices#blittable-types).

Blittable types are types that have the same bit-level representation in managed
and native code. As such they do not need to be converted to another format to
be marshalled to and from native code, and as this improves performance they
should be preferred. Some types are not blittable but are known to contain
blittable contents. These types have similar optimisations as blittable types
when they are not contained in another type, but are not considered blittable
when in fields of `struct`s or for the purposes of
[UnmanagedCallersOnlyAttribute](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.unmanagedcallersonlyattribute).

The table in the next section shows blittable intrinsic types.

In addition, the following are also blittable:

- `struct`s with fixed layout that only have blittable value types for instance
  fields
  - fixed layout requires `[StructLayout(LayoutKind.Sequential)]` or
    `[StructLayout(LayoutKind.Explicit)]`
  - `struct`s are `LayoutKind.Sequential` by default.
- non-nested, one-dimensional arrays of blittable primitive types (for example,
  `int[]`)
- classes with fixed layout that only have blittable value types for instance
  fields
  - fixed layout requires `[StructLayout(LayoutKind.Sequential)]` or
    `[StructLayout(LayoutKind.Explicit)]`
  - classes are LayoutKind.Auto by default
- `char` is blittable in a one-dimensional array or if it's part of a type that
  contains it's explicitly marked with `[StructLayout]` with `CharSet =
  CharSet.Unicode`.

Note that P/Invoke cannot have non-blittable types as a return value as given in
[Blittable and Non-blittable
Types](https://learn.microsoft.com/en-us/dotnet/framework/interop/blittable-and-non-blittable-types)

### 4.2. Directional Parameters

See [.NET 4.0 Directional
Attributes](https://learn.microsoft.com/en-us/previous-versions/dotnet/netframework-4.0/77e6taeh(v=vs.100))
for information about default behaviour.

Keywords `ref`, and `out` parameter modifiers cause method arguments to be
marshaled by reference rather than by value. Method arguments passed by value
are marshaled to unmanaged code as values on the stack; arguments passed by
reference are marshaled as pointers on the stack.

The
[InAttribute](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.inattribute)
is optional. The attribute is supported for COM interop and platform invoke
only. In the absence of explicit settings, the interop marshaller assumes rules
based on the parameter type, whether the parameter is passed by reference or by
value, and whether the type is blittable or non-blittable. For example, the
`StringBuilder` class is always assumed to be `In`/`Out` and an array of strings
passed by value is assumed to be `In`. You cannot apply the `InAttribute` to a
parameter modified with the C#-styled `out` keyword.

The
[OutAttribute](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.outattribute)
is optional. The attribute is supported for COM interop and platform invoke
only. In the absence of explicit settings, the interop marshaller assumes rules
based on the parameter type, whether the parameter is passed by reference or by
value, and whether the type is blittable or non-blittable. `Out`-only behaviour
is never a default marshalling behaviour for parameters. You can apply the
`OutAttribute` to value and reference types passed by reference to change
`In`/`Out` behaviour to `Out`-only behaviour, which is equivalent to using the
`out` keyword in C#. For example, arrays passed by value, marshalled as
`In`-only parameters by default, can be changed to `Out`-only. However, the
behaviour does not always provide expected semantics when the types include
all-blittable elements or fields because the interop marshaller uses pinning. If
you do not care about passing data into the callee, `Out`-only marshaling can
provide better performance for non-blittable types.

### 4.3. Structs and Classes for In/Out parameters

The main difference between classes and structures are outlined, with the
perspective of P/Invoke.

Classes are reference types in C#, while structs as value types. So when passing
a `class` as a parameter, it is already a pointer. A `struct` must be passed
with the `ref` keyword to get the pointer.

The default layout of a struct is `LayoutKind.Sequential`, which is blittable.
The default layout of a class is `LayoutKind.Auto` which is not blittable.
Often, when a struct is changed to a class, it no longer becomes blittable, and
if the attributes `[In]` and `[Out]` are not correctly applied, the calls do not
behave as expected.

According to [Copying and
Pinning](https://learn.microsoft.com/en-us/dotnet/framework/interop/copying-and-pinning),
Formatted blittable classes have fixed layout (formatted) and common data
representation in both managed and unmanaged memory. When these types require
marshalling, a pointer to the object in the heap is passed to the callee
directly.

#### 4.3.1. <a name="InOutStructs"></a>Structs and In/Out parameters

Almost all the P/Invoke methods prefer to use `struct` value types, as a
performance optimisation, to keep as much of the parameters as possible on the
stack as possible, given that these are often temporary values. Passing pointers
to the structures as parameters for P/Invoke is done using the `ref` keyword.

When mapping Win32 API to a parameter inputs as `struct`:

| Native Parameter Direction | C# Keyword (struct) | Notes                                                                         |
| -------------------------- | ------------------- | ----------------------------------------------------------------------------- |
| [in] `LPMYSTRUCT`          | `[In] ref MyStruct` | Default is `[In, Out]`. A `ref` is needed for a `struct` to pass as a pointer |
| [out] `LPMYSTRUCT`         | `out MyStruct`      | The `struct` is allocated on the stack, and P/Invoke copies to the pointer    |
| [in, out] `LPMYSTRUCT`     | `ref MyStruct`      |                                                                               |

One should note, that blittable types have an optimisation where the pointer is
marshalled, not a copy of the original structure. Thus, when the table above
mentions data is copied.

In other words, if the structure used is in the context of `[In] ref MyStruct`,
and it is a blittable type, marshalling gives the pointer to the structure to
the underlying native call without doing an intermediate copy. Any changes to
the structure will be reflected on return of that call, despite there being no
`[Out]` attribute. This may lead to subtle errors, where the definition of the
P/Invoke appears to work, but is technically incorrect.

One should ignore the `const` prefix of C-Style native calls when translating to
P/Invoke for C#. The C# `const` keyword should not be used as it has no meaning
with regard to any of the calling conventions StdApi, FastCall or C, etc.

#### 4.3.2. Classes and In/Out parameters

TBD

### 4.4. Value Type Marshalling

#### 4.4.1. Different Architectures

For reference, different systems have different sizes for their intrinsic types

| Type      | ILP64 | LP64 | LLP64 |
| --------- | ----- | ---- | ----- |
| char      | 8     | 8    | 8     |
| short     | 16    | 16   | 16    |
| int       | 64    | 32   | 32    |
| long      | 64    | 64   | 32    |
| long long | 64    | 64   | 64    |
| pointer   | 64    | 64   | 64    |

Windows uses LLP64, where Linux and MacOS-X use LP64. Therefore, the `long` type
differs in size between the two Operating Systems. C/C++ long and C# long are
not the same types. Using C# long to interop with C/C++ long is almost never
correct.

#### 4.4.2. Windows Value Types

See [Windows Data
Types](https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types)
for the complete list of Windows data types.

| C# Type  | System Type      | Win32 Type                                                                        | Default Marshalling Behaviour | Blittable | Size  | Notes                                                                                                                                                                                                                                                                          |
| -------- | ---------------- | --------------------------------------------------------------------------------- | ----------------------------- | --------- | ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `sbyte`  | `System.SByte`   | `CHAR`, `INT8`                                                                    | `UnmanagedType.I1`            | Yes       | 8     |                                                                                                                                                                                                                                                                                |
| `byte`   | `System.Byte`    | `BYTE`, `UCHAR`, `UINT8`, `BOOLEAN`                                               | `UnmanagedType.U1`            | Yes       | 8     |                                                                                                                                                                                                                                                                                |
| `short`  | `System.Int16`   | `SHORT`, `INT16`, `CSHORT`                                                        | `UnmanagedType.I2`            | Yes       | 16    |                                                                                                                                                                                                                                                                                |
| `ushort` | `System.UInt16`  | `WORD`, `USHORT`, `UINT16`, `ATOM`                                                | `UnmanagedType.U2`            | Yes       | 16    |                                                                                                                                                                                                                                                                                |
| `int`    | `System.Int32`   | `LONG`, `INT`, `INT32`, `LONG32`, `HRESULT`, `NTSTATUS`                           | `UnmanagedType.I4`            | Yes       | 32    |                                                                                                                                                                                                                                                                                |
| `uint`   | `System.UInt32`  | `DWORD`, `ULONG`, `UINT`, `UINT32`, `ULONG32`, `DWORD32`                          | `UnmanagedType.U4`            | Yes       | 32    |                                                                                                                                                                                                                                                                                |
| `long`   | `System.Int64`   | `QWORD`, `LONGLONG`. `INT64`, `LONG64`, `LARGE_INTEGER`                           | `UnmanagedType.I8`            | Yes       | 64    |                                                                                                                                                                                                                                                                                |
| `ulong`  | `System.UInt64`  | `DWORDLONG`, `ULONGLONG`, `UINT64`, `ULONG64`, `DWORD64`, `ULARGE_INTEGER`        | `UnmanagedType.U8`            | Yes       | 64    |                                                                                                                                                                                                                                                                                |
| `nint`   | `System.IntPtr`  | `HANDLE`, `HWND`, `INT_PTR`, `LONG_PTR`, `LPVOID`. `LPARAM`, `LRESULT`, `SSIZE_T` |                               | Yes       | 32/64 |                                                                                                                                                                                                                                                                                |
| `nuint`  | `System.UIntPtr` | `UINT_PTR`, `ULONG_PTR`, `SIZE_T`, `WPARAM`                                       |                               | Yes       | 32/64 |                                                                                                                                                                                                                                                                                |
| `float`  | `System.Single`  | `FLOAT`                                                                           |                               | Yes       | 32    |                                                                                                                                                                                                                                                                                |
| `double` | `System.Double`  | `double`                                                                          |                               | Yes       | 64    |                                                                                                                                                                                                                                                                                |
| `char`   | `System.Char`    | `WCHAR`                                                                           |                               | Yes       | 16    | When marshalling as `CharSet.Unicode`                                                                                                                                                                                                                                          |
| `bool`   | `System.Boolean` | `BOOL`                                                                            | `UnmanagedType.BOOL`          | No        | 32    | This is an `int` 4 byte value. Use an `int` instead. A win `BOOLEAN` is equivalent to a byte. See [Customising Boolean field marshalling](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/customize-struct-marshalling#customizing-boolean-field-marshalling) |
| `string` | `System.String`  | `LPWSTR`                                                                          | `UnmanagedType.LPWStr`        | No        | N/A   | When marshalling as `CharSet.Unicode`                                                                                                                                                                                                                                          |

To marshal a C++ boolean type (1 byte), one can apply the attribute

```csharp
[MarshalAs(UnmanagedType.U1)] bool
```

#### 4.4.3. Strings

##### 4.4.3.1. Strings as Input Parameter

To pass strings to a function, ensure that the `CharSet` is properly defined to
`CharSet.Unicode`. Then one doesn't need to use `MarshalAs.LPWStr` as this is
automatic.

From [Copying and
Pinning](https://learn.microsoft.com/en-us/dotnet/framework/interop/copying-and-pinning#systemstring-and-systemtextstringbuilder)
an optimization when either `String` or `StringBuilder` is marshalled by value
(such as a Unicode character string), the marshaller passes the callee a direct
pointer to managed strings in the internal Unicode buffer instead of copying it
to a new buffer.

When a `String` is passed by reference, the marshaller copies the contents of
the string to a secondary buffer before making the call. It then copies the
contents of the buffer into a new string on return from the call. This technique
ensures that the immutable managed string remains unaltered.

When a `StringBuilder` is passed by value, the marshaller passes a reference to
a temporary copy of the internal buffer of the `StringBuilder` to the caller.
The caller and callee must agree on the size of the buffer. The caller is
responsible for creating a `StringBuilder` of adequate length. The callee must
take the necessary precautions to ensure that the buffer is not overrun.
`StringBuilder` is an exception to the rule that reference types passed by value
are passed as In parameters by default. `StringBuilder` is always passed as
`In`/`Out`. See the next section that the `StringBuilder` should not be used for
performance.

##### 4.4.3.2. Strings Copied into Output Buffers

A lot of old documentation suggested to use a `StringBuilder`, this is not
recommended any more. See
[CA1838](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1838).

TODO: Go into more detail about possibilities using `stackalloc`, arrays,
`ArrayPool` and `Span`.

### 4.5. Marshalling Structs

- DO match the managed struct as closely as possible to the shape and names that
  are used in the official platform documentation or header.
- DO use the C# `sizeof()` instead of `Marshal.SizeOf<MyStruct>()` for blittable
  structures to improve performance.
- DO NOT use `System.Delegate` or `System.MulticastDelegate` fields to represent
  function pointer fields in structures.
  - This has been removed in .NET 5.0 and later, as it can destabilize the
    run-time. See [Best Practices -
    Structs](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/best-practices#structs)

#### 4.5.1. Field Alignment

According to [MSVC Struct Member
Alignment](https://learn.microsoft.com/en-us/cpp/build/reference/zp-struct-member-alignment)
the default alignment is:

| Architecture    | Alignment                               |
| --------------- | --------------------------------------- |
| x86, ARM, ARM64 | Packs structures on 8-byte boundaries.  |
| x64, ARM64EC    | Packs structures on 16-byte boundaries. |

This must be considered when defining structures and classes for blittable types
to Native API.

See [Padding and Alignment of Structure
Members](https://learn.microsoft.com/en-us/cpp/c-language/padding-and-alignment-of-structure-members),
so that each field in C/C++ is allocated a memory address in order it is defined
in the structure, such that the type is correctly aligned.

[x64 ABI for
Windows](https://learn.microsoft.com/en-us/cpp/build/x64-software-conventions)
states:

- The alignment of an array is the same as the alignment of one of the elements
  of the array.
- The alignment of the beginning of a structure or a union is the maximum
  alignment of any individual member. Each member within the structure or union
  must be placed at its proper alignment as defined in the previous table, which
  may require implicit internal padding, depending on the previous member.
- Structure size must be an integral multiple of its alignment, which may
  require padding after the last member. Since structures and unions can be
  grouped in arrays, each array element of a structure or union must begin and
  end at the proper alignment previously determined.

For example, take the following struct in C:

```c
typedef struct _STRRET {
  UINT uType;
  union {
    LPWSTR pOleStr;
    UINT uOffset;
    char cStr[260];
  }  DUMMYUNIONNAME;
} STRRET;
```

The equivalent in 32-bit C#:

```csharp
[StructLayout(LayoutKind.Explicit, Size = 264)]
public struct STRRET_32
{
    [FieldOffset(0)]
    public uint uType;

    [FieldOffset(4)]
    public IntPtr pOleStr;

    [FieldOffset(4)]
    public uint uOffset;

    [FieldOffset(4)]
    public IntPtr cStr;
}
```

The equivalent in 64-bit C#:

```csharp
[StructLayout(LayoutKind.Explicit, Size = 272)]
public struct STRRET_64
{
    [FieldOffset(0)]
    public uint uType;

    [FieldOffset(8)]
    public IntPtr pOleStr;

    [FieldOffset(8)]
    public uint uOffset;

    [FieldOffset(8)]
    public IntPtr cStr;
}
```

Differences attribute that the union must be aligned to 4 bytes on 32-bit or 8
bytes on 64-bit is due to maximum alignment of the `LPWSTR`.

##### 4.5.1.1. Using C++ To Test Alignment (Example DbgHelp)

If you are unsure, it is easy to write a small program in C++ for the Visual
Studio Compiler and test the alignment of the fields. For example, the field
`MINIDUMP_EXCEPTION_INFORMATION` requires the following definition:

```csharp
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct MINIDUMP_EXCEPTION_INFORMATION {
    public uint ThreadId;
    public IntPtr ExceptionPointers;
    public int ClientPointers;
}
```

With the following test program:

```c++
#include <Windows.h>
#include <DbgHelp.h>
#include <cstddef>
#include <iostream>

auto main() -> int {
    std::cout << "Offset ThreadId: " <<
        offsetof(MINIDUMP_EXCEPTION_INFORMATION, ThreadId) << std::endl;
    std::cout << "Offset ExceptionPointers: " <<
        offsetof(MINIDUMP_EXCEPTION_INFORMATION, ExceptionPointers) << std::endl;
    std::cout << "Offset ClientPointers: " <<
        offsetof(MINIDUMP_EXCEPTION_INFORMATION, ClientPointers) << std::endl;
    return 0;
}
```

On 32-bit it returns:

- Offset `ThreadId`: 0
- Offset `ExceptionPointers`: 4
- Offset `ClientPointers`: 8

On 64-bit it returns:

- Offset `ThreadId`: 0
- Offset `ExceptionPointers`: 4
- Offset `ClientPointers`: 12

We can clearly see that alignment with a packing of 4 bytes is required. If we
don't align properly, the program is likely to crash in very difficult to debug
ways.

Even though in the previous section the default packing is 8 bytes for 64-bit,
we clearly see experimentally that the packing is 4. The reason for this is
found in the header:

```c++
#include <pshpack4.h>
```

And the contents of this header file is simple, a `pragma` is used to override
the default:

```c++
#if ! (defined(lint) || defined(RC_INVOKED))
#if ( _MSC_VER >= 800 && !defined(_M_I86)) || defined(_PUSHPOP_SUPPORTED)
#pragma warning(disable:4103)
#if !(defined( MIDL_PASS )) || defined( __midl )
#pragma pack(push,4)
#else
#pragma pack(4)
#endif
#else
#pragma pack(4)
#endif
#endif /* ! (defined(lint) || defined(RC_INVOKED)) */
```

#### 4.5.2. Blittable Strings (actually char[])

As mentioned in [Blittable types when runtime marshalling is
enabled](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/best-practices#blittable-types-when-runtime-marshalling-is-enabled),
`char` is blittable in a one-dimensional array or if it's part of a type that
contains it's explicitly marked with `[StructLayout]` with `CharSet =
CharSet.Unicode`.

#### 4.5.3. Strings in Structs

In [Customising Struct
Marshalling](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/customize-struct-marshalling)
we can pass a fixed size character array via:

```csharp
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct DefaultString {
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string str;
}
```

A specific example can be seen for `OSVERSIONINFOW`:

```cpp
typedef struct _OSVERSIONINFOW {
  DWORD dwOSVersionInfoSize;
  DWORD dwMajorVersion;
  DWORD dwMinorVersion;
  DWORD dwBuildNumber;
  DWORD dwPlatformId;
  WCHAR szCSDVersion[128];
} OSVERSIONINFOW, *POSVERSIONINFOW, *LPOSVERSIONINFOW, RTL_OSVERSIONINFOW, *PRTL_OSVERSIONINFOW;
```

which has the C# equivalent of (non-blittable due to the presence of the
`string`)

```csharp
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public class OSVERSIONINFO {
  public int OSVersionInfoSize;
  public int MajorVersion;
  public int MinorVersion;
  public int BuildNumber;
  public int PlatformId;
  [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x80)]
  public string CSDVersion;

  public OSVERSIONINFO() {
    OSVersionInfoSize = Marshal.SizeOf(this);
  }
}
```

#### 4.5.4. Fixed Size Buffers

[Unsafe Fixed Size
Buffers](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/unsafe-code#fixed-size-buffers)
are defined since [C#
7.0](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/unsafe-code#2282-fixed-size-buffer-declarations).
This can be used to help define blittable structs. Let's take `OSVERSIONINFO`.

```csharp
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public unsafe struct OSVERSIONINFO {
  public int OSVersionInfoSize;
  public int MajorVersion;
  public int MinorVersion;
  public int BuildNumber;
  public int PlatformId;
  public fixed char CSDVersion[128];
}
```

### 4.6. Memory Pinning

When calling a P/Invoke method, `struct`s and `class`es are automatically pinned
for the duration of the P/Invoke call. Any conversions of structures to pointers
via the `IntPtr` class must be explicitly pinned by the calling code. If you
need to implement your own marshalling routines and use an `IntPtr` or pointers,
usually you must also consider pinning.

On return of the call, the structure is no longer pinned, so that if the
underlying library maintains a copy of the structure reference pointer, .NET
native code must pin it first.

#### 4.6.1. Memory Buffers for Asynchronous I/O

A common example is to ensure an array byte of data does not move between
various invocations of Win32 API. For example, an asynchronous read is
initiated, followed by a method to wait for when the read is complete. The
contents of that buffer may not move.

As such, explicitly pinning the object is required, e.g.

```csharp
byte[] read = new byte[readBuffer];
GCHandle m_ReadHandle = GCHandle.Alloc(read, GCHandleType.Pinned);
IntPtr ptr = m_ReadHandle.AddrOfPinnedObject();
...
m_ReadHandle.Free();
```

##### 4.6.1.1. Native Overlapped

An example of this is the `NativeOverlapped` structure, where Win32 keeps a copy
of the pointer in its own internal implementation, to later notify operations as
complete by signalling parameters within that structure asynchronously.

In this specific case, the .NET runtime provides methods to do the pinning for
you, and those methods should be used. See my CodeProject article [Asynchronous
I/O with
Thread.BindHandle](https://www.codeproject.com/Articles/523355/Asynchronous-I-O-with-Thread-BindHandle)
for details.

#### 4.6.2. Memory Buffers for Callbacks

Any callbacks the reenter into C# code might be easier to pin using the `fixed`
keyword, which fixes the memory region and provides an unsafe pointer.

#### 4.6.3. Garbage Collection

One must ensure that the underlying C# type must not be freed by the GC while
the native code maintains a pointer to the .NET object. This is usually done by
ensuring there is always a reference to the .NET object during the lifetime of
the native code keeping a reference to the object.

### 4.7. Safe Handles

TBD

## 5. Callbacks and Function Pointers

TBD.

- [Function Pointers for Managed
  Code](https://devblogs.microsoft.com/dotnet/improvements-in-native-code-interop-in-net-5-0/)

## 6. Appendix

### 6.1. References

.NET Standard:

- [Native Interoperability Best
  Practices](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/best-practices)
- [Cross-Platform
  P/Invoke](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/cross-platform)
- [Charsets and
  Marshalling](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/charset)
- [Customise Struct
  Marshalling](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/customize-struct-marshalling)
- [Type
  Marshalling](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/type-marshalling)

.NET Framework

- [Interop
  Marshalling](https://learn.microsoft.com/en-us/dotnet/framework/interop/interop-marshalling)
- [Marshalling Classes, Structures and
  Unions](https://learn.microsoft.com/en-us/dotnet/framework/interop/marshalling-classes-structures-and-unions)
- [.NET 4.0 Directional
  Attributes](https://learn.microsoft.com/en-us/previous-versions/dotnet/netframework-4.0/77e6taeh(v=vs.100))
- [Copying and
  Pinning](https://learn.microsoft.com/en-us/dotnet/framework/interop/copying-and-pinning)
- [HowTo: Marshal Structures using
  P/Invoke](https://learn.microsoft.com/en-us/cpp/dotnet/how-to-marshal-structures-using-pinvoke?redirectedfrom=MSDN&view=msvc-170)

### 6.2. Abbreviations

A list of abbreviations used in this document

- AOT - Ahead of Time compilation
- GC - .NET Garbage Collector
- P/Invoke - Platform Invocation
