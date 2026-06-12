using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NitroOptimizer.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Management;
using System.Threading.Tasks;

namespace NitroOptimizer.ViewModels
{
    public class DriverItem
    {
        public string Name { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
    }

    public partial class DriversViewModel : ObservableObject
    {
        private readonly ITweakService _tweakService;

        [ObservableProperty] private ObservableCollection<DriverItem> _installedDrivers = new();
        [ObservableProperty] private string _statusMessage = string.Empty;
        [ObservableProperty] private bool _isBusy = false;

        public DriversViewModel(ITweakService tweakService)
        {
            _tweakService = tweakService;
            _ = LoadDriversAsync();
        }

        [RelayCommand]
        private async Task LoadDriversAsync()
        {
            IsBusy = true;
            StatusMessage = "⏳ Loading installed drivers (this may take a moment)...";
            InstalledDrivers.Clear();

            await Task.Run(() =>
            {
                try
                {
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPSignedDriver WHERE DeviceName IS NOT NULL"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            var item = new DriverItem
                            {
                                Name = obj["DeviceName"]?.ToString() ?? "Unknown Device",
                                Manufacturer = obj["Manufacturer"]?.ToString() ?? "Unknown",
                                Version = obj["DriverVersion"]?.ToString() ?? "Unknown"
                            };

                            App.Current.Dispatcher.Invoke(() => InstalledDrivers.Add(item));
                        }
                    }
                }
                catch { }
            });

            StatusMessage = $"✅ Loaded {InstalledDrivers.Count} drivers.";
            IsBusy = false;
        }

        [RelayCommand]
        private async Task BackupDriversAsync()
        {
            IsBusy = true;
            StatusMessage = "⏳ Backing up all system drivers to C:\\DriversBackup (DISM)...";
            var result = await _tweakService.BackupDriversAsync(@"C:\DriversBackup");
            StatusMessage = result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
            IsBusy = false;
        }

        [RelayCommand]
        private async Task RestoreDriversAsync()
        {
            IsBusy = true;
            StatusMessage = "⏳ Restoring drivers from C:\\DriversBackup (Pnputil)...";
            var result = await _tweakService.RestoreDriversAsync(@"C:\DriversBackup");
            StatusMessage = result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
            IsBusy = false;
        }
    }
}
