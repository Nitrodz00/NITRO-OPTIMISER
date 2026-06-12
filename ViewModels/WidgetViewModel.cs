using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NitroOptimizer.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;

namespace NitroOptimizer.ViewModels
{
    public partial class WidgetViewModel : ObservableObject
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetSystemTimes(out FILETIME lpIdleTime, out FILETIME lpKernelTime, out FILETIME lpUserTime);

        [StructLayout(LayoutKind.Sequential)]
        private struct FILETIME
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        }

        private ulong _prevIdleTime = 0;
        private ulong _prevKernelTime = 0;
        private ulong _prevUserTime = 0;

        private Timer? _updateTimer;
        private NetworkInterface? _activeInterface;
        private long _lastBytesReceived = 0;
        private Window? _widgetWindow;

        [ObservableProperty] private string _cpuUsage = "0%";
        [ObservableProperty] private string _gpuUsage = "0%";
        [ObservableProperty] private string _ramUsage = "0%";
        [ObservableProperty] private string _netSpeed = "0.0 KB/s";

        public WidgetViewModel()
        {
            InitializeCounters();
            
            _updateTimer = new Timer(1000);
            _updateTimer.Elapsed += UpdateTimer_Elapsed;
        }

        private void InitializeCounters()
        {
            try
            {
                _activeInterface = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(ni => ni.OperationalStatus == OperationalStatus.Up && 
                                          ni.NetworkInterfaceType != NetworkInterfaceType.Loopback);
                if (_activeInterface != null)
                {
                    _lastBytesReceived = _activeInterface.GetIPStatistics().BytesReceived;
                }
            }
            catch { }
        }

        private float GetCpuUsage()
        {
            if (!GetSystemTimes(out var idleTime, out var kernelTime, out var userTime))
                return 0;

            ulong idle = ((ulong)idleTime.dwHighDateTime << 32) | idleTime.dwLowDateTime;
            ulong kernel = ((ulong)kernelTime.dwHighDateTime << 32) | kernelTime.dwLowDateTime;
            ulong user = ((ulong)userTime.dwHighDateTime << 32) | userTime.dwLowDateTime;

            if (_prevIdleTime == 0 && _prevKernelTime == 0 && _prevUserTime == 0)
            {
                _prevIdleTime = idle;
                _prevKernelTime = kernel;
                _prevUserTime = user;
                return 0;
            }

            ulong diffIdle = idle - _prevIdleTime;
            ulong diffKernel = kernel - _prevKernelTime;
            ulong diffUser = user - _prevUserTime;

            _prevIdleTime = idle;
            _prevKernelTime = kernel;
            _prevUserTime = user;

            ulong totalSys = diffKernel + diffUser;
            if (totalSys == 0) return 0;

            double active = totalSys - diffIdle;
            double pct = (active * 100.0) / totalSys;

            if (pct < 0) pct = 0;
            if (pct > 100) pct = 100;

            return (float)pct;
        }

        private float GetGpuUsage()
        {
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT UtilizationPercentage FROM Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine"))
                {
                    double maxUsage = 0;
                    foreach (var obj in searcher.Get())
                    {
                        double usage = Convert.ToDouble(obj["UtilizationPercentage"]);
                        if (usage > maxUsage)
                        {
                            maxUsage = usage;
                        }
                    }
                    return (float)maxUsage;
                }
            }
            catch { }
            return 0;
        }

        private void UpdateTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            // CPU
            float cpu = GetCpuUsage();

            // GPU
            float gpu = GetGpuUsage();
            
            // RAM
            double ram = 0;
            var memStatus = new MEMORYSTATUSEX();
            if (MemoryWin32.GlobalMemoryStatusEx(memStatus))
            {
                ram = memStatus.dwMemoryLoad;
            }

            // Net
            double netKB = 0;
            if (_activeInterface != null)
            {
                try
                {
                    long currentBytes = _activeInterface.GetIPStatistics().BytesReceived;
                    long diff = currentBytes - _lastBytesReceived;
                    _lastBytesReceived = currentBytes;
                    netKB = diff / 1024.0;
                }
                catch { }
            }

            App.Current.Dispatcher.Invoke(() =>
            {
                CpuUsage = $"{cpu:F0}%";
                GpuUsage = $"{gpu:F0}%";
                RamUsage = $"{ram:F0}%";
                NetSpeed = netKB > 1024 
                    ? $"{(netKB / 1024.0):F1} MB/s" 
                    : $"{netKB:F1} KB/s";
            });
        }

        public void ToggleWidget()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (_widgetWindow == null)
                {
                    _widgetWindow = new NitroOptimizer.Views.WidgetWindow();
                    _widgetWindow.DataContext = this;
                    
                    // Handle manual closing
                    _widgetWindow.Closed += (s, e) =>
                    {
                        _updateTimer?.Stop();
                        _widgetWindow = null;
                    };

                    _widgetWindow.Show();
                    _updateTimer?.Start();
                }
                else
                {
                    _widgetWindow.Close();
                }
            });
        }
    }
}
