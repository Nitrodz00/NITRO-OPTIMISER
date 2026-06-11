using System;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NitroOptimizer.Models;
using NitroOptimizer.Services.Interfaces;

namespace NitroOptimizer.Services
{
    public class RestorePointService : IRestorePointService
    {
        [DllImport("Srclient.dll")]
        private static extern int SRSetRestorePointW(ref RESTOREPOINTINFO pRestorePtSpec, out STATEMGRSTATUS pSMgrStatus);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct RESTOREPOINTINFO
        {
            public int dwEventType;
            public int dwRestorePtType;
            public long llSequenceNumber;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szDescription;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STATEMGRSTATUS
        {
            public int nStatus;
            public long llSequenceNumber;
        }

        public Task<TweakResult> CreateRestorePointAsync(string description)
        {
            return Task.Run(() =>
            {
                try
                {
                    RESTOREPOINTINFO rpInfo = new RESTOREPOINTINFO
                    {
                        dwEventType = 100, // BEGIN_SYSTEM_CHANGE
                        dwRestorePtType = 0, // APPLICATION_INSTALL
                        llSequenceNumber = 0,
                        szDescription = description
                    };

                    STATEMGRSTATUS status;
                    int result = SRSetRestorePointW(ref rpInfo, out status);

                    if (result == 0)
                    {
                        return new TweakResult { Success = true, Message = "Restore point created successfully." };
                    }
                    else
                    {
                        return new TweakResult { Success = false, Message = $"Failed to create restore point. Error code: {result}" };
                    }
                }
                catch (Exception ex)
                {
                    return new TweakResult { Success = false, Message = $"Error: {ex.Message}" };
                }
            });
        }
    }
}
