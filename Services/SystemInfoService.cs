using System;
using System.IO;
using System.Management;
using System.Threading.Tasks;
using NitroOptimizer.Models;
using NitroOptimizer.Services.Interfaces;

namespace NitroOptimizer.Services
{
    public class SystemInfoService : ISystemInfoService
    {
        public Task<SystemInfoModel> GetSystemInfoAsync()
        {
            return Task.Run(() =>
            {
                var model = new SystemInfoModel();
                
                try
                {
                    // Processor
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select Name, LoadPercentage from Win32_Processor"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            model.ProcessorName = obj["Name"]?.ToString() ?? "Unknown";
                            if (ushort.TryParse(obj["LoadPercentage"]?.ToString(), out ushort load))
                            {
                                model.CpuUsage = load;
                            }
                            break;
                        }
                    }

                    // Graphics Card
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select Name from Win32_VideoController"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            model.GraphicsCardName = obj["Name"]?.ToString() ?? "Unknown";
                            break;
                        }
                    }

                    // OS Version
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select Caption, Version from Win32_OperatingSystem"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            model.WindowsVersion = $"{obj["Caption"]} ({obj["Version"]})";
                            break;
                        }
                    }

                    // RAM
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select TotalVisibleMemorySize, FreePhysicalMemory from Win32_OperatingSystem"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            if (ulong.TryParse(obj["TotalVisibleMemorySize"]?.ToString(), out ulong total) &&
                                ulong.TryParse(obj["FreePhysicalMemory"]?.ToString(), out ulong free))
                            {
                                double totalGB = total / 1024.0 / 1024.0;
                                double usedGB = (total - free) / 1024.0 / 1024.0;
                                model.RamUsage = Math.Round((usedGB / totalGB) * 100, 1);
                            }
                            break;
                        }
                    }

                    // Disk
                    DriveInfo mainDrive = new DriveInfo("C");
                    if (mainDrive.IsReady)
                    {
                        double totalGB = mainDrive.TotalSize / 1024.0 / 1024.0 / 1024.0;
                        double freeGB = mainDrive.AvailableFreeSpace / 1024.0 / 1024.0 / 1024.0;
                        double usedGB = totalGB - freeGB;
                        model.TotalDiskSpace = $"{Math.Round(totalGB, 1)} GB";
                        model.FreeDiskSpace = $"{Math.Round(freeGB, 1)} GB";
                        model.DiskUsage = Math.Round((usedGB / totalGB) * 100, 1);
                    }

                    // Uptime
                    model.Uptime = GetUptime();
                }
                catch (Exception ex)
                {
                    // Log error
                    Console.WriteLine(ex.Message);
                }

                return model;
            });
        }

        private string GetUptime()
        {
            using (var uptime = new System.Diagnostics.PerformanceCounter("System", "System Up Time"))
            {
                uptime.NextValue();
                TimeSpan ts = TimeSpan.FromSeconds(uptime.NextValue());
                return $"{ts.Days}d {ts.Hours}h {ts.Minutes}m";
            }
        }
    }
}
