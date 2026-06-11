using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NitroOptimizer.Services.Interfaces;
using System.Threading.Tasks;
using System.Windows;

namespace NitroOptimizer.ViewModels
{
    public partial class PerformanceViewModel : ObservableObject
    {
        private readonly ITweakService _tweakService;

        [ObservableProperty] private string _statusMessage = string.Empty;
        [ObservableProperty] private bool _isBusy = false;

        public PerformanceViewModel(ITweakService tweakService)
        {
            _tweakService = tweakService;
        }

        [RelayCommand]
        private async Task EnableUltimatePerformanceAsync()
        {
            IsBusy = true;
            StatusMessage = "⏳ Activating Ultimate Performance...";
            var result = await _tweakService.EnableUltimatePerformanceAsync();
            StatusMessage = result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
            IsBusy = false;
        }

        [RelayCommand]
        private async Task EnableHighPerformanceAsync()
        {
            IsBusy = true;
            StatusMessage = "⏳ Activating High Performance...";
            var result = await _tweakService.EnableHighPerformanceAsync();
            StatusMessage = result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
            IsBusy = false;
        }

        [RelayCommand]
        private async Task RestoreDefaultPowerAsync()
        {
            IsBusy = true;
            StatusMessage = "⏳ Restoring Default Power Settings...";
            var result = await _tweakService.RestoreDefaultPowerSettingsAsync();
            StatusMessage = result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
            IsBusy = false;
        }

        [RelayCommand]
        private async Task PreventSleepAsync()
        {
            IsBusy = true;
            StatusMessage = "⏳ Disabling Sleep Mode...";
            var result = await _tweakService.PreventSleepDuringGamingAsync();
            StatusMessage = result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
            IsBusy = false;
        }

        [RelayCommand]
        private async Task ApplyGamingOptimizationsAsync()
        {
            IsBusy = true;
            StatusMessage = "⏳ Applying System Gaming Optimizations...";
            var result = await _tweakService.ApplyGamingOptimizationsAsync();
            StatusMessage = result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
            IsBusy = false;
        }
    }
}
