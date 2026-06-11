using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace NitroOptimizer.ViewModels
{
    public class ProcessItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Memory { get; set; } = string.Empty;
    }

    public partial class ProcessesViewModel : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<ProcessItem> _runningProcesses = new();
        [ObservableProperty] private ProcessItem? _selectedProcess;
        [ObservableProperty] private string _statusMessage = string.Empty;

        public ProcessesViewModel()
        {
            _ = LoadProcessesAsync();
        }

        [RelayCommand]
        private async Task LoadProcessesAsync()
        {
            StatusMessage = "⏳ Loading processes...";
            RunningProcesses.Clear();

            await Task.Run(() =>
            {
                var procs = Process.GetProcesses().OrderByDescending(p => p.WorkingSet64).ToList();
                foreach (var p in procs)
                {
                    try
                    {
                        var item = new ProcessItem
                        {
                            Id = p.Id,
                            Name = p.ProcessName,
                            Memory = $"{p.WorkingSet64 / 1024 / 1024} MB"
                        };
                        App.Current.Dispatcher.Invoke(() => RunningProcesses.Add(item));
                    }
                    catch { } // Ignore access denied
                }
            });
            StatusMessage = $"✅ Loaded {RunningProcesses.Count} processes.";
        }

        [RelayCommand]
        private void KillProcess()
        {
            if (SelectedProcess != null)
            {
                try
                {
                    var p = Process.GetProcessById(SelectedProcess.Id);
                    p.Kill();
                    RunningProcesses.Remove(SelectedProcess);
                    StatusMessage = $"✅ Killed process {SelectedProcess.Name}";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"❌ Error: {ex.Message}";
                }
            }
        }
    }
}
