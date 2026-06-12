using System;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Threading.Tasks;
using NitroOptimizer.Services.Interfaces;

namespace NitroOptimizer.Services
{
    public class GameBoosterService
    {
        private readonly ITweakService _tweakService;
        private readonly ICleanerService _cleanerService;
        private readonly Timer _monitorTimer;
        private bool _isBoosted = false;
        private string _activeGameName = string.Empty;
        private int _activeGamePid = 0;

        private readonly string[] _targetGames = new[]
        {
            "AndroidEmulator",            // Gameloop
            "VALORANT-Win64-Shipping",    // Valorant
            "TslGame",                    // PUBG PC
            "cs2"                         // Counter-Strike 2
        };

        public event Action<string, bool>? BoostStatusChanged;

        public bool IsAutoBoostEnabled { get; set; } = false;

        public GameBoosterService(ITweakService tweakService, ICleanerService cleanerService)
        {
            _tweakService = tweakService;
            _cleanerService = cleanerService;
            
            // Set up monitoring timer (every 5 seconds)
            _monitorTimer = new Timer(5000);
            _monitorTimer.Elapsed += MonitorTimer_Elapsed;
            _monitorTimer.Start();
        }

        private async void MonitorTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (!IsAutoBoostEnabled)
            {
                if (_isBoosted)
                {
                    await RevertBoostAsync();
                }
                return;
            }

            try
            {
                if (_isBoosted)
                {
                    // Check if the current boosted process is still running
                    var proc = Process.GetProcesses().FirstOrDefault(p => p.Id == _activeGamePid);
                    if (proc == null || proc.HasExited)
                    {
                        await RevertBoostAsync();
                    }
                }
                else
                {
                    // Scan for target games
                    foreach (var gameName in _targetGames)
                    {
                        var proc = Process.GetProcessesByName(gameName).FirstOrDefault();
                        if (proc != null && !proc.HasExited)
                        {
                            await ApplyBoostAsync(proc, gameName);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Auto-Boost error: {ex.Message}");
            }
        }

        private async Task ApplyBoostAsync(Process proc, string gameName)
        {
            _isBoosted = true;
            _activeGameName = gameName;
            _activeGamePid = proc.Id;

            try
            {
                // Elevate priority
                proc.PriorityClass = ProcessPriorityClass.High;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to set process priority: {ex.Message}");
            }

            // Optimize power and RAM
            await _tweakService.EnableUltimatePerformanceAsync();
            await _cleanerService.CleanRamAsync();

            BoostStatusChanged?.Invoke(_activeGameName, true);
        }

        private async Task RevertBoostAsync()
        {
            _isBoosted = false;
            var game = _activeGameName;
            _activeGameName = string.Empty;
            _activeGamePid = 0;

            // Restore defaults
            await _tweakService.RestoreDefaultPowerSettingsAsync();

            BoostStatusChanged?.Invoke(game, false);
        }
    }
}
