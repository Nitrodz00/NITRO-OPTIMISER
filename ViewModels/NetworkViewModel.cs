using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NitroOptimizer.Services.Interfaces;
using System.Threading.Tasks;

namespace NitroOptimizer.ViewModels
{
    public partial class NetworkViewModel : ObservableObject
    {
        private readonly INetworkService _networkService;

        [ObservableProperty] private string _statusMessage = string.Empty;

        public NetworkViewModel(INetworkService networkService)
        {
            _networkService = networkService;
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
