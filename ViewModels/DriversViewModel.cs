using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        [ObservableProperty] private ObservableCollection<DriverItem> _installedDrivers = new();
        [ObservableProperty] private string _statusMessage = string.Empty;
        [ObservableProperty] private bool _isBusy = false;

        public DriversViewModel()
        {
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
    }
}
