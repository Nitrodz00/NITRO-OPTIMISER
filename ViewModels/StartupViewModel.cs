using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace NitroOptimizer.ViewModels
{
    public class StartupItem : ObservableObject
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        
        private bool _isEnabled;
        public bool IsEnabled 
        { 
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }
    }

    public partial class StartupViewModel : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<StartupItem> _startupItems = new();
        [ObservableProperty] private string _statusMessage = string.Empty;
        [ObservableProperty] private StartupItem? _selectedItem;

        public StartupViewModel()
        {
            _ = LoadStartupItemsAsync();
        }

        [RelayCommand]
        private async Task LoadStartupItemsAsync()
        {
            StatusMessage = "⏳ Loading startup items...";
            StartupItems.Clear();

            await Task.Run(() =>
            {
                // Load Enabled Items
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false))
                {
                    if (key != null)
                    {
                        foreach (string valueName in key.GetValueNames())
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                StartupItems.Add(new StartupItem
                                {
                                    Name = valueName,
                                    Path = key.GetValue(valueName)?.ToString() ?? "",
                                    IsEnabled = true
                                });
                            });
                        }
                    }
                }

                // Load Disabled Items
                using (RegistryKey disabledKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run_Disabled", false))
                {
                    if (disabledKey != null)
                    {
                        foreach (string valueName in disabledKey.GetValueNames())
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                StartupItems.Add(new StartupItem
                                {
                                    Name = valueName,
                                    Path = disabledKey.GetValue(valueName)?.ToString() ?? "",
                                    IsEnabled = false
                                });
                            });
                        }
                    }
                }
            });
            StatusMessage = $"✅ Found {StartupItems.Count} startup items.";
        }

        [RelayCommand]
        private void ToggleState()
        {
            if (SelectedItem == null) return;

            try
            {
                if (SelectedItem.IsEnabled)
                {
                    // Disable it
                    using (RegistryKey runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                    using (RegistryKey disabledKey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run_Disabled", true))
                    {
                        runKey?.DeleteValue(SelectedItem.Name, false);
                        disabledKey?.SetValue(SelectedItem.Name, SelectedItem.Path);
                        SelectedItem.IsEnabled = false;
                        StatusMessage = $"✅ Disabled {SelectedItem.Name}";
                    }
                }
                else
                {
                    // Enable it
                    using (RegistryKey runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                    using (RegistryKey disabledKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run_Disabled", true))
                    {
                        disabledKey?.DeleteValue(SelectedItem.Name, false);
                        runKey?.SetValue(SelectedItem.Name, SelectedItem.Path);
                        SelectedItem.IsEnabled = true;
                        StatusMessage = $"✅ Enabled {SelectedItem.Name}";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Error: {ex.Message}";
            }
        }
    }
}
