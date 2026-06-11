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
        private string _statusText = "Status: Idle";

        public MainViewModel()
        {
            var services = App.AppHost!.Services;
            _restorePointService = services.GetRequiredService<IRestorePointService>();
            _cleanerService = services.GetRequiredService<ICleanerService>();
            _networkService = services.GetRequiredService<INetworkService>();
            _tweakService = services.GetRequiredService<ITweakService>();

            // Set default view
            CurrentView = services.GetRequiredService<DashboardViewModel>();
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
