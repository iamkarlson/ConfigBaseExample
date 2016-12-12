using System;
using NLog;
using System.Configuration;
using System.IO;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Configuration;

namespace ConfigBaseExample {
    public class Program {
        public static void Main() {
            Console.WriteLine("Config test");
            Console.WriteLine($"MyConnectionString: {MyConfig.MyConnectionString}");
            Console.WriteLine($"MyAppSetting: {MyConfig.MyAppSetting}");
            Console.ReadKey();
        }
    }

    public abstract class ConfigBase {
        protected static string _getProp([CallerMemberName] string propertyName = null) {
            return Utils.GetAppSettings(propertyName);
        }
        protected static string _getConnectionString([CallerMemberName] string propertyName = null) {
            return Utils.GetConnectionString(propertyName);
        }
    }

    public class MyConfig: ConfigBase {
        public static string MyConnectionString => _getConnectionString();

        public static string MyAppSetting => _getProp();
    }

    public static class Utils {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Get connection string
        /// </summary>
        /// <param name="key">connection string name</param>
        /// <returns>Connection string without any modification</returns>
        public static string GetConnectionString(string key) {
            try {
                System.Configuration.ConnectionStringSettingsCollection connStringsDict = currentConnectionStrings;

                ConnectionStringSettings connectionString = connStringsDict[key];

                if(connectionString == null) {
                    return "";
                }
                return connectionString.ConnectionString;
            } catch(Exception e) {
                _log.Error(e);
                return string.Empty;
            }
        }
        private static ConnectionStringSettingsCollection currentConnectionStrings {
            get {
                ConnectionStringSettingsCollection connStringsDict;

                if(HttpContext.Current == null) {
                    connStringsDict = currentAppConfiguration.ConnectionStrings.ConnectionStrings;
                } else {
                    connStringsDict = ConfigurationManager.ConnectionStrings;
                }
                return connStringsDict;
            }
        }

        /// <summary>
        /// Configuration of exe
        /// </summary>
        private static Configuration currentAppConfiguration {
            get {
                Configuration currentAppConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                return currentAppConfig;
            }
        }


        /// <summary>
        /// Application settings key value
        /// </summary>
        /// <param name="key">app settings key name</param>
        /// <returns>setting without modification</returns>
        public static string GetAppSettings(string key) {
            try {
                if(HttpContext.Current == null) {
                    var appSettingsSection = currentAppConfiguration.AppSettings;
                    //  App.Config settings
                    KeyValueConfigurationElement setting = appSettingsSection.Settings[key];
                    if(setting == null) {
                        return "";
                    }
                    return setting.Value;
                } else {
                    // Web.config app settings 
                    var appSettings = WebConfigurationManager.AppSettings;
                    var setting = appSettings[key];
                    if(setting == null) {
                        return "";
                    }

                    return setting;
                }
            } catch(Exception e) {
                _log.Error(e);
                return "";
            }
        }

    }


}