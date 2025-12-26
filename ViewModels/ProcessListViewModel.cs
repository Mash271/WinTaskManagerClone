using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.ComponentModel;
using TaskManagerClone.Models;
using System.Windows;

namespace TaskManagerClone.ViewModels
{
    public class ProcessListViewModel : ViewModelBase
    {
        private readonly System.Timers.Timer _timer;
        public ObservableCollection<ProcessInfo> Processes { get; } = new ObservableCollection<ProcessInfo>();

        private ProcessInfo? _selectedProcess;
        public ProcessInfo? SelectedProcess
        {
            get => _selectedProcess;
            set => SetProperty(ref _selectedProcess, value);
        }

        public ProcessListViewModel()
        {
            LoadProcesses();
            _timer = new System.Timers.Timer(3000); // 3 seconds refresh
            _timer.Elapsed += (s, e) => Application.Current.Dispatcher.Invoke(LoadProcesses);
            _timer.Start();
        }

        private void LoadProcesses()
        {
            try
            {
                var runningProcesses = Process.GetProcesses();
                
                // Remove processes no longer running
                var currentIds = runningProcesses.Select(p => p.Id).ToHashSet();
                for (int i = Processes.Count - 1; i >= 0; i--)
                {
                    if (!currentIds.Contains(Processes[i].Id))
                        Processes.RemoveAt(i);
                }

                // Add or update processes
                foreach (var p in runningProcesses)
                {
                    var existing = Processes.FirstOrDefault(x => x.Id == p.Id);
                    if (existing != null)
                    {
                        try { existing.MemoryUsage = p.WorkingSet64 / (1024.0 * 1024.0); } catch { }
                    }
                    else
                    {
                        try
                        {
                            Processes.Add(new ProcessInfo
                            {
                                Id = p.Id,
                                Name = p.ProcessName,
                                MemoryUsage = p.WorkingSet64 / (1024.0 * 1024.0),
                                FileName = p.MainModule?.FileName ?? ""
                            });
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading processes: {ex.Message}");
            }
        }

        public bool EndTask(ProcessInfo processInfo)
        {
            try
            {
                var p = Process.GetProcessById(processInfo.Id);
                p.Kill();
                Processes.Remove(processInfo);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not end task: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public void OpenFileLocation(ProcessInfo processInfo)
        {
            try
            {
                if (!string.IsNullOrEmpty(processInfo.FileName))
                {
                    Process.Start("explorer.exe", $"/select,\"{processInfo.FileName}\"");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open file location: {ex.Message}");
            }
        }

        public void SearchOnline(ProcessInfo processInfo)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"https://www.google.com/search?q={processInfo.Name}",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not search online: {ex.Message}");
            }
        }
    }
}
