namespace Utilities.MemoryHandling
{
    using System;
    using System.Runtime.InteropServices;

    internal static class Imports
    {
        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);
        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, uint lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);
        [DllImport("kernel32.dll")]
        public static extern int GetLastError();
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessID);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, AllocationType flAllocationType, MemoryProtection flProtect);
        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr lpHandle, IntPtr lpAddress, byte[] lpBuffer, int lpSize, out int lpBytesWrote);

        [Flags]
        public enum AllocationType
        {
            Commit = 0x1000,
            Decommit = 0x4000,
            LargePages = 0x20000000,
            Physical = 0x400000,
            Release = 0x8000,
            Reserve = 0x2000,
            Reset = 0x80000,
            TopDown = 0x100000,
            WriteWatch = 0x200000
        }

        [Flags]
        public enum MemoryProtection
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            GuardModifierflag = 0x100,
            NoAccess = 1,
            NoCacheModifierflag = 0x200,
            ReadOnly = 2,
            ReadWrite = 4,
            WriteCombineModifierflag = 0x400,
            WriteCopy = 8
        }

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x1f0fff,
            CreateThread = 2,
            DupHandle = 0x40,
            QueryInformation = 0x400,
            SetInformation = 0x200,
            Synchronize = 0x100000,
            Terminate = 1,
            VMOperation = 8,
            VMRead = 0x10,
            VMWrite = 0x20
        }
    }
}

