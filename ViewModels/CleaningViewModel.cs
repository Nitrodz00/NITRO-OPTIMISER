using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NitroOptimizer.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace NitroOptimizer.ViewModels
{
    public partial class CleaningViewModel : ObservableObject
    {
        private readonly ICleanerService _cleanerService;

        [ObservableProperty] private string _statusMessage = "Ready to clean.";
        [ObservableProperty] private bool _isBusy = false;
        [ObservableProperty] private string _totalFreed = "0 MB";
        [ObservableProperty] private int _filesDeleted = 0;
        [ObservableProperty] private double _progressValue = 0;

        public CleaningViewModel(ICleanerService cleanerService)
        {
            _cleanerService = cleanerService;
        }

        private string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1048576) return $"{bytes / 1024.0:F1} KB";
            return $"{bytes / 1048576.0:F1} MB";
        }

        [RelayCommand]
        private async Task CleanTempAsync()
        {
            IsBusy = true; ProgressValue = 0;
            StatusMessage = "⏳ Cleaning Temp Files...";
            var freed = await _cleanerService.CleanTempFilesAsync();
            TotalFreed = FormatBytes(freed);
            StatusMessage = $"✅ Temp cleaned! Freed: {TotalFreed}";
            ProgressValue = 100; IsBusy = false;
        }

        [RelayCommand]
        private async Task CleanDirectXAsync()
        {
            IsBusy = true; ProgressValue = 0;
            StatusMessage = "⏳ Cleaning DirectX Shader Cache...";
            var freed = await _cleanerService.CleanDirectXCacheAsync();
            TotalFreed = FormatBytes(freed);
            StatusMessage = $"✅ DirectX Cache cleaned! Freed: {TotalFreed}";
            ProgressValue = 100; IsBusy = false;
        }

        [RelayCommand]
        private async Task CleanWindowsUpdateAsync()
        {
            IsBusy = true; ProgressValue = 0;
            StatusMessage = "⏳ Cleaning Windows Update Cache...";
            var freed = await _cleanerService.CleanWindowsUpdateCacheAsync();
            TotalFreed = FormatBytes(freed);
            StatusMessage = $"✅ Windows Update Cache cleaned! Freed: {TotalFreed}";
            ProgressValue = 100; IsBusy = false;
        }

        [RelayCommand]
        private async Task CleanGpuAsync()
        {
            IsBusy = true; ProgressValue = 0;
            StatusMessage = "⏳ Cleaning NVIDIA & AMD GPU Cache...";
            var freed = await _cleanerService.CleanGpuCacheAsync();
            TotalFreed = FormatBytes(freed);
            StatusMessage = $"✅ GPU Cache cleaned! Freed: {TotalFreed}";
            ProgressValue = 100; IsBusy = false;
        }

        [RelayCommand]
        private async Task CleanFiveMAsync()
        {
            IsBusy = true; ProgressValue = 0;
            StatusMessage = "⏳ Cleaning FiveM / CitizenFX Cache...";
            var freed = await _cleanerService.CleanFiveMCacheAsync();
            TotalFreed = FormatBytes(freed);
            StatusMessage = freed > 0 
                ? $"✅ FiveM Cache cleaned! Freed: {TotalFreed}" 
                : "ℹ️ FiveM not found or cache already empty.";
            ProgressValue = 100; IsBusy = false;
        }

        [RelayCommand]
        private async Task CleanRamAsync()
        {
            IsBusy = true; ProgressValue = 0;
            StatusMessage = "⏳ Cleaning RAM Standby List and Cache...";
            await _cleanerService.CleanRamAsync();
            StatusMessage = $"✅ RAM Cached Memory Cleaned!";
            ProgressValue = 100; IsBusy = false;
        }

        [RelayCommand]
        private async Task CleanAllAsync()
        {
            IsBusy = true;
            long total = 0;
            
            StatusMessage = "⏳ Cleaning Temp Files..."; ProgressValue = 10;
            total += await _cleanerService.CleanTempFilesAsync();

            StatusMessage = "⏳ Cleaning DirectX Cache..."; ProgressValue = 40;
            total += await _cleanerService.CleanDirectXCacheAsync();

            StatusMessage = "⏳ Cleaning GPU Cache..."; ProgressValue = 55;
            total += await _cleanerService.CleanGpuCacheAsync();

            StatusMessage = "⏳ Cleaning Windows Update Cache..."; ProgressValue = 70;
            total += await _cleanerService.CleanWindowsUpdateCacheAsync();

            StatusMessage = "⏳ Cleaning FiveM Cache..."; ProgressValue = 70;
            total += await _cleanerService.CleanFiveMCacheAsync();

            StatusMessage = "⏳ Cleaning RAM Cache..."; ProgressValue = 90;
            await _cleanerService.CleanRamAsync();

            TotalFreed = FormatBytes(total) + " + RAM Cache";
            StatusMessage = $"✅ All cleaned! Total Freed: {TotalFreed}";
            ProgressValue = 100; IsBusy = false;
        }
    }
}
