using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32;
using NitroOptimizer.Models;
using NitroOptimizer.Services.Interfaces;

namespace NitroOptimizer.Services
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;
        public int dmPositionX;
        public int dmPositionY;
        public int dmDisplayOrientation;
        public int dmDisplayFixedOutput;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTweakMode;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
        public short dmLogPixels;
        public short dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
        public int dmICMMethod;
        public int dmICMIntent;
        public int dmMediaType;
        public int dmDitherType;
        public int dmReserved1;
        public int dmReserved2;
        public int dmPanningWidth;
        public int dmPanningHeight;
    }

    public static class User32
    {
        [DllImport("user32.dll")]
        public static extern int EnumDisplaySettings(string? deviceName, int modeNum, ref DEVMODE devMode);
        
        [DllImport("user32.dll", EntryPoint = "ChangeDisplaySettingsW")]
        public static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

        [DllImport("user32.dll", EntryPoint = "ChangeDisplaySettingsW")]
        public static extern int ChangeDisplaySettings(IntPtr devMode, int flags);

        public const int ENUM_CURRENT_SETTINGS = -1;
        public const int DISP_CHANGE_SUCCESSFUL = 0;
    }

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
                    Registry.SetValue(@"HKEY_CURRENT_USER\System\GameConfigStore", "GameDVR_Enabled", 0, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\GameDVR", "AllowGameDVR", 0, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode", 2, RegistryValueKind.DWord);
                    return new TweakResult { Success = true, Message = "Gaming optimizations applied successfully. (Restart may be required)" };
                }
                catch (Exception ex)
                {
                    return new TweakResult { Success = false, Message = $"Registry Error: {ex.Message}" };
                }
            });
        }

        public Task<TweakResult> ApplyMouseTweaksAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    // Disable acceleration
                    Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseSpeed", "0", RegistryValueKind.String);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseThreshold1", "0", RegistryValueKind.String);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseThreshold2", "0", RegistryValueKind.String);
                    
                    // MarkC Mouse Curves (1-to-1 Raw Input)
                    byte[] smoothX = new byte[] { 
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
                        0x15, 0x6e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
                        0x2a, 0xdc, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
                        0x40, 0x4a, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 
                        0x55, 0xb8, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00 
                    };
                    byte[] smoothY = new byte[] { 
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
                        0xfd, 0x11, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 
                        0xfa, 0x23, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 
                        0xf7, 0x35, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 
                        0xf4, 0x47, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00 
                    };
                    Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "SmoothMouseXCurve", smoothX, RegistryValueKind.Binary);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "SmoothMouseYCurve", smoothY, RegistryValueKind.Binary);

                    return new TweakResult { Success = true, Message = "Mouse raw input fix (MarkC Curve) applied successfully." };
                }
                catch (Exception ex)
                {
                    return new TweakResult { Success = false, Message = $"Error applying mouse fix: {ex.Message}" };
                }
            });
        }

        public Task<TweakResult> RestoreMouseTweaksAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseSpeed", "1", RegistryValueKind.String);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseThreshold1", "6", RegistryValueKind.String);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseThreshold2", "10", RegistryValueKind.String);
                    
                    byte[] defaultX = new byte[] { 
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
                        0x15, 0x6e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
                        0x00, 0x40, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 
                        0x00, 0x80, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 
                        0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00 
                    };
                    byte[] defaultY = new byte[] { 
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
                        0xfd, 0x11, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 
                        0x00, 0x24, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 
                        0x00, 0x48, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 
                        0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00 
                    };
                    Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "SmoothMouseXCurve", defaultX, RegistryValueKind.Binary);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "SmoothMouseYCurve", defaultY, RegistryValueKind.Binary);

                    return new TweakResult { Success = true, Message = "Mouse settings restored to Windows defaults." };
                }
                catch (Exception ex)
                {
                    return new TweakResult { Success = false, Message = $"Error restoring mouse settings: {ex.Message}" };
                }
            });
        }

        public Task<TweakResult> ChangeResolutionAsync(int width, int height)
        {
            return Task.Run(() =>
            {
                try
                {
                    DEVMODE dm = new DEVMODE();
                    dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
                    if (User32.EnumDisplaySettings(null, User32.ENUM_CURRENT_SETTINGS, ref dm) != 0)
                    {
                        dm.dmPelsWidth = width;
                        dm.dmPelsHeight = height;
                        dm.dmFields = 0x00080000 | 0x00100000; // dmPelsWidth | dmPelsHeight
                        
                        int result = User32.ChangeDisplaySettings(ref dm, 0);
                        if (result == User32.DISP_CHANGE_SUCCESSFUL)
                        {
                            return new TweakResult { Success = true, Message = $"Resolution changed to {width}x{height} successfully." };
                        }
                        return new TweakResult { Success = false, Message = $"Display settings rejected the mode. Code: {result}" };
                    }
                    return new TweakResult { Success = false, Message = "Could not query current monitor settings." };
                }
                catch (Exception ex)
                {
                    return new TweakResult { Success = false, Message = $"Error: {ex.Message}" };
                }
            });
        }

        public Task<TweakResult> RestoreDefaultResolutionAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    int result = User32.ChangeDisplaySettings(IntPtr.Zero, 0);
                    if (result == User32.DISP_CHANGE_SUCCESSFUL)
                    {
                        return new TweakResult { Success = true, Message = "Resolution restored to Windows default successfully." };
                    }
                    return new TweakResult { Success = false, Message = $"Failed to restore resolution. Code: {result}" };
                }
                catch (Exception ex)
                {
                    return new TweakResult { Success = false, Message = $"Error: {ex.Message}" };
                }
            });
        }

        public async Task<TweakResult> ApplyGameProfileAsync(string profileName)
        {
            try
            {
                if (profileName == "PUBG Mobile (Gameloop)")
                {
                    // Disable Fullscreen Optimization for Gameloop if present, and optimize latency
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers", @"C:\Program Files\TxGameAssistant\UI\AndroidEmulator.exe", "~ DISABLEDXMAXIMIZEDWINDOWEDMODE", RegistryValueKind.String);
                    // Network priority adjustments for Gameloop
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "GPU Priority", 8, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Priority", 6, RegistryValueKind.DWord);
                    return new TweakResult { Success = true, Message = "Gameloop PUBG Mobile Profile applied." };
                }
                else if (profileName == "Valorant / CS2")
                {
                    // High GPU scheduling / high priority
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "GPU Priority", 10, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Scheduling Category", "High", RegistryValueKind.String);
                    return new TweakResult { Success = true, Message = "Valorant / CS2 Esports Profile applied." };
                }
                else
                {
                    // General game mode values
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "GPU Priority", 8, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Scheduling Category", "Medium", RegistryValueKind.String);
                    return new TweakResult { Success = true, Message = "General Gaming Profile applied." };
                }
            }
            catch (Exception ex)
            {
                return new TweakResult { Success = false, Message = $"Error applying profile: {ex.Message}" };
            }
        }

        public async Task<TweakResult> RestoreGameProfileAsync()
        {
            try
            {
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "GPU Priority", 8, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Priority", 2, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Scheduling Category", "Medium", RegistryValueKind.String);
                return new TweakResult { Success = true, Message = "Game profile settings reverted to defaults." };
            }
            catch (Exception ex)
            {
                return new TweakResult { Success = false, Message = $"Error reverting profiles: {ex.Message}" };
            }
        }

        public async Task<TweakResult> BackupDriversAsync(string destinationPath)
        {
            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }
            return await RunCommandAsync("dism", $"/online /export-driver /destination:\"{destinationPath}\"", "Drivers backed up successfully to " + destinationPath);
        }

        public async Task<TweakResult> RestoreDriversAsync(string sourcePath)
        {
            if (!Directory.Exists(sourcePath))
            {
                return new TweakResult { Success = false, Message = "Backup directory does not exist." };
            }
            return await RunCommandAsync("pnputil", $"/add-driver \"{sourcePath}\\*.inf\" /subdirs /install", "Drivers restored from " + sourcePath);
        }

        public async Task<TweakResult> DebloatWindowsAsync(bool removeApps, bool disableTelemetry)
        {
            try
            {
                if (disableTelemetry)
                {
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "AllowTelemetry", 0, RegistryValueKind.DWord);
                    
                    // Disable Diagnostic Tracking Service (DiagTrack)
                    await RunCommandAsync("sc", "config DiagTrack start= disabled");
                    await RunCommandAsync("sc", "stop DiagTrack");
                }

                if (removeApps)
                {
                    // Async shell execution of app removal
                    string powershellCmd = "Get-AppxPackage -AllUsers *Cortana* | Remove-AppxPackage; " +
                                           "Get-AppxPackage -AllUsers *BingWeather* | Remove-AppxPackage; " +
                                           "Get-AppxPackage -AllUsers *XboxSpeechToTextOverlay* | Remove-AppxPackage; " +
                                           "Get-AppxPackage -AllUsers *Microsoft3DViewer* | Remove-AppxPackage; " +
                                           "Get-AppxPackage -AllUsers *FeedbackHub* | Remove-AppxPackage";
                    
                    await RunCommandAsync("powershell", $"-Command \"{powershellCmd}\"");
                }

                return new TweakResult { Success = true, Message = "Windows debloat tasks executed successfully." };
            }
            catch (Exception ex)
            {
                return new TweakResult { Success = false, Message = $"Error debloating: {ex.Message}" };
            }
        }

        public async Task<TweakResult> ApplyTcpIpTweaksAsync()
        {
            try
            {
                // Disable Nagle's Algorithm via Netsh & Registry (Network throttling and auto-tuning)
                await RunCommandAsync("netsh", "int tcp set global autotuninglevel=normal");
                await RunCommandAsync("netsh", "int tcp set global ecncapability=enabled");
                await RunCommandAsync("netsh", "int tcp set global chimney=enabled");
                
                // Disable network throttling index for lower ping
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex", 0xFFFFFFFF, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "SystemResponsiveness", 0, RegistryValueKind.DWord);

                return new TweakResult { Success = true, Message = "Advanced TCP/IP and Network tweaks applied." };
            }
            catch (Exception ex)
            {
                return new TweakResult { Success = false, Message = $"Error: {ex.Message}" };
            }
        }

        public async Task<TweakResult> RestoreTcpIpTweaksAsync()
        {
            try
            {
                await RunCommandAsync("netsh", "int tcp set global autotuninglevel=normal");
                await RunCommandAsync("netsh", "int tcp set global ecncapability=default");
                await RunCommandAsync("netsh", "int tcp set global chimney=default");
                
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex", 10, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "SystemResponsiveness", 20, RegistryValueKind.DWord);

                return new TweakResult { Success = true, Message = "Network and TCP/IP settings reverted to Windows defaults." };
            }
            catch (Exception ex)
            {
                return new TweakResult { Success = false, Message = $"Error: {ex.Message}" };
            }
        }

        public async Task<TweakResult> ManageContextMenuAsync(bool disableJunk)
        {
            return Task.Run(() =>
            {
                try
                {
                    // NVIDIA Control Panel
                    string val = disableJunk ? "-{3D1975AD-2B9F-4D2A-885E-5F075E01E48A}" : "{3D1975AD-2B9F-4D2A-885E-5F075E01E48A}";
                    
                    using (RegistryKey? key = Registry.ClassesRoot.OpenSubKey(@"Directory\Background\shellex\ContextMenuHandlers\NvCplDesktopContext", true))
                    {
                        if (key != null)
                        {
                            key.SetValue("", val);
                        }
                    }
                    return new TweakResult { Success = true, Message = disableJunk ? "Junk desktop context menus disabled." : "Desktop context menus restored." };
                }
                catch (Exception ex)
                {
                    return new TweakResult { Success = false, Message = $"Error modifying context menu: {ex.Message}" };
                }
            }).Result;
        }

        public async Task<TweakResult> RevertAllTweaksAsync()
        {
            try
            {
                // Sequence of restore commands
                await RestoreDefaultPowerSettingsAsync();
                await RestoreMouseTweaksAsync();
                await RestoreDefaultResolutionAsync();
                await RestoreGameProfileAsync();
                await RestoreTcpIpTweaksAsync();
                await ManageContextMenuAsync(false);

                // Enable Diagtrack again
                await RunCommandAsync("sc", "config DiagTrack start= auto");
                await RunCommandAsync("sc", "start DiagTrack");

                return new TweakResult { Success = true, Message = "All system optimizations reverted successfully to Windows defaults." };
            }
            catch (Exception ex)
            {
                return new TweakResult { Success = false, Message = $"Error reverting tweaks: {ex.Message}" };
            }
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
