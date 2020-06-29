using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CoreUtility.Console
{
    class Program
    {
        #region Members
        static IServiceProvider _serviceProvider;
        #endregion


        #region Entry Method/
        static void Main(string[] args)
        {
            ILogger<Program> logger = null;
            try
            {
                ConfigureServices();
                logger = _serviceProvider.GetService<ILogger<Program>>();
                logger.LogInformation("Application has started");

                IServiceScope scope = _serviceProvider.CreateScope();
                scope.ServiceProvider.GetRequiredService<ExcelTestRunner>().Run();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
                logger.LogError(ex,"Main.Exception");
            }
            finally
            {
                DisposeServices();
            }
        } 
        #endregion


        #region Private Methods
        private static void ConfigureServices()
        {
            // Build configuration
            AppConfigSettings.Configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", false)
                .Build();

            ServiceCollection services = new ServiceCollection();

            // Add access to generic IConfigurationRoot
            services.AddSingleton(AppConfigSettings.Configuration);

            services.ConfigureCoreUtilityServices();

            ConfigureLogger(services);
            // Add app
            _serviceProvider = services.BuildServiceProvider(true);
        }

        private static void ConfigureLogger(IServiceCollection services)
        {
            var logger = new LoggerConfiguration().WriteTo.File($"{AppConfigSettings.LoggerDirPath}Program{DateTime.Now:_yyMMdd_HHmmss}.log").CreateLogger();
            services.AddLogging(configure => configure.AddSerilog(logger));
            services.Configure<LoggerFilterOptions>(options => options.MinLevel = AppConfigSettings.MinLogLevel);
        }

        private static void DisposeServices()
        {
            if (_serviceProvider == null)
                return;
            if (_serviceProvider is IDisposable)
                ((IDisposable)_serviceProvider).Dispose();
        } 
        #endregion
    }
}
