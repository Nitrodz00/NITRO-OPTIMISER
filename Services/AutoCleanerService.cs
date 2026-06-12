using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using NitroOptimizer.Services.Interfaces;

namespace NitroOptimizer.Services
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
        public MEMORYSTATUSEX()
        {
            this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
        }
    }

    public static class MemoryWin32
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
    }

    public class AutoCleanerService
    {
        private readonly ICleanerService _cleanerService;
        private readonly Timer _monitorTimer;
        private System.Windows.Forms.NotifyIcon? _trayIcon;

        public bool IsAutoCleanerEnabled { get; set; } = true;

        public AutoCleanerService(ICleanerService cleanerService)
        {
            _cleanerService = cleanerService;

            InitializeTrayIcon();

            // Set up monitoring timer (every 10 seconds)
            _monitorTimer = new Timer(10000);
            _monitorTimer.Elapsed += MonitorTimer_Elapsed;
            _monitorTimer.Start();
        }

        private void InitializeTrayIcon()
        {
            try
            {
                _trayIcon = new System.Windows.Forms.NotifyIcon();
                
                // Load icon from Assets
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "icon.ico");
                if (File.Exists(iconPath))
                {
                    _trayIcon.Icon = new System.Drawing.Icon(iconPath);
                }
                else
                {
                    _trayIcon.Icon = System.Drawing.SystemIcons.Application;
                }

                _trayIcon.Text = "NITRO OPTIMISER - Running in background";
                _trayIcon.Visible = true;

                // Simple double-click to restore window
                _trayIcon.DoubleClick += (s, e) =>
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        var mainWindow = App.Current.MainWindow;
                        if (mainWindow != null)
                        {
                            if (mainWindow.WindowState == WindowState.Minimized)
                            {
                                mainWindow.WindowState = WindowState.Normal;
                            }
                            mainWindow.Activate();
                            mainWindow.Show();
                        }
                    });
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize tray icon: {ex.Message}");
            }
        }

        private async void MonitorTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (!IsAutoCleanerEnabled) return;

            try
            {
                var memStatus = new MEMORYSTATUSEX();
                if (MemoryWin32.GlobalMemoryStatusEx(memStatus))
                {
                    if (memStatus.dwMemoryLoad >= 80)
                    {
                        ulong availBeforeBytes = memStatus.ullAvailPhys;

                        // Perform RAM cleanup
                        await _cleanerService.CleanRamAsync();

                        // Query memory status again to see how much we freed
                        var memStatusAfter = new MEMORYSTATUSEX();
                        if (MemoryWin32.GlobalMemoryStatusEx(memStatusAfter))
                        {
                            ulong availAfterBytes = memStatusAfter.ullAvailPhys;
                            if (availAfterBytes > availBeforeBytes)
                            {
                                double freedMB = (availAfterBytes - availBeforeBytes) / 1024.0 / 1024.0;
                                if (freedMB > 10)
                                {
                                    ShowNotification("Auto-Cleaner Active", $"Recovered {freedMB:F0} MB of RAM automatically!");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Auto-Cleaner execution error: {ex.Message}");
            }
        }

        public void ShowNotification(string title, string message)
        {
            if (_trayIcon != null)
            {
                _trayIcon.ShowBalloonTip(3000, title, message, System.Windows.Forms.ToolTipIcon.Info);
            }
        }

        public void Shutdown()
        {
            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
            }
            _monitorTimer.Stop();
            _monitorTimer.Dispose();
        }
    }
}
