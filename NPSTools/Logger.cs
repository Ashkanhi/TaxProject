using System;
using System.Globalization;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System.Collections.Generic;

namespace NPSTools.Logger
{
	public enum ModuleName
	{
        ArghamFinanceUI,
        SaveResult,
        AccountingController,
        AccountingDocumentBusiness,
        ArghamCompany_TaxGoverment,

		
	}

	public static class NPSLogger
	{
		private static Dictionary<ModuleName, Log4NetLogger> _loggers = new Dictionary<ModuleName, Log4NetLogger>();
		private static object loggerLock = new object();
		public static Log4NetLogger GetLogger(ModuleName module)
		{
			try
			{
				if (!_loggers.ContainsKey(module))
				{
					lock (loggerLock)
					{
						if (!_loggers.ContainsKey(module)) // use double locking
						{
							var logger = new Log4NetLogger(module);
							_loggers.Add(module, logger);
						}
					}
				}
				return _loggers[module];
			}
			catch (Exception ex) { }
			return null;
		}

		public static string LogFileLocation()
		{
			var fileLocation = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			var rootDirectoryLocation = Path.Combine(fileLocation, AppDomain.CurrentDomain.BaseDirectory);
			return Path.Combine(rootDirectoryLocation, "Logs");
			
		}
	}
	public class Log4NetLogger
	{
		private readonly ILog _log;
		public string LogFileName;

		public Log4NetLogger(ModuleName module)
		{
			try
			{
			    var logFileName = GetLogFileName(module);
			    var repoName = logFileName + "Repository";



                var patternLayout = new PatternLayout {ConversionPattern = "%date [%thread] %-5level - %message%newline"};
				patternLayout.ActivateOptions();

				var hierarchy = (Hierarchy)LogManager.CreateRepository(repoName);
                hierarchy.Root.Additivity = false;
                var fileAppender = new RollingFileAppender();
				fileAppender.AppendToFile = true;
				fileAppender.RollingStyle = RollingFileAppender.RollingMode.Composite;
				fileAppender.MaxSizeRollBackups = 5;
				fileAppender.MaximumFileSize = "2048KB";
				fileAppender.LockingModel = new FileAppender.MinimalLock();
				//fileAppender.MaxSizeRollBackups = 10;
				fileAppender.PreserveLogFileNameExtension = true;
				fileAppender.StaticLogFileName = true;
				fileAppender.Layout = patternLayout;
				LogFileName = GetLogFIleLocation(module);
				fileAppender.File = LogFileName;
				fileAppender.ActivateOptions();
                hierarchy.Root.AddAppender(fileAppender);
                hierarchy.Configured = true;

                _log = LogManager.GetLogger(repoName, logFileName);
			}
			catch (Exception ex)
			{
			}
		}

		private static string GetLogFIleLocation(ModuleName module)
		{
			var fileName = GetLogFileName(module);
		    fileName += ".txt";

		    var fileLocation = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

			var rootDirectoryLocation = Path.Combine(fileLocation,"ArghamFinance");

			var logDirectoryPath = Path.Combine(rootDirectoryLocation, "Logs");
			var logFileLocation = Path.Combine(logDirectoryPath, fileName);

			if (!Directory.Exists(rootDirectoryLocation))
			{
				var info = Directory.CreateDirectory(rootDirectoryLocation);
				var security = info.GetAccessControl();
				security.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
					FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
					PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
				info.SetAccessControl(security);
			}

			if (!Directory.Exists(logDirectoryPath))
			{
				var info = Directory.CreateDirectory(logDirectoryPath);
				var security = info.GetAccessControl();
				security.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
					FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
					PropagationFlags.InheritOnly, AccessControlType.Allow));
				info.SetAccessControl(security);
			}
			return logFileLocation;
		}

	    private static string GetLogFileName(ModuleName module)
	    {
		
			return module.ToString();
			
	    }

	    #region ILogger Members

		public void LogException(Exception exception)
		{
			if (_log.IsErrorEnabled)
			{
				var message = exception.Message;
				_log.Error(string.Format(CultureInfo.InvariantCulture, "{0}", message), exception);
			}
		}

		public void LogError(string message, Exception exception)
		{
			if (_log.IsErrorEnabled)
			{	
				_log.Error(string.Format(CultureInfo.InvariantCulture, "{0}", message));
				_log.Error(string.Format(CultureInfo.InvariantCulture, "{0}", exception.Message), exception);
			}
		}

		public void LogError(string message)
		{
			if (_log.IsErrorEnabled)
			{
				_log.Error(string.Format(CultureInfo.InvariantCulture, "{0}", message));
			}
		}

		public void LogWarningMessage(string message)
		{
			if (_log.IsWarnEnabled)
			{
				_log.Warn(string.Format(CultureInfo.InvariantCulture, "{0}", message));
			}
		}

		public void LogInfoMessage(string message)
		{

			_log.Info(string.Format(CultureInfo.InvariantCulture, "{0}", message));
		}

		#endregion
	}
}