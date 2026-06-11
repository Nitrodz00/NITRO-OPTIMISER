using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace NitroOptimizer.ViewModels
{
    public class DriveItem
    {
        public string Name { get; set; } = string.Empty;
        public string UsageText { get; set; } = string.Empty;
        public double UsagePercentage { get; set; }
    }

    public partial class DashboardViewModel : ObservableObject
    {
        [ObservableProperty] private string _systemStatus = "Good";
        [ObservableProperty] private ObservableCollection<DriveItem> _drives = new();

        public DashboardViewModel()
        {
            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            try
            {
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    if (drive.IsReady)
                    {
                        long totalSize = drive.TotalSize;
                        long freeSpace = drive.AvailableFreeSpace;
                        long usedSpace = totalSize - freeSpace;

                        double totalGB = totalSize / 1073741824.0;
                        double freeGB = freeSpace / 1073741824.0;
                        double usedGB = usedSpace / 1073741824.0;

                        double usagePercentage = (totalSize > 0) ? (usedSpace * 100.0 / totalSize) : 0;

                        App.Current.Dispatcher.Invoke(() => Drives.Add(new DriveItem
                        {
                            Name = $"Drive {drive.Name}",
                            UsageText = $"{usedGB:F1} GB / {totalGB:F1} GB",
                            UsagePercentage = usagePercentage
                        }));
                    }
                }
            }
            catch { }
        }
    }
}
