using System;
using System.Diagnostics;
using System.Management;
using System.Threading.Tasks;
using NitroOptimizer.Models;
using NitroOptimizer.Services.Interfaces;

namespace NitroOptimizer.Services
{
    public class NetworkService : INetworkService
    {
        public Task<TweakResult> FlushDnsAsync()
        {
            return RunCommandAsync("ipconfig", "/flushdns", "DNS Flushed successfully");
        }

        public Task<TweakResult> ResetWinsockAsync()
        {
            return RunCommandAsync("netsh", "winsock reset", "Winsock Reset successfully");
        }

        public Task<TweakResult> ResetTcpIpAsync()
        {
            return RunCommandAsync("netsh", "int ip reset", "TCP/IP Reset successfully");
        }

        public Task<TweakResult> ChangeDnsAsync(string primary, string secondary)
        {
            return Task.Run(() =>
            {
                try
                {
                    using (ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration"))
                    {
                        ManagementObjectCollection objMOC = objMC.GetInstances();
                        foreach (ManagementObject objMO in objMOC)
                        {
                            if ((bool)objMO["IPEnabled"])
                            {
                                ManagementBaseObject objdns = objMO.GetMethodParameters("SetDNSServerSearchOrder");
                                if (objdns != null)
                                {
                                    objdns["DNSServerSearchOrder"] = new string[] { primary, secondary };
                                    objMO.InvokeMethod("SetDNSServerSearchOrder", objdns, null);
                                }
                            }
                        }
                    }
                    return new TweakResult { Success = true, Message = $"DNS changed to {primary}, {secondary}" };
                }
                catch (Exception ex)
                {
                    return new TweakResult { Success = false, Message = $"Failed to change DNS: {ex.Message}" };
                }
            });
        }

        private async Task<TweakResult> RunCommandAsync(string fileName, string arguments, string successMessage)
        {
            try
            {
                var processInfo = new ProcessStartInfo(fileName, arguments)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };

                using (var process = Process.Start(processInfo))
                {
                    if (process != null)
                    {
                        await process.WaitForExitAsync();
                        return new TweakResult { Success = true, Message = successMessage };
                    }
                    return new TweakResult { Success = false, Message = "Process failed to start." };
                }
            }
            catch (Exception ex)
            {
                return new TweakResult { Success = false, Message = $"Error: {ex.Message}" };
            }
        }
    }
}
