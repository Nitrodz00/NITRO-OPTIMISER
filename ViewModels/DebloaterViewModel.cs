using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NitroOptimizer.Services.Interfaces;
using System.Threading.Tasks;

namespace NitroOptimizer.ViewModels
{
    public partial class DebloaterViewModel : ObservableObject
    {
        private readonly ITweakService _tweakService;

        [ObservableProperty] private bool _removeBloatwareApps = true;
        [ObservableProperty] private bool _disableTelemetryAndDataCollection = true;
        [ObservableProperty] private string _statusMessage = string.Empty;
        [ObservableProperty] private bool _isBusy = false;

        public DebloaterViewModel(ITweakService tweakService)
        {
            _tweakService = tweakService;
        }

        [RelayCommand]
        private async Task RunDebloatAsync()
        {
            if (IsBusy) return;
            
            IsBusy = true;
            StatusMessage = "⏳ Running Debloat Tasks (this might take a few minutes)...";

            var result = await _tweakService.DebloatWindowsAsync(RemoveBloatwareApps, DisableTelemetryAndDataCollection);
            
            StatusMessage = result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
            IsBusy = false;
        }
    }
}
