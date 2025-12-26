using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using LibreHardwareMonitor.Hardware;

namespace TaskManagerClone.Services
{
    public class HardwareMonitorService : IHardwareMonitorService
    {
        private readonly Computer _computer;
        private readonly System.Threading.Timer _updateTimer;
        private readonly PerformanceCounter _cpuCounter;
        private readonly PerformanceCounter _ramCounter;
        
        private double _cpuUsage;
        private double _cpuTemp;
        private double _gpuUsage;
        private double _gpuTemp;
        private double _memoryUsed;
        private double _memoryTotal;
        private double _networkUpload;
        private double _networkDownload;
        
        private long _lastBytesSent;
        private long _lastBytesReceived;
        private DateTime _lastNetworkCheck;

        public event EventHandler<HardwareMetricsEventArgs>? MetricsUpdated;

        public HardwareMonitorService()
        {
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsNetworkEnabled = true,
                IsStorageEnabled = false
            };

            _computer.Open();
            
            // Initialize performance counters
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            
            // Initialize network tracking
            _lastNetworkCheck = DateTime.Now;
            UpdateNetworkBaseline();
            
            _updateTimer = new System.Threading.Timer(UpdateMetrics, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Start()
        {
            _updateTimer.Change(0, 1000); // Update every second
        }

        public void Stop()
        {
            _updateTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void UpdateMetrics(object? state)
        {
            try
            {
                foreach (var hardware in _computer.Hardware)
                {
                    hardware.Update();
                    
                    switch (hardware.HardwareType)
                    {
                        case HardwareType.Cpu:
                            UpdateCpuMetrics(hardware);
                            break;
                        case HardwareType.GpuNvidia:
                        case HardwareType.GpuAmd:
                        case HardwareType.GpuIntel:
                            UpdateGpuMetrics(hardware);
                            break;
                        case HardwareType.Memory:
                            UpdateMemoryMetrics(hardware);
                            break;
                        case HardwareType.Network:
                            UpdateNetworkMetrics(hardware);
                            break;
                    }
                }

                // Use performance counter for CPU as fallback
                _cpuUsage = _cpuCounter.NextValue();
                
                // Calculate available memory
                var availableMemoryMB = _ramCounter.NextValue();
                var totalMemoryInfo = GC.GetGCMemoryInfo();
                _memoryTotal = totalMemoryInfo.TotalAvailableMemoryBytes / (1024.0 * 1024.0 * 1024.0);
                _memoryUsed = _memoryTotal - (availableMemoryMB / 1024.0);

                MetricsUpdated?.Invoke(this, new HardwareMetricsEventArgs
                {
                    CpuUsage = _cpuUsage,
                    CpuTemperature = _cpuTemp,
                    GpuUsage = _gpuUsage,
                    GpuTemperature = _gpuTemp,
                    MemoryUsed = _memoryUsed,
                    MemoryTotal = _memoryTotal,
                    NetworkUpload = _networkUpload,
                    NetworkDownload = _networkDownload
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating metrics: {ex.Message}");
            }
        }

        private void UpdateCpuMetrics(IHardware hardware)
        {
            var loadSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name.Contains("Total"));
            if (loadSensor != null && loadSensor.Value.HasValue)
            {
                _cpuUsage = loadSensor.Value.Value;
            }

            var tempSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature && s.Name.Contains("Package"));
            if (tempSensor != null && tempSensor.Value.HasValue)
            {
                _cpuTemp = tempSensor.Value.Value;
            }
        }

        private void UpdateGpuMetrics(IHardware hardware)
        {
            var loadSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name.Contains("Core"));
            if (loadSensor != null && loadSensor.Value.HasValue)
            {
                _gpuUsage = loadSensor.Value.Value;
            }

            var tempSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature && s.Name.Contains("Core"));
            if (tempSensor != null && tempSensor.Value.HasValue)
            {
                _gpuTemp = tempSensor.Value.Value;
            }
        }

        private void UpdateMemoryMetrics(IHardware hardware)
        {
            var usedMemory = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Data && s.Name.Contains("Used"));
            var availableMemory = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Data && s.Name.Contains("Available"));

            if (usedMemory != null && usedMemory.Value.HasValue)
            {
                _memoryUsed = usedMemory.Value.Value;
            }

            if (availableMemory != null && availableMemory.Value.HasValue)
            {
                _memoryTotal = _memoryUsed + availableMemory.Value.Value;
            }
        }

        private void UpdateNetworkMetrics(IHardware hardware)
        {
            var now = DateTime.Now;
            var timeDiff = (now - _lastNetworkCheck).TotalSeconds;
            
            if (timeDiff < 0.5) return; // Update at most every 0.5 seconds

            long currentBytesSent = 0;
            long currentBytesReceived = 0;

            var uploadSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Throughput && s.Name.Contains("Upload"));
            var downloadSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Throughput && s.Name.Contains("Download"));

            if (uploadSensor != null && uploadSensor.Value.HasValue)
            {
                _networkUpload = uploadSensor.Value.Value / (1024.0 * 1024.0); // Convert to MB/s
            }

            if (downloadSensor != null && downloadSensor.Value.HasValue)
            {
                _networkDownload = downloadSensor.Value.Value / (1024.0 * 1024.0); // Convert to MB/s
            }

            _lastNetworkCheck = now;
        }

        private void UpdateNetworkBaseline()
        {
            try
            {
                var interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
                foreach (var ni in interfaces)
                {
                    if (ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
                    {
                        var stats = ni.GetIPv4Statistics();
                        _lastBytesSent += stats.BytesSent;
                        _lastBytesReceived += stats.BytesReceived;
                    }
                }
            }
            catch { }
        }

        public double GetCpuUsage() => _cpuUsage;
        public double GetCpuTemperature() => _cpuTemp;
        public double GetGpuUsage() => _gpuUsage;
        public double GetGpuTemperature() => _gpuTemp;
        public double GetMemoryUsed() => _memoryUsed;
        public double GetMemoryTotal() => _memoryTotal;
        public double GetNetworkUpload() => _networkUpload;
        public double GetNetworkDownload() => _networkDownload;

        public void Dispose()
        {
            _updateTimer?.Dispose();
            _cpuCounter?.Dispose();
            _ramCounter?.Dispose();
            _computer?.Close();
        }
    }
}
