namespace NitroOptimizer.Models
{
    public class SystemInfoModel
    {
        public string ProcessorName { get; set; } = string.Empty;
        public string GraphicsCardName { get; set; } = string.Empty;
        public string WindowsVersion { get; set; } = string.Empty;
        public string Uptime { get; set; } = string.Empty;
        
        // Percentages
        public double CpuUsage { get; set; }
        public double RamUsage { get; set; }
        public double GpuUsage { get; set; }
        public double DiskUsage { get; set; }
        
        // Space
        public string TotalDiskSpace { get; set; } = string.Empty;
        public string FreeDiskSpace { get; set; } = string.Empty;
    }
}
