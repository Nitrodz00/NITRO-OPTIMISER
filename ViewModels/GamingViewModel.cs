using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NitroOptimizer.Services;
using NitroOptimizer.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace NitroOptimizer.ViewModels
{
    public partial class GamingViewModel : ObservableObject
    {
        private readonly ITweakService _tweakService;
        private readonly GameBoosterService _gameBoosterService;

        [ObservableProperty] private string _statusMessage = string.Empty;

        [ObservableProperty] private bool _isAutoBoostEnabled;

        [ObservableProperty] private string _selectedResolution = "Select Resolution";
        public ObservableCollection<string> Resolutions { get; } = new()
        {
            "Select Resolution",
            "1720x1080",
            "1440x1080",
            "1280x960",
            "1024x768"
        };

        [ObservableProperty] private string _selectedProfile = "Select Profile";
        public ObservableCollection<string> Profiles { get; } = new()
        {
            "Select Profile",
            "PUBG Mobile (Gameloop)",
            "Valorant / CS2",
            "General Gaming"
        };

        public GamingViewModel(ITweakService tweakService, GameBoosterService gameBoosterService)
        {
            _tweakService = tweakService;
            _gameBoosterService = gameBoosterService;
            _isAutoBoostEnabled = _gameBoosterService.IsAutoBoostEnabled;
            
            _gameBoosterService.BoostStatusChanged += OnBoostStatusChanged;
        }

        private void OnBoostStatusChanged(string gameName, bool active)
        {
            StatusMessage = active 
                ? $"🚀 Auto-Boost: Optimized active game [{gameName}]!" 
                : "ℹ️ Auto-Boost: Game closed, system reverted to default.";
        }

        partial void OnIsAutoBoostEnabledChanged(bool value)
        {
            _gameBoosterService.IsAutoBoostEnabled = value;
            StatusMessage = value ? "✅ Background Auto-Boost Monitor Activated." : "ℹ️ Background Auto-Boost Monitor Deactivated.";
        }

        [RelayCommand]
        private async Task ApplyGamingTweaksAsync()
        {
            StatusMessage = "⏳ Applying Gaming Tweaks (Disabling GameDVR, HAGS)...";
            var result = await _tweakService.ApplyGamingOptimizationsAsync();
            StatusMessage = result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
        }

        [RelayCommand]
        private async Task ApplyMouseTweaksAsync()
        {
            StatusMessage = "⏳ Applying Mouse Raw Input Fix...";
            var result = await _tweakService.ApplyMouseTweaksAsync();
            StatusMessage = result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
        }

        [RelayCommand]
        private async Task RestoreMouseTweaksAsync()
        {
            StatusMessage = "⏳ Restoring default Mouse Settings...";
            var result = await _tweakService.RestoreMouseTweaksAsync();
            StatusMessage = result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
        }

        [RelayCommand]
        private async Task ApplyResolutionAsync()
        {
            if (SelectedResolution == "Select Resolution")
            {
                StatusMessage = "⚠️ Please select a resolution first.";
                return;
            }

            var parts = SelectedResolution.Split('x');
            if (parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h))
            {
                StatusMessage = $"⏳ Changing display resolution to {SelectedResolution}...";
                var result = await _tweakService.ChangeResolutionAsync(w, h);
                StatusMessage = result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
            }
        }

        [RelayCommand]
        private async Task RestoreResolutionAsync()
        {
            StatusMessage = "⏳ Restoring default Windows resolution...";
            var result = await _tweakService.RestoreDefaultResolutionAsync();
            StatusMessage = result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
        }

        [RelayCommand]
        private async Task ApplyProfileAsync()
        {
            if (SelectedProfile == "Select Profile")
            {
                StatusMessage = "⚠️ Please select a game profile first.";
                return;
            }

            StatusMessage = $"⏳ Applying game profile for {SelectedProfile}...";
            var result = await _tweakService.ApplyGameProfileAsync(SelectedProfile);
            StatusMessage = result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
        }

        [RelayCommand]
        private async Task RestoreProfileAsync()
        {
            StatusMessage = "⏳ Reverting profile tweaks...";
            var result = await _tweakService.RestoreGameProfileAsync();
            StatusMessage = result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
        }
    }
}
