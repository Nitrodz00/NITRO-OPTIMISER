using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Win32;
using NitroOptimizer.Models;
using NitroOptimizer.Services.Interfaces;

namespace NitroOptimizer.Services
{
    public class TweakService : ITweakService
    {
        public Task<TweakResult> EnableUltimatePerformanceAsync()
        {
            return RunCommandAsync("powercfg", "-duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61", "Ultimate Performance Power Plan added.");
        }

        public Task<TweakResult> EnableHighPerformanceAsync()
        {
            return RunCommandAsync("powercfg", "-setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c", "High Performance Power Plan activated.");
        }

        public Task<TweakResult> RestoreDefaultPowerSettingsAsync()
        {
            return RunCommandAsync("powercfg", "-restoredefaultschemes", "Power settings restored to default.");
        }

        public Task<TweakResult> PreventSleepDuringGamingAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    // Disable sleep (0 means never)
                    RunCommandAsync("powercfg", "-change -standby-timeout-ac 0").Wait();
                    RunCommandAsync("powercfg", "-change -standby-timeout-dc 0").Wait();
                    return new TweakResult { Success = true, Message = "Sleep mode disabled." };
                }
                catch (Exception ex)
                {
                    return new TweakResult { Success = false, Message = $"Error: {ex.Message}" };
                }
            });
        }

        public Task<TweakResult> ApplyGamingOptimizationsAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    // GameDVR off
                    Registry.SetValue(@"HKEY_CURRENT_USER\System\GameConfigStore", "GameDVR_Enabled", 0, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\GameDVR", "AllowGameDVR", 0, RegistryValueKind.DWord);

                    // GPU Scheduling (HAGS) - requires restart
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode", 2, RegistryValueKind.DWord);

                    return new TweakResult { Success = true, Message = "Gaming optimizations applied successfully. (Restart may be required)" };
                }
                catch (Exception ex)
                {
                    return new TweakResult { Success = false, Message = $"Registry Error: {ex.Message}" };
                }
            });
        }

        private async Task<TweakResult> RunCommandAsync(string fileName, string arguments, string successMessage = "Success")
        {
            try
            {
                var processInfo = new ProcessStartInfo(fileName, arguments)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };

                using (var process = Process.Start(processInfo))
                {
                    if (process != null)
                    {
                        await process.WaitForExitAsync();
                        return new TweakResult { Success = true, Message = successMessage };
                    }
                    return new TweakResult { Success = false, Message = "Process failed to start." };
                }
            }
            catch (Exception ex)
            {
                return new TweakResult { Success = false, Message = $"Error: {ex.Message}" };
            }
        }
    }
}
