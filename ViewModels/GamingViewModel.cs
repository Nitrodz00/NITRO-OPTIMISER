using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NitroOptimizer.Services.Interfaces;
using System.Threading.Tasks;

namespace NitroOptimizer.ViewModels
{
    public partial class GamingViewModel : ObservableObject
    {
        private readonly ITweakService _tweakService;

        [ObservableProperty] private string _statusMessage = string.Empty;

        public GamingViewModel(ITweakService tweakService)
        {
            _tweakService = tweakService;
        }

        [RelayCommand]
        private async Task ApplyGamingTweaksAsync()
        {
            StatusMessage = "⏳ Applying Gaming Tweaks (Disabling GameDVR, HAGS)...";
            var result = await _tweakService.ApplyGamingOptimizationsAsync();
            StatusMessage = result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
        }
    }
}
