using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TaskManagerClone.Services;
using TaskManagerClone.ViewModels;

namespace TaskManagerClone
{
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Services
            services.AddSingleton<IHardwareMonitorService, HardwareMonitorService>();

            // ViewModels
            services.AddSingleton<MainViewModel>();

            // Windows
            services.AddSingleton<MainWindow>();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
