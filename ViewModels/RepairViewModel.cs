using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NitroOptimizer.ViewModels
{
    public partial class RepairViewModel : ObservableObject
    {
        [ObservableProperty] private string _statusMessage = string.Empty;
        [ObservableProperty] private bool _isBusy = false;

        [RelayCommand]
        private async Task RunSfcScanAsync()
        {
            await RunRepairCommandAsync("sfc", "/scannow", "SFC Scan initiated. This will take some time and run in the background.");
        }

        [RelayCommand]
        private async Task RunDismAsync()
        {
            await RunRepairCommandAsync("dism", "/online /cleanup-image /restorehealth", "DISM RestoreHealth initiated.");
        }

        [RelayCommand]
        private async Task RebuildIconCacheAsync()
        {
            IsBusy = true;
            StatusMessage = "⏳ Rebuilding Icon Cache...";
            await Task.Run(() =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c ie4uinit.exe -show & taskkill /IM explorer.exe /F & DEL /A /Q \"%localappdata%\\IconCache.db\" & DEL /A /F /Q \"%localappdata%\\Microsoft\\Windows\\Explorer\\iconcache*\" & start explorer.exe",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    })?.WaitForExit();
                    StatusMessage = "✅ Icon Cache Rebuilt!";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"❌ Error: {ex.Message}";
                }
            });
            IsBusy = false;
        }

        private async Task RunRepairCommandAsync(string cmd, string args, string successMsg)
        {
            IsBusy = true;
            StatusMessage = $"⏳ Running {cmd}...";
            await Task.Run(() =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = cmd,
                        Arguments = args,
                        CreateNoWindow = false, // Show window so user sees progress for sfc/dism
                        UseShellExecute = true,
                        Verb = "runas"
                    });
                    StatusMessage = $"✅ {successMsg}";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"❌ Error: {ex.Message}";
                }
            });
            IsBusy = false;
        }
    }
}
