using System;
using System.Collections.ObjectModel;
using System.Windows;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using TaskManagerClone.Services;

namespace TaskManagerClone.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IHardwareMonitorService _hardwareMonitor;
        private const int MaxDataPoints = 60;

        // CPU Properties
        private double _cpuUsage;
        public double CpuUsage
        {
            get => _cpuUsage;
            set => SetProperty(ref _cpuUsage, value);
        }

        private double _cpuTemperature;
        public double CpuTemperature
        {
            get => _cpuTemperature;
            set => SetProperty(ref _cpuTemperature, value);
        }

        // GPU Properties
        private double _gpuUsage;
        public double GpuUsage
        {
            get => _gpuUsage;
            set => SetProperty(ref _gpuUsage, value);
        }

        private double _gpuTemperature;
        public double GpuTemperature
        {
            get => _gpuTemperature;
            set => SetProperty(ref _gpuTemperature, value);
        }

        // Memory Properties
        private double _memoryUsed;
        public double MemoryUsed
        {
            get => _memoryUsed;
            set => SetProperty(ref _memoryUsed, value);
        }

        private double _memoryTotal;
        public double MemoryTotal
        {
            get => _memoryTotal;
            set => SetProperty(ref _memoryTotal, value);
        }

        public double MemoryUsagePercent => _memoryTotal > 0 ? (_memoryUsed / _memoryTotal) * 100 : 0;

        // Network Properties
        private double _networkUpload;
        public double NetworkUpload
        {
            get => _networkUpload;
            set => SetProperty(ref _networkUpload, value);
        }

        private double _networkDownload;
        public double NetworkDownload
        {
            get => _networkDownload;
            set => SetProperty(ref _networkDownload, value);
        }

        // Chart Data
        public ObservableCollection<ObservableValue> CpuChartData { get; }
        public ObservableCollection<ObservableValue> GpuChartData { get; }
        public ObservableCollection<ObservableValue> MemoryChartData { get; }
        public ObservableCollection<ObservableValue> NetworkChartData { get; }

        // Chart Series
        public ISeries[] CpuSeries { get; set; }
        public ISeries[] GpuSeries { get; set; }
        public ISeries[] MemorySeries { get; set; }
        public ISeries[] NetworkSeries { get; set; }

        // Axes
        public Axis[] XAxes { get; set; } = { new Axis { IsVisible = false } };
        public Axis[] YAxesPercent { get; set; } = { new Axis { IsVisible = false, MinLimit = 0, MaxLimit = 100 } };
        public Axis[] YAxesNetwork { get; set; } = { new Axis { IsVisible = false, MinLimit = 0 } };

        public MainViewModel(IHardwareMonitorService hardwareMonitor)
        {
            _hardwareMonitor = hardwareMonitor;

            // Initialize chart data
            CpuChartData = new ObservableCollection<ObservableValue>();
            GpuChartData = new ObservableCollection<ObservableValue>();
            MemoryChartData = new ObservableCollection<ObservableValue>();
            NetworkChartData = new ObservableCollection<ObservableValue>();

            // Initialize with zeros
            for (int i = 0; i < MaxDataPoints; i++)
            {
                CpuChartData.Add(new ObservableValue(0));
                GpuChartData.Add(new ObservableValue(0));
                MemoryChartData.Add(new ObservableValue(0));
                NetworkChartData.Add(new ObservableValue(0));
            }

            // Create chart series
            CpuSeries = new ISeries[]
            {
                new LineSeries<ObservableValue>
                {
                    Values = CpuChartData,
                    Fill = null,
                    GeometrySize = 0,
                    LineSmoothness = 0.5,
                    Stroke = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(
                        new SkiaSharp.SKColor(99, 102, 241)) { StrokeThickness = 2 }
                }
            };

            GpuSeries = new ISeries[]
            {
                new LineSeries<ObservableValue>
                {
                    Values = GpuChartData,
                    Fill = null,
                    GeometrySize = 0,
                    LineSmoothness = 0.5,
                    Stroke = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(
                        new SkiaSharp.SKColor(236, 72, 153)) { StrokeThickness = 2 }
                }
            };

            MemorySeries = new ISeries[]
            {
                new LineSeries<ObservableValue>
                {
                    Values = MemoryChartData,
                    Fill = null,
                    GeometrySize = 0,
                    LineSmoothness = 0.5,
                    Stroke = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(
                        new SkiaSharp.SKColor(34, 197, 94)) { StrokeThickness = 2 }
                }
            };

            NetworkSeries = new ISeries[]
            {
                new LineSeries<ObservableValue>
                {
                    Values = NetworkChartData,
                    Fill = null,
                    GeometrySize = 0,
                    LineSmoothness = 0.5,
                    Stroke = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(
                        new SkiaSharp.SKColor(251, 191, 36)) { StrokeThickness = 2 }
                }
            };

            // Subscribe to hardware monitor events
            _hardwareMonitor.MetricsUpdated += OnMetricsUpdated;
            _hardwareMonitor.Start();
        }

        private void OnMetricsUpdated(object? sender, HardwareMetricsEventArgs e)
        {
            // Update on UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                CpuUsage = e.CpuUsage;
                CpuTemperature = e.CpuTemperature;
                GpuUsage = e.GpuUsage;
                GpuTemperature = e.GpuTemperature;
                MemoryUsed = e.MemoryUsed;
                MemoryTotal = e.MemoryTotal;
                NetworkUpload = e.NetworkUpload;
                NetworkDownload = e.NetworkDownload;

                OnPropertyChanged(nameof(MemoryUsagePercent));

                // Update chart data
                UpdateChartData(CpuChartData, e.CpuUsage);
                UpdateChartData(GpuChartData, e.GpuUsage);
                UpdateChartData(MemoryChartData, MemoryUsagePercent);
                UpdateChartData(NetworkChartData, e.NetworkDownload);
            });
        }

        private void UpdateChartData(ObservableCollection<ObservableValue> data, double value)
        {
            // Remove oldest value
            data.RemoveAt(0);
            // Add new value
            data.Add(new ObservableValue(value));
        }
    }
}