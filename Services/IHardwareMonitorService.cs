using System;

namespace TaskManagerClone.Services
{
    public interface IHardwareMonitorService : IDisposable
    {
        event EventHandler<HardwareMetricsEventArgs>? MetricsUpdated;
        void Start();
        void Stop();
        
        double GetCpuUsage();
        double GetCpuTemperature();
        double GetGpuUsage();
        double GetGpuTemperature();
        double GetMemoryUsed();
        double GetMemoryTotal();
        double GetNetworkUpload();
        double GetNetworkDownload();
    }

    public class HardwareMetricsEventArgs : EventArgs
    {
        public double CpuUsage { get; set; }
        public double CpuTemperature { get; set; }
        public double GpuUsage { get; set; }
        public double GpuTemperature { get; set; }
        public double MemoryUsed { get; set; }
        public double MemoryTotal { get; set; }
        public double NetworkUpload { get; set; }
        public double NetworkDownload { get; set; }
    }
}
