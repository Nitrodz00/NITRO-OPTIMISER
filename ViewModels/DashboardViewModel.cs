using System.Management;
using System.Diagnostics;
using System.Timers;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NitroOptimizer.Services.Interfaces;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace NitroOptimizer.ViewModels
{
    public class DriveItem
    {
        public string Name { get; set; } = string.Empty;
        public string UsageText { get; set; } = string.Empty;
        public double UsagePercentage { get; set; }
    }

    public partial class DashboardViewModel : ObservableObject
    {
        private readonly ITweakService _tweakService;
        private readonly Timer _tempTimer;

        [ObservableProperty] private string _systemStatus = "Good";
        [ObservableProperty] private ObservableCollection<DriveItem> _drives = new();
        [ObservableProperty] private string _cpuTemp = "-";
        [ObservableProperty] private string _gpuTemp = "-";

        public DashboardViewModel(ITweakService tweakService)
        {
            _tweakService = tweakService;
            LoadDashboardData();

            _tempTimer = new Timer(3000);
            _tempTimer.Elapsed += (s, e) => UpdateTemps();
            _tempTimer.Start();
            UpdateTemps();
        }

        private void LoadDashboardData()
        {
            try
            {
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    if (drive.IsReady)
                    {
                        long totalSize = drive.TotalSize;
                        long freeSpace = drive.AvailableFreeSpace;
                        long usedSpace = totalSize - freeSpace;

                        double totalGB = totalSize / 1073741824.0;
                        double freeGB = freeSpace / 1073741824.0;
                        double usedGB = usedSpace / 1073741824.0;

                        double usagePercentage = (totalSize > 0) ? (usedSpace * 100.0 / totalSize) : 0;

                        App.Current.Dispatcher.Invoke(() => Drives.Add(new DriveItem
                        {
                            Name = $"Drive {drive.Name}",
                            UsageText = $"{usedGB:F1} GB / {totalGB:F1} GB",
                            UsagePercentage = usagePercentage
                        }));
                    }
                }
            }
            catch { }
        }

        private void UpdateTemps()
        {
            double cpu = GetCpuTemperature();
            double gpu = GetGpuTemperature();

            App.Current.Dispatcher.Invoke(() =>
            {
                CpuTemp = cpu > 0 ? $"{cpu:F0} °C" : "N/A";
                GpuTemp = gpu > 0 ? $"{gpu:F0} °C" : "N/A";
                
                if (cpu > 82 || gpu > 82)
                {
                    SystemStatus = "High Temp Warn!";
                }
                else
                {
                    SystemStatus = "Good";
                }
            });
        }

        private double GetCpuTemperature()
        {
            try
            {
                double maxTemp = 0;
                using (var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        double tempKelvin = Convert.ToDouble(obj["CurrentTemperature"]);
                        double tempCelsius = (tempKelvin - 2731.5) / 10.0;
                        if (tempCelsius > maxTemp && tempCelsius < 120 && tempCelsius > 10)
                        {
                            maxTemp = tempCelsius;
                        }
                    }
                }
                return maxTemp;
            }
            catch { }
            return 0;
        }

        private double GetGpuTemperature()
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = "nvidia-smi",
                        Arguments = "--query-gpu=temperature.gpu --format=csv,noheader",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit();
                    if (double.TryParse(output, out double temp))
                    {
                        return temp;
                    }
                }
            }
            catch { }
            return 0;
        }

        [RelayCommand]
        private async Task RevertTweaksAsync()
        {
            var result = MessageBox.Show(
                "Are you sure you want to revert all system optimizations back to Windows defaults?",
                "NITRO OPTIMISER - Revert Tweaks",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var res = await _tweakService.RevertAllTweaksAsync();
                MessageBox.Show(res.Message, "NITRO OPTIMISER", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        [RelayCommand]
        private void ToggleWidget()
        {
            var services = App.AppHost!.Services;
            var widgetViewModel = services.GetRequiredService<WidgetViewModel>();
            widgetViewModel.ToggleWidget();
        }
    }
}
