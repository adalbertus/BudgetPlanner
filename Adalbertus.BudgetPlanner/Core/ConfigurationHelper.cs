using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sys = System.Configuration;

namespace Adalbertus.BudgetPlanner.Core
{
    public static class AppConfigurationHelper
    {
        public struct Keys
        {
            public const string LogFileName = "LogFileName";
        }

        public static string DefaultConnectionString { 
            get
            {
                return Sys.ConfigurationManager.ConnectionStrings["default"].ConnectionString;
            }
        }

        public static T GetValueOrDefault<T>(string key, T defaultValue = default(T))
        {
            if (Sys.ConfigurationManager.AppSettings[key] == null)
            {
                return defaultValue;
            }

            return (T)Convert.ChangeType(Sys.ConfigurationManager.AppSettings[key], typeof(T));
        }

        public static void SaveValue(string key, string value)
        {
            var config = Sys.ConfigurationManager.OpenExeConfiguration(Sys.ConfigurationUserLevel.None);
            if (config.AppSettings.Settings[key] != null)
            {
                config.AppSettings.Settings.Remove(key);
            }
            config.AppSettings.Settings.Add(key, value);
            config.Save(Sys.ConfigurationSaveMode.Modified);

            Sys.ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
