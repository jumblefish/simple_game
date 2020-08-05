using Microsoft.Diagnostics.Runtime.ICorDebug;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
//using static Bootstrapper.WinImports;

namespace Bootstrapper
{
    class Program
    {
        const string PATH_TO_GAME = @"C:\Users\Black Adder\source\repos\Simple_Game\Release\Simple_Game.exe";
        const uint DEBUG_PROCESS = 0x00000001;
        const uint DEBUG_ONLY_THIS_PROCESS = 0x00000002;
        const uint CREATE_SUSPENDED = 0x00000004;
        const uint DETACHED_PROCESS = 0x00000008;
        const uint CREATE_NEW_CONSOLE = 0x00000010;
        const uint NORMAL_PRIORITY_CLASS = 0x00000020;
        const uint IDLE_PRIORITY_CLASS = 0x00000040;
        const uint HIGH_PRIORITY_CLASS = 0x00000080;
        const uint REALTIME_PRIORITY_CLASS = 0x00000100;
        const uint CREATE_NEW_PROCESS_GROUP = 0x00000200;
        const uint CREATE_UNICODE_ENVIRONMENT = 0x00000400;
        const uint CREATE_SEPARATE_WOW_VDM = 0x00000800;
        const uint CREATE_SHARED_WOW_VDM = 0x00001000;
        const uint CREATE_FORCEDOS = 0x00002000;
        const uint BELOW_NORMAL_PRIORITY_CLASS = 0x00004000;
        const uint ABOVE_NORMAL_PRIORITY_CLASS = 0x00008000;
        const uint INHERIT_PARENT_AFFINITY = 0x00010000;
        const uint INHERIT_CALLER_PRIORITY = 0x00020000;
        const uint CREATE_PROTECTED_PROCESS = 0x00040000;
        const uint EXTENDED_STARTUPINFO_PRESENT = 0x00080000;
        const uint PROCESS_MODE_BACKGROUND_BEGIN = 0x00100000;
        const uint PROCESS_MODE_BACKGROUND_END = 0x00200000;
        const uint CREATE_BREAKAWAY_FROM_JOB = 0x01000000;
        const uint CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000;
        const uint CREATE_DEFAULT_ERROR_MODE = 0x04000000;
        const uint CREATE_NO_WINDOW = 0x08000000;
        const uint PROFILE_USER = 0x10000000;
        const uint PROFILE_KERNEL = 0x20000000;
        const uint PROFILE_SERVER = 0x40000000;
        const uint CREATE_IGNORE_SYSTEM_DEFAULT = 0x80000000;
        

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress,
           int dwSize, AllocationType dwFreeType);


        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess,
           IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress,
           IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(
          IntPtr hProcess,
          IntPtr lpBaseAddress,
          byte[] lpBuffer,
          Int32 nSize,
          out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(
          IntPtr hProcess,
          IntPtr lpBaseAddress,
          [MarshalAs(UnmanagedType.AsAny)] object lpBuffer,
          int dwSize,
          out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
           uint dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

        private enum MemoryFreeType : uint
        {
            /// <summary>
            /// Decommits the specified region of committed pages. 
            /// After the operation, the pages are in the reserved 
            /// state. 
            /// </summary>
            Decommit = 0x4000,
            /// <summary>
            /// Releases the specified region of pages. After this 
            /// operation, the pages are in the free state. 
            /// </summary>
            Release = 0x8000
        }

        [Flags]
        public enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        [Flags]
        public enum MemoryProtection
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }
        /*
        [StructLayout(LayoutKind.Sequential)]
        internal class SECURITY_ATTRIBUTES
        {
            
            public int nLength = 12;
            public SafeLocalMemHandle lpSecurityDescriptor = new SafeLocalMemHandle(IntPtr.Zero, false);
            public bool bInheritHandle = false;
        }
        */
        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool CreateProcess(
           string lpApplicationName,
           string lpCommandLine,
           ref SECURITY_ATTRIBUTES lpProcessAttributes,
           ref SECURITY_ATTRIBUTES lpThreadAttributes,
           bool bInheritHandles,
           uint dwCreationFlags,
           IntPtr lpEnvironment,
           string lpCurrentDirectory,
           [In] ref STARTUPINFO lpStartupInfo,
           out PROCESS_INFORMATION lpProcessInformation);

        static void Main()
        {
            var startupInfo = new STARTUPINFO();

            IntPtr myPtr = new IntPtr(0);
            // run BloogsQuest.exe in a new process
            CreateProcess(
                PATH_TO_GAME,
                null,
                ref myPtr,
                IntPtr.Zero,
                false,
                CREATE_DEFAULT_ERROR_MODE,
                IntPtr.Zero,
                null,
                ref startupInfo,
                out PROCESS_INFORMATION processInfo);

            // get a handle to the BloogsQuest process
            var processHandle = Process.GetProcessById((int)processInfo.dwProcessId).Handle;

            // resolve the file path to Loader.dll relative to our current working directory
            var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var loaderPath = $"{currentFolder}\\Internal\\Loader.dll";

            // allocate enough memory to hold the full file path to Loader.dll within the BloogsQuest process
            var loaderPathPtr = VirtualAllocEx(
                processHandle,
                (IntPtr)0,
                loaderPath.Length,
                MemoryAllocationType.MEM_COMMIT,
                MemoryProtectionType.PAGE_EXECUTE_READWRITE);

            // write the file path to Loader.dll to the BloogsQuest process's memory
            var bytes = Encoding.Unicode.GetBytes(loaderPath);
            var bytesWritten = 0; // throw away
            WriteProcessMemory(processHandle, loaderPathPtr, bytes, bytes.Length, out bytesWritten);

            // search current process's for the memory address of the LoadLibraryW function within the kernel32.dll module
            var loaderDllPointer = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryW");

            // create a new thread with the execution starting at the LoadLibraryW function, 
            // with the path to our Loader.dll passed as a parameter
            CreateRemoteThread(processHandle, (IntPtr)null, (IntPtr)0, loaderDllPointer, loaderPathPtr, 0, (IntPtr)null);

            // free the memory that was allocated by VirtualAllocEx
            VirtualFreeEx(processHandle, loaderPathPtr, 0, MemoryFreeType.Release);
        }
    }
