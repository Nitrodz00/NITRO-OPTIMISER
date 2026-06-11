using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace NitroOptimizer.ViewModels
{
    public partial class ReportsViewModel : ObservableObject
    {
        [ObservableProperty] private string _statusMessage = string.Empty;
        [ObservableProperty] private string _reportText = "Click 'Generate Report' to view system details here.";
        [ObservableProperty] private bool _isBusy = false;

        [RelayCommand]
        private async Task GenerateReportAsync()
        {
            IsBusy = true;
            StatusMessage = "⏳ Generating detailed system report...";
            await Task.Run(() =>
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("===========================================");
                    sb.AppendLine("       NITRO OPTIMIZER SYSTEM REPORT       ");
                    sb.AppendLine("===========================================");
                    sb.AppendLine($"Date: {DateTime.Now}");
                    sb.AppendLine($"OS Version: {Environment.OSVersion.VersionString}");
                    sb.AppendLine($"64-Bit OS: {Environment.Is64BitOperatingSystem}");
                    sb.AppendLine($"System Directory: {Environment.SystemDirectory}");
                    sb.AppendLine($"Machine Name: {Environment.MachineName}");
                    sb.AppendLine($"User Name: {Environment.UserName}");
                    sb.AppendLine();
                    
                    sb.AppendLine("--- Processor (CPU) ---");
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name, NumberOfCores, NumberOfLogicalProcessors FROM Win32_Processor"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            sb.AppendLine($"Model: {obj["Name"]}");
                            sb.AppendLine($"Cores: {obj["NumberOfCores"]} / Threads: {obj["NumberOfLogicalProcessors"]}");
                        }
                    }
                    
                    sb.AppendLine();
                    sb.AppendLine("--- Memory (RAM) ---");
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Capacity, Speed, Manufacturer FROM Win32_PhysicalMemory"))
                    {
                        long totalRam = 0;
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            if (obj["Capacity"] != null)
                            {
                                long ram = Convert.ToInt64(obj["Capacity"]);
                                totalRam += ram;
                                sb.AppendLine($"Module: {ram / 1073741824} GB - {obj["Speed"]} MHz - {obj["Manufacturer"]}");
                            }
                        }
                        sb.AppendLine($"Total Installed RAM: {totalRam / 1073741824} GB");
                    }

                    sb.AppendLine();
                    sb.AppendLine("--- Motherboard ---");
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Manufacturer, Product, Version FROM Win32_BaseBoard"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            sb.AppendLine($"Manufacturer: {obj["Manufacturer"]}");
                            sb.AppendLine($"Model: {obj["Product"]} (Rev {obj["Version"]})");
                        }
                    }

                    sb.AppendLine();
                    sb.AppendLine("--- Video Controller (GPU) ---");
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name, AdapterRAM, DriverVersion FROM Win32_VideoController"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            sb.AppendLine($"GPU: {obj["Name"]}");
                            if (obj["AdapterRAM"] != null)
                            {
                                long ram = Convert.ToInt64(obj["AdapterRAM"]);
                                sb.AppendLine($"VRAM: {ram / 1048576} MB");
                            }
                            sb.AppendLine($"Driver Version: {obj["DriverVersion"]}");
                        }
                    }

                    sb.AppendLine();
                    sb.AppendLine("--- Disk Drives ---");
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Model, Size FROM Win32_DiskDrive"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            sb.AppendLine($"Model: {obj["Model"]}");
                            if (obj["Size"] != null)
                            {
                                long size = Convert.ToInt64(obj["Size"]);
                                sb.AppendLine($"Size: {size / 1073741824} GB");
                            }
                        }
                    }

                    App.Current.Dispatcher.Invoke(() => ReportText = sb.ToString());
                    StatusMessage = $"✅ Report generated successfully.";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"❌ Error: {ex.Message}";
                }
            });
            IsBusy = false;
        }

        [RelayCommand]
        private void SaveReport()
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "NitroOptimizer_Report.txt");
                File.WriteAllText(path, ReportText);
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
                StatusMessage = $"✅ Report saved to Desktop: NitroOptimizer_Report.txt";
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Error saving report: {ex.Message}";
            }
        }
    }
}
