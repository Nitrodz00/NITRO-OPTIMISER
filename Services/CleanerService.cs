using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NitroOptimizer.Services.Interfaces;

namespace NitroOptimizer.Services
{
    public class CleanerService : ICleanerService
    {
        [DllImport("psapi.dll")]
        static extern int EmptyWorkingSet(IntPtr hwProc);

        [DllImport("ntdll.dll")]
        static extern int NtSetSystemInformation(int SystemInformationClass, IntPtr SystemInformation, int SystemInformationLength);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall, ref TOKEN_PRIVILEGES newst, int len, IntPtr prev, IntPtr relen);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool LookupPrivilegeValue(string host, string name, ref LUID pluid);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct TOKEN_PRIVILEGES
        {
            public int PrivilegeCount;
            public LUID Privileges;
            public int Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LUID
        {
            public int LowPart;
            public int HighPart;
        }

        const int SE_PRIVILEGE_ENABLED = 2;
        const int TOKEN_QUERY = 0x00000008;
        const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        const string SE_PROF_SINGLE_PROCESS_NAME = "SeProfileSingleProcessPrivilege";

        public Task<long> CleanRamAsync()
        {
            return Task.Run(() =>
            {
                long freed = 0;
                try
                {
                    // 1. Empty Working Set for all processes
                    foreach (Process process in Process.GetProcesses())
                    {
                        try
                        {
                            EmptyWorkingSet(process.Handle);
                            freed += 1024 * 1024; // Dummy estimate
                        }
                        catch { }
                    }

                    // 2. Clear Standby List (Cache)
                    ClearStandbyList();
                }
                catch { }
                return freed;
            });
        }

        private static void ClearStandbyList()
        {
            try
            {
                IntPtr token = IntPtr.Zero;
                if (OpenProcessToken(Process.GetCurrentProcess().Handle, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref token))
                {
                    TOKEN_PRIVILEGES tp;
                    tp.PrivilegeCount = 1;
                    tp.Attributes = SE_PRIVILEGE_ENABLED;
                    LUID luid = new LUID();
                    LookupPrivilegeValue(null, SE_PROF_SINGLE_PROCESS_NAME, ref luid);
                    tp.Privileges = luid;

                    if (AdjustTokenPrivileges(token, false, ref tp, Marshal.SizeOf(tp), IntPtr.Zero, IntPtr.Zero))
                    {
                        // 4 = SystemMemoryListInformation
                        // 4 is the Command to clear standby list (SystemClearStandbyPageList)
                        IntPtr memoryListCommand = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
                        Marshal.WriteInt32(memoryListCommand, 4); 
                        NtSetSystemInformation(80, memoryListCommand, Marshal.SizeOf(typeof(int))); // 80 is SystemMemoryListInformation
                        Marshal.FreeHGlobal(memoryListCommand);
                    }
                }
            }
            catch { }
        }
        public Task<long> CleanTempFilesAsync()
        {
            return Task.Run(() =>
            {
                long freedSpace = 0;
                string[] tempFolders = {
                    Path.GetTempPath(),
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\Temp"
                };

                foreach (var folder in tempFolders)
                {
                    if (Directory.Exists(folder))
                    {
                        freedSpace += DeleteFolderContents(folder);
                    }
                }
                return freedSpace;
            });
        }

        public Task<long> CleanWindowsUpdateCacheAsync()
        {
            return Task.Run(() =>
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\SoftwareDistribution\Download";
                if (Directory.Exists(path))
                {
                    return DeleteFolderContents(path);
                }
                return 0L;
            });
        }

        public Task<long> CleanDirectXCacheAsync()
        {
            return Task.Run(() =>
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\D3DSCache";
                if (Directory.Exists(path))
                {
                    return DeleteFolderContents(path);
                }
                return 0L;
            });
        }

        public Task<long> CleanGpuCacheAsync()
        {
            return Task.Run(() =>
            {
                long freed = 0;
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                
                // NVIDIA
                string nvCache1 = Path.Combine(localAppData, "NVIDIA Corporation", "NV_Cache");
                string nvCache2 = Path.Combine(localAppData, "NVIDIA", "GLCache");
                string nvCache3 = Path.Combine(localAppData, "NVIDIA", "DXCache");
                
                // AMD
                string amdCache = Path.Combine(localAppData, "AMD", "DxCache");

                freed += DeleteFolderContents(nvCache1);
                freed += DeleteFolderContents(nvCache2);
                freed += DeleteFolderContents(nvCache3);
                freed += DeleteFolderContents(amdCache);

                return freed;
            });
        }

        public Task<long> CleanFiveMCacheAsync()
        {
            return Task.Run(() =>
            {
                long freed = 0;
                string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string fivemPath = Path.Combine(localApp, @"FiveM\FiveM.app\data");

                if (Directory.Exists(fivemPath))
                {
                    string[] foldersToClean = { "cache", "server-cache", "server-cache-priv" };
                    foreach (var folder in foldersToClean)
                    {
                        string p = Path.Combine(fivemPath, folder);
                        if (Directory.Exists(p))
                        {
                            freed += DeleteFolderContents(p);
                        }
                    }
                }
                return freed;
            });
        }

        private long DeleteFolderContents(string folderPath)
        {
            long freed = 0;
            try
            {
                DirectoryInfo di = new DirectoryInfo(folderPath);

                foreach (FileInfo file in di.GetFiles())
                {
                    try
                    {
                        freed += file.Length;
                        file.Delete();
                    }
                    catch { /* File in use */ }
                }

                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    try
                    {
                        freed += GetDirectorySize(dir);
                        dir.Delete(true);
                    }
                    catch { /* Directory in use */ }
                }
            }
            catch { /* Access denied */ }

            return freed;
        }

        private long GetDirectorySize(DirectoryInfo d)
        {
            long size = 0;
            try
            {
                FileInfo[] fis = d.GetFiles();
                foreach (FileInfo fi in fis) size += fi.Length;
                DirectoryInfo[] dis = d.GetDirectories();
                foreach (DirectoryInfo di in dis) size += GetDirectorySize(di);
            }
            catch { }
            return size;
        }
    }
}
