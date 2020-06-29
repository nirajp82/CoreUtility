using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreUtility.Console
{
    internal static class AppConfigSettings
    {
        #region Members
        static IConfiguration _configuration;
       
        internal static IConfiguration Configuration
        {
            set
            {
                _configuration = value;
                LoggerDirPath = GetValue<string>("Logging:LoggerPath");
                InitMinLogLevel();
            }
            get
            {
                return _configuration;
            }
        }

        public static string LoggerDirPath { get; private set; }
        public static LogLevel MinLogLevel { get; private set; }
        #endregion


        #region Private Methods
        private static T GetValue<T>(string key)
        {
            return Configuration.GetValue<T>(key);
        }

        private static void InitMinLogLevel() 
        {
            string defaultLogLevel = GetValue<string>("Logging:LogLevel:Default");
            LogLevel minLogLevel = LogLevel.None;

            if (!Enum.TryParse<LogLevel>(defaultLogLevel, true, out minLogLevel))
                minLogLevel = LogLevel.Warning;

            MinLogLevel = minLogLevel;
        }
        #endregion
    }
}
