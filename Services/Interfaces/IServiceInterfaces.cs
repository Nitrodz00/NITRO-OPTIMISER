namespace NitroOptimizer.Services.Interfaces
{
    using NitroOptimizer.Models;
    using System.Threading.Tasks;

    public interface ISystemInfoService
    {
        Task<SystemInfoModel> GetSystemInfoAsync();
    }
    
    public interface ICleanerService
    {
        Task<long> CleanTempFilesAsync();
        Task<long> CleanWindowsUpdateCacheAsync();
        Task<long> CleanDirectXCacheAsync();
        Task<long> CleanGpuCacheAsync();
        Task<long> CleanFiveMCacheAsync();
        Task<long> CleanRamAsync();
    }

    public interface ITweakService
    {
        Task<TweakResult> EnableUltimatePerformanceAsync();
        Task<TweakResult> EnableHighPerformanceAsync();
        Task<TweakResult> RestoreDefaultPowerSettingsAsync();
        Task<TweakResult> PreventSleepDuringGamingAsync();
        Task<TweakResult> ApplyGamingOptimizationsAsync();
    }

    public interface INetworkService
    {
        Task<TweakResult> FlushDnsAsync();
        Task<TweakResult> ResetWinsockAsync();
        Task<TweakResult> ResetTcpIpAsync();
        Task<TweakResult> ChangeDnsAsync(string primary, string secondary);
    }

    public interface IRestorePointService
    {
        Task<TweakResult> CreateRestorePointAsync(string description);
    }
}
