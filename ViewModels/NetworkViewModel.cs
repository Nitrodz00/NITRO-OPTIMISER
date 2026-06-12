using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NitroOptimizer.Models;
using NitroOptimizer.Services.Interfaces;
using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace NitroOptimizer.ViewModels
{
    public partial class NetworkViewModel : ObservableObject
    {
        private readonly INetworkService _networkService;
        private readonly ITweakService _tweakService;

        [ObservableProperty] private string _statusMessage = string.Empty;

        [ObservableProperty] private string _cloudflarePing = "-";
        [ObservableProperty] private string _googlePing = "-";
        [ObservableProperty] private string _quad9Ping = "-";
        [ObservableProperty] private string _adGuardPing = "-";
        [ObservableProperty] private string _level3Ping = "-";

        public NetworkViewModel(INetworkService networkService, ITweakService tweakService)
        {
            _networkService = networkService;
            _tweakService = tweakService;
        }

        private async Task<long> PingDnsAsync(string ip)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync(ip, 1200);
                    if (reply.Status == IPStatus.Success)
                    {
                        return reply.RoundtripTime;
                    }
                }
            }
            catch
            {
                // Fail silently
            }
            return 9999; // Represents timeout
        }

        [RelayCommand]
        private async Task BenchmarkDnsAsync()
        {
            StatusMessage = "⏳ Benchmarking DNS servers (sending pings)...";
            CloudflarePing = "Testing...";
            GooglePing = "Testing...";
            Quad9Ping = "Testing...";
            AdGuardPing = "Testing...";
            Level3Ping = "Testing...";

            var cf = await PingDnsAsync("1.1.1.1");
            CloudflarePing = cf == 9999 ? "Timeout" : $"{cf} ms";

            var go = await PingDnsAsync("8.8.8.8");
            GooglePing = go == 9999 ? "Timeout" : $"{go} ms";

            var q9 = await PingDnsAsync("9.9.9.9");
            Quad9Ping = q9 == 9999 ? "Timeout" : $"{q9} ms";

            var ag = await PingDnsAsync("94.140.14.14");
            AdGuardPing = ag == 9999 ? "Timeout" : $"{ag} ms";

            var l3 = await PingDnsAsync("4.2.2.2");
            Level3Ping = l3 == 9999 ? "Timeout" : $"{l3} ms";

            StatusMessage = "✅ DNS Benchmark completed.";
        }

        [RelayCommand]
        private async Task ApplyFastestDnsAsync()
        {
            StatusMessage = "⏳ Testing ping to determine the fastest DNS...";
            
            var cf = await PingDnsAsync("1.1.1.1");
            var go = await PingDnsAsync("8.8.8.8");
            var q9 = await PingDnsAsync("9.9.9.9");
            var ag = await PingDnsAsync("94.140.14.14");
            var l3 = await PingDnsAsync("4.2.2.2");

            long min = Math.Min(cf, Math.Min(go, Math.Min(q9, Math.Min(ag, l3))));

            if (min == 9999)
            {
                StatusMessage = "❌ All DNS servers timed out or are unreachable.";
                return;
            }

            TweakResult result;
            if (min == cf)
            {
                StatusMessage = "⏳ Applying Cloudflare DNS (Fastest)...";
                result = await _networkService.ChangeDnsAsync("1.1.1.1", "1.0.0.1");
            }
            else if (min == go)
            {
                StatusMessage = "⏳ Applying Google DNS (Fastest)...";
                result = await _networkService.ChangeDnsAsync("8.8.8.8", "8.8.4.4");
            }
            else if (min == q9)
            {
                StatusMessage = "⏳ Applying Quad9 DNS (Fastest)...";
                result = await _networkService.ChangeDnsAsync("9.9.9.9", "149.112.112.112");
            }
            else if (min == ag)
            {
                StatusMessage = "⏳ Applying AdGuard DNS (Fastest)...";
                result = await _networkService.ChangeDnsAsync("94.140.14.14", "94.140.15.15");
            }
            else
            {
                StatusMessage = "⏳ Applying Level3 DNS (Fastest)...";
                result = await _networkService.ChangeDnsAsync("4.2.2.2", "4.2.2.1");
            }

            StatusMessage = result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
        }

        [RelayCommand]
        private async Task ApplyTcpIpTweaksAsync()
        {
            StatusMessage = "⏳ Applying Advanced TCP/IP tweaks (Autotuning, ECN, Throttling)...";
            var result = await _tweakService.ApplyTcpIpTweaksAsync();
            StatusMessage = result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
        }

        [RelayCommand]
        private async Task RestoreTcpIpTweaksAsync()
        {
            StatusMessage = "⏳ Reverting TCP/IP settings to default...";
            var result = await _tweakService.RestoreTcpIpTweaksAsync();
            StatusMessage = result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
        }

        [RelayCommand]
        private async Task FlushDnsAsync()
        {
            StatusMessage = "⏳ Flushing DNS...";
            var result = await _networkService.FlushDnsAsync();
            StatusMessage = result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
        }

        [RelayCommand]
        private async Task ResetWinsockAsync()
        {
            StatusMessage = "⏳ Resetting Winsock...";
            var result = await _networkService.ResetWinsockAsync();
            StatusMessage = result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
        }

        [RelayCommand]
        private async Task ResetTcpIpAsync()
        {
            StatusMessage = "⏳ Resetting TCP/IP...";
            var result = await _networkService.ResetTcpIpAsync();
            StatusMessage = result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
        }

        [RelayCommand]
        private async Task SetGoogleDnsAsync()
        {
            StatusMessage = "⏳ Applying Google DNS...";
            var result = await _networkService.ChangeDnsAsync("8.8.8.8", "8.8.4.4");
            StatusMessage = result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
        }
        
        [RelayCommand]
        private async Task SetCloudflareDnsAsync()
        {
            StatusMessage = "⏳ Applying Cloudflare DNS...";
            var result = await _networkService.ChangeDnsAsync("1.1.1.1", "1.0.0.1");
            StatusMessage = result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
        }
    }
}
