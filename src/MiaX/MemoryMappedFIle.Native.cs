using System;
using System.Runtime.InteropServices;

namespace MiaX
{
    internal partial class MemoryMappedFIle : IDisposable
    {
        private IntPtr _fileHandle;
        private IntPtr _memoryMappedFileHandle;

        public void Create(string fileName, long capacity)
        {
           _fileHandle = Native.CreateFile(
                fileName,
                Native.GENERIC_READ_WRITE,
                Native.PREVENTS_OTHER_PROCESSES,
                IntPtr.Zero,
                Native.CREATE_ALWAYS,
                Native.FILE_ATTRIBUTE_NORMAL,
                IntPtr.Zero);

           _memoryMappedFileHandle = Native.CreateFileMapping(
               _fileHandle,
               IntPtr.Zero,
               Native.PAGE_READWRITE,
               (uint)capacity >> 32,
               (uint)capacity & 0xFFFFFFFF,
               null);
        }

        public void Open(string fileName)
        {
            _fileHandle = Native.CreateFile(
                @"\\C:\" + fileName,
                Native.GENERIC_READ_WRITE,
                Native.PREVENTS_OTHER_PROCESSES,
                IntPtr.Zero,
                Native.OPEN_EXISTING,
                Native.FILE_ATTRIBUTE_NORMAL,
                IntPtr.Zero);


        }



        public void Dispose()
        {
            Native.CloseHandle(_memoryMappedFileHandle);
            Native.CloseHandle(_fileHandle);
        }

        static partial class Native
        {
            public const uint GENERIC_READ             = 0x00000001;
            public const uint GENERIC_WRITE            = 0x00000002;
            public const uint GENERIC_READ_WRITE       = GENERIC_READ | GENERIC_WRITE;
            public const uint PREVENTS_OTHER_PROCESSES = 0x00000000;
            public const uint CREATE_ALWAYS            = 0x00000002;
            public const uint OPEN_EXISTING            = 0x00000003;
            public const uint FILE_ATTRIBUTE_NORMAL    = 0x00000080;
            public const uint PAGE_READWRITE           = 0x00000004;



            private const string KERNEL32 = "kernel32";

            [DllImport(KERNEL32, EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr CreateFile(
                [MarshalAs(UnmanagedType.LPWStr)] 
                string filename,
                uint   access,
                uint   share,
                IntPtr securityAttributes,
                uint   creationDisposition,
                uint   flagsAndAttributes,
                IntPtr templateFile);



            [DllImport(KERNEL32, EntryPoint = "CreateFileMappingW", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr CreateFileMapping(
                IntPtr hFile,
                IntPtr lpFileMappingAttributes,
                uint   flProtect,
                uint   dwMaximumSizeHigh,
                uint   dwMaximumSizeLow,
                [MarshalAs(UnmanagedType.LPWStr)] 
                string lpName);

            [DllImport(KERNEL32, SetLastError =true)]
            public static extern bool CloseHandle(IntPtr hHandle);

        }
    }
}
