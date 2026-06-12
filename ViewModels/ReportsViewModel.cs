using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace NitroOptimizer.ViewModels
{
    public partial class ReportsViewModel : ObservableObject
    {
        [ObservableProperty] private string _statusMessage = string.Empty;
        [ObservableProperty] private string _reportText = "Click 'Generate Report' to view system details here.";
        [ObservableProperty] private bool _isBusy = false;

        [RelayCommand]
        private async Task GenerateReportAsync()
        {
            IsBusy = true;
            StatusMessage = "⏳ Generating detailed system report...";
            await Task.Run(() =>
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("===========================================");
                    sb.AppendLine("       NITRO OPTIMIZER SYSTEM REPORT       ");
                    sb.AppendLine("===========================================");
                    sb.AppendLine($"Date: {DateTime.Now}");
                    sb.AppendLine($"OS Version: {Environment.OSVersion.VersionString}");
                    sb.AppendLine($"64-Bit OS: {Environment.Is64BitOperatingSystem}");
                    sb.AppendLine($"System Directory: {Environment.SystemDirectory}");
                    sb.AppendLine($"Machine Name: {Environment.MachineName}");
                    sb.AppendLine($"User Name: {Environment.UserName}");
                    sb.AppendLine();
                    
                    sb.AppendLine("--- Processor (CPU) ---");
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name, NumberOfCores, NumberOfLogicalProcessors FROM Win32_Processor"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            sb.AppendLine($"Model: {obj["Name"]}");
                            sb.AppendLine($"Cores: {obj["NumberOfCores"]} / Threads: {obj["NumberOfLogicalProcessors"]}");
                        }
                    }
                    
                    sb.AppendLine();
                    sb.AppendLine("--- Memory (RAM) ---");
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Capacity, Speed, Manufacturer FROM Win32_PhysicalMemory"))
                    {
                        long totalRam = 0;
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            if (obj["Capacity"] != null)
                            {
                                long ram = Convert.ToInt64(obj["Capacity"]);
                                totalRam += ram;
                                sb.AppendLine($"Module: {ram / 1073741824} GB - {obj["Speed"]} MHz - {obj["Manufacturer"]}");
                            }
                        }
                        sb.AppendLine($"Total Installed RAM: {totalRam / 1073741824} GB");
                    }

                    sb.AppendLine();
                    sb.AppendLine("--- Motherboard ---");
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Manufacturer, Product, Version FROM Win32_BaseBoard"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            sb.AppendLine($"Manufacturer: {obj["Manufacturer"]}");
                            sb.AppendLine($"Model: {obj["Product"]} (Rev {obj["Version"]})");
                        }
                    }

                    sb.AppendLine();
                    sb.AppendLine("--- Video Controller (GPU) ---");
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name, AdapterRAM, DriverVersion FROM Win32_VideoController"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            sb.AppendLine($"GPU: {obj["Name"]}");
                            if (obj["AdapterRAM"] != null)
                            {
                                long ram = Convert.ToInt64(obj["AdapterRAM"]);
                                sb.AppendLine($"VRAM: {ram / 1048576} MB");
                            }
                            sb.AppendLine($"Driver Version: {obj["DriverVersion"]}");
                        }
                    }

                    sb.AppendLine();
                    sb.AppendLine("--- Disk Drives ---");
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Model, Size FROM Win32_DiskDrive"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            sb.AppendLine($"Model: {obj["Model"]}");
                            if (obj["Size"] != null)
                            {
                                long size = Convert.ToInt64(obj["Size"]);
                                sb.AppendLine($"Size: {size / 1073741824} GB");
                            }
                        }
                    }

                    App.Current.Dispatcher.Invoke(() => ReportText = sb.ToString());
                    StatusMessage = $"✅ Report generated successfully.";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"❌ Error: {ex.Message}";
                }
            });
            IsBusy = false;
        }

        [RelayCommand]
        private void SaveReport()
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "NitroOptimizer_Report.txt");
                File.WriteAllText(path, ReportText);
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
                StatusMessage = $"✅ Report saved to Desktop: NitroOptimizer_Report.txt";
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Error saving report: {ex.Message}";
            }
        }

        private string GetCpuName()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return obj["Name"]?.ToString()?.Trim() ?? "Unknown Processor";
                    }
                }
            }
            catch { }
            return "Unknown Processor";
        }

        private string GetGpuName()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return obj["Name"]?.ToString()?.Trim() ?? "Unknown GPU";
                    }
                }
            }
            catch { }
            return "Unknown GPU";
        }

        private string GetRamInfo()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Capacity, Speed FROM Win32_PhysicalMemory"))
                {
                    long totalBytes = 0;
                    long speed = 0;
                    int count = 0;
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        if (obj["Capacity"] != null)
                        {
                            totalBytes += Convert.ToInt64(obj["Capacity"]);
                            speed = Convert.ToInt64(obj["Speed"]);
                            count++;
                        }
                    }
                    if (totalBytes > 0)
                    {
                        return $"{totalBytes / 1073741824} GB ({count}x Modules) @ {speed} MHz";
                    }
                }
            }
            catch { }
            return "Unknown Memory";
        }

        private string GetMotherboardInfo()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Product FROM Win32_BaseBoard"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return $"{obj["Manufacturer"]} {obj["Product"]}";
                    }
                }
            }
            catch { }
            return "Unknown Motherboard";
        }

        [RelayCommand]
        private async Task ExportSpecsCardAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            StatusMessage = "⏳ Generating system specifications card image...";

            await Task.Run(() =>
            {
                try
                {
                    string cpu = GetCpuName();
                    string gpu = GetGpuName();
                    string ram = GetRamInfo();
                    string mobo = GetMotherboardInfo();
                    string os = Environment.OSVersion.VersionString;

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        // Outer Card Border
                        var card = new System.Windows.Controls.Border
                        {
                            Width = 600,
                            Height = 350,
                            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x0C, 0x0C, 0x12)),
                            BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0x00, 0xFF)), // Neon Magenta
                            BorderThickness = new System.Windows.Thickness(2),
                            CornerRadius = new System.Windows.CornerRadius(12),
                            Padding = new System.Windows.Thickness(25)
                        };

                        // Card Layout Grid
                        var grid = new System.Windows.Controls.Grid();
                        grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = System.Windows.GridLength.Auto });
                        grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
                        grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = System.Windows.GridLength.Auto });

                        // Header Title
                        var title = new System.Windows.Controls.TextBlock
                        {
                            Text = "NITRO OPTIMISER - SYSTEM SPECS",
                            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x00, 0xFF, 0xFF)), // Neon Cyan
                            FontSize = 20,
                            FontWeight = System.Windows.FontWeights.Bold,
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                            Margin = new System.Windows.Thickness(0, 0, 0, 20)
                        };
                        System.Windows.Controls.Grid.SetRow(title, 0);
                        grid.Children.Add(title);

                        // Spec rows panel
                        var panel = new System.Windows.Controls.StackPanel
                        {
                            VerticalAlignment = System.Windows.VerticalAlignment.Center
                        };

                        var items = new (string Label, string Value)[]
                        {
                            ("Processor:", cpu),
                            ("Graphics Card:", gpu),
                            ("System Memory:", ram),
                            ("Motherboard:", mobo),
                            ("Operating System:", os)
                        };

                        foreach (var item in items)
                        {
                            var row = new System.Windows.Controls.Grid { Margin = new System.Windows.Thickness(0, 6, 0, 6) };
                            row.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new System.Windows.GridLength(140) });
                            row.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });

                            var lbl = new System.Windows.Controls.TextBlock
                            {
                                Text = item.Label,
                                Foreground = System.Windows.Media.Brushes.Gray,
                                FontSize = 13,
                                FontWeight = System.Windows.FontWeights.SemiBold
                            };
                            System.Windows.Controls.Grid.SetColumn(lbl, 0);
                            row.Children.Add(lbl);

                            var val = new System.Windows.Controls.TextBlock
                            {
                                Text = item.Value,
                                Foreground = System.Windows.Media.Brushes.White,
                                FontSize = 13,
                                FontWeight = System.Windows.FontWeights.Bold,
                                TextWrapping = System.Windows.TextWrapping.Wrap
                            };
                            System.Windows.Controls.Grid.SetColumn(val, 1);
                            row.Children.Add(val);

                            panel.Children.Add(row);
                        }

                        System.Windows.Controls.Grid.SetRow(panel, 1);
                        grid.Children.Add(panel);

                        // Footer watermark
                        var footer = new System.Windows.Controls.TextBlock
                        {
                            Text = "Generated by NITRO OPTIMISER 🚀",
                            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0x00, 0xFF)), // Magenta
                            FontSize = 11,
                            FontStyle = System.Windows.FontStyles.Italic,
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                            Margin = new System.Windows.Thickness(0, 15, 0, 0)
                        };
                        System.Windows.Controls.Grid.SetRow(footer, 2);
                        grid.Children.Add(footer);

                        card.Child = grid;

                        // Force measurement and layout arrange to render elements
                        card.Measure(new System.Windows.Size(600, 350));
                        card.Arrange(new System.Windows.Rect(0, 0, 600, 350));
                        card.UpdateLayout();

                        var rtb = new System.Windows.Media.Imaging.RenderTargetBitmap(600, 350, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                        rtb.Render(card);

                        var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                        encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(rtb));

                        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        string specCardPath = Path.Combine(desktopPath, "NitroOptimizer_Specs.png");

                        using (var stream = File.Create(specCardPath))
                        {
                            encoder.Save(stream);
                        }

                        StatusMessage = "✅ Specs Card saved to Desktop: NitroOptimizer_Specs.png";

                        Process.Start(new ProcessStartInfo
                        {
                            FileName = specCardPath,
                            UseShellExecute = true
                        });
                    });
                }
                catch (Exception ex)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        StatusMessage = $"❌ Error generating image: {ex.Message}";
                    });
                }
            });

            IsBusy = false;
        }
    }
}
