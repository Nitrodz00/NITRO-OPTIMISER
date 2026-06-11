using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using NitroOptimizer.Services;
using NitroOptimizer.Services.Interfaces;
using NitroOptimizer.ViewModels;

namespace NitroOptimizer
{
    public partial class App : Application
    {
        public static IHost? AppHost { get; private set; }

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    // Services
                    services.AddSingleton<ISystemInfoService, SystemInfoService>();
                    services.AddSingleton<ICleanerService, CleanerService>();
                    services.AddSingleton<ITweakService, TweakService>();
                    services.AddSingleton<INetworkService, NetworkService>();
                    services.AddSingleton<IRestorePointService, RestorePointService>();
                    
                    // ViewModels
                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<DashboardViewModel>();
                    services.AddSingleton<PerformanceViewModel>();
                    services.AddSingleton<CleaningViewModel>();
                    services.AddSingleton<GamingViewModel>();
                    services.AddSingleton<NetworkViewModel>();
                    services.AddSingleton<RepairViewModel>();
                    services.AddSingleton<StartupViewModel>();
                    services.AddSingleton<ProcessesViewModel>();
                    services.AddSingleton<DriversViewModel>();
                    services.AddSingleton<ReportsViewModel>();
                    
                    // Windows
                    services.AddSingleton<MainWindow>();
                })
                .Build();
        }

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            await AppHost!.StartAsync();

            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost!.StopAsync();
            base.OnExit(e);
        }
    }
}
