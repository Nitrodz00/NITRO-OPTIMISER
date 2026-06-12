using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NitroOptimizer.Services.Interfaces;
using System.Threading.Tasks;
using System.Windows;

namespace NitroOptimizer.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IRestorePointService _restorePointService;
        private readonly ICleanerService _cleanerService;
        private readonly INetworkService _networkService;
        private readonly ITweakService _tweakService;

        [ObservableProperty]
        private object? _currentView;

        [ObservableProperty]
        private bool _isOptimizing = false;

        [ObservableProperty]
        private bool _createRestorePoint = true;

        [ObservableProperty]
        private string _statusText = "Status: Ready - Awaiting commands";

        public MainViewModel()
        {
            var services = App.AppHost!.Services;
            _restorePointService = services.GetRequiredService<IRestorePointService>();
            _cleanerService = services.GetRequiredService<ICleanerService>();
            _networkService = services.GetRequiredService<INetworkService>();
            _tweakService = services.GetRequiredService<ITweakService>();

            // Set default view
            CurrentView = services.GetRequiredService<DashboardViewModel>();

            // Check for updates
            _ = CheckForUpdatesAsync();
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("NitroOptimizer-Updater");
                    var response = await client.GetStringAsync("https://api.github.com/repos/Nitrodz00/NITRO-OPTIMISER/releases/latest");
                    
                    using (var doc = System.Text.Json.JsonDocument.Parse(response))
                    {
                        var root = doc.RootElement;
                        if (root.TryGetProperty("tag_name", out var tagProperty))
                        {
                            var latestVersion = tagProperty.GetString()?.Trim().ToLower(); // e.g. "v1.0.0"
                            var currentVersion = "v1.0.1"; // Updated to current release v1.0.1
                            
                            if (latestVersion != null && latestVersion != currentVersion)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    var result = MessageBox.Show(
                                        $"A new update ({latestVersion}) is available!\nWould you like to download it now?",
                                        "NITRO OPTIMISER - Update Available",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Information);
                                        
                                    if (result == MessageBoxResult.Yes)
                                    {
                                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                                        {
                                            FileName = "https://github.com/Nitrodz00/NITRO-OPTIMISER/releases/latest",
                                            UseShellExecute = true
                                        });
                                    }
                                });
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update check failed: {ex.Message}");
            }
        }

        [RelayCommand]
        private void Navigate(string viewName)
        {
            var services = App.AppHost!.Services;
            switch (viewName)
            {
                case "Dashboard":
                    CurrentView = services.GetRequiredService<DashboardViewModel>();
                    break;
                case "Performance":
                    CurrentView = services.GetRequiredService<PerformanceViewModel>();
                    break;
                case "Cleaning":
                    CurrentView = services.GetRequiredService<CleaningViewModel>();
                    break;
                case "Gaming":
                    CurrentView = services.GetRequiredService<GamingViewModel>();
                    break;
                case "Network":
                    CurrentView = services.GetRequiredService<NetworkViewModel>();
                    break;
                case "Repair":
                    CurrentView = services.GetRequiredService<RepairViewModel>();
                    break;
                case "Startup":
                    CurrentView = services.GetRequiredService<StartupViewModel>();
                    break;
                case "Processes":
                    CurrentView = services.GetRequiredService<ProcessesViewModel>();
                    break;
                case "Drivers":
                    CurrentView = services.GetRequiredService<DriversViewModel>();
                    break;
                case "Reports":
                    CurrentView = services.GetRequiredService<ReportsViewModel>();
                    break;
                case "Debloat":
                    CurrentView = services.GetRequiredService<DebloaterViewModel>();
                    break;
            }
        }

        [RelayCommand]
        private async Task SmartOptimizeAsync()
        {
            if (IsOptimizing) return;
            IsOptimizing = true;
            StatusText = "Status: Preparing...";

            if (CreateRestorePoint)
            {
                // 1. Create Restore Point
                StatusText = "Status: Creating Restore Point...";
                await _restorePointService.CreateRestorePointAsync("NitroOptimizer Before Smart Optimize");
            }

            // 2. Clean Temp
            StatusText = "Status: Cleaning Temp Files...";
            await _cleanerService.CleanTempFilesAsync();

            // 3. Clean DirectX Cache
            StatusText = "Status: Cleaning DirectX Cache...";
            await _cleanerService.CleanDirectXCacheAsync();

            // 4. Clean Windows Update Cache
            StatusText = "Status: Cleaning Windows Update Cache...";
            await _cleanerService.CleanWindowsUpdateCacheAsync();

            // 5. Clean FiveM Cache
            StatusText = "Status: Cleaning FiveM Cache...";
            await _cleanerService.CleanFiveMCacheAsync();

            // 6. Flush DNS
            StatusText = "Status: Flushing DNS...";
            await _networkService.FlushDnsAsync();

            // 7. Ultimate Performance
            StatusText = "Status: Enabling Ultimate Performance...";
            await _tweakService.EnableUltimatePerformanceAsync();

            StatusText = "Status: Optimization Complete!";
            MessageBox.Show("Smart Optimization Completed Successfully!\nCheck the reports for details.", "NITRO OPTIMIZER", MessageBoxButton.OK, MessageBoxImage.Information);
            
            IsOptimizing = false;
        }
    }
}
