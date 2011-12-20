using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using NLog.Config;
using NLog.Targets;

namespace Adalbertus.BudgetPlanner.Core
{
    public class NLogLogger : ILog
    {
        private readonly NLog.Logger _innerLogger;
        private static string _logFileName;
        public static string LogFileName
        {
            get { return _logFileName; }
        }

        public NLogLogger(Type type)
        {
            _innerLogger = NLog.LogManager.GetLogger(type.Name);
            if (NLog.LogManager.Configuration == null)
            {
                LoggingConfiguration config = new LoggingConfiguration();

                FileTarget fileTarget = new FileTarget();
                config.AddTarget("file", fileTarget);                

                _logFileName = AppConfigurationHelper.GetValueOrDefault(AppConfigurationHelper.Keys.LogFileName, "Adalbertus.BudgetPlanner.log");
                fileTarget.FileName = _logFileName;
                fileTarget.Layout = "${date} - ${message}";
                fileTarget.DeleteOldFileOnStartup = true;
                
                LoggingRule rule = new LoggingRule("*", NLog.LogLevel.Trace, fileTarget);                
                config.LoggingRules.Add(rule);

                NLog.LogManager.Configuration = config;                
            }
        }

        public void Error(Exception exception)
        {
            _innerLogger.ErrorException(exception.Message, exception);
        }

        public void Info(string format, params object[] args)
        {
            _innerLogger.Info(format, args);
        }

        public void Warn(string format, params object[] args)
        {
            _innerLogger.Warn(format, args);
        }
    }
}
