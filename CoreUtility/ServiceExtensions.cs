using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreUtility
{
    public static class ServiceExtensions
    {
        public static void ConfigureCoreUtilityServices(this IServiceCollection services)
        {
            services.AddScoped<ExcelTestRunner>();
            services.AddScoped<IExcelUtil, ExcelUtil>();
        }
    }
}
