using System;

using CSS.Cloud.Common;
using CSS.Cloud.Framework;

namespace CSS.Connector.FileProcessing.Core
{

	public class LoggingHelper
	{
		public static bool UseVerboseLogging { get; set; }

		private FileService _fileService;
		private FileService FileService
		{
			get
			{
				if (_fileService == null) _fileService = new FileService();
				return _fileService;
			}
		}

		/// <summary>
		/// Log an error or status message
		/// </summary>
		/// <param name="logMessage">Message to log</param>
		/// <param name="loggingSeverity">1 = Always log; 2 = Log if verbose logging is set</param>
		public void Log(string method, string logMessage, int loggingSeverity, bool isError)
        {
			Log(method, logMessage,	null, loggingSeverity, isError);
        }

		/// <summary>
		/// Log an error or status message
		/// </summary>
		/// <param name="logMessage">Message to log</param>
		/// <param name="instanceId">If processing a specific file, include the instand ID</param>
		/// <param name="loggingSeverity">1 = Always log; 2 = Log if verbose logging is set</param>
		public void Log(string method, string logMessage, string instanceId, int loggingSeverity, bool isError)
		{
			try
			{
				// Log errors with Framework Logging to the Errors table in the Logging DB
				if (isError)
				{
					LogWithFrameworkLogger(method, logMessage, instanceId);
				}
				// Only log less important status messages if verbose logging is on
				if (loggingSeverity > 1 && !UseVerboseLogging) return;
				// This will log errors and status messages to the FileEventLogs table in the Connector DB (so errors will be logged to both tables).
				FileService.WriteFileEventLog(new FileProcessing.Models.FileEventLog { Message = logMessage, TimeStamp = DateTime.UtcNow, InstanceId = instanceId });
			}

			catch (Exception)
			{
			}

			return;
		}

		private void LogWithFrameworkLogger(string method, string message, string instanceId)
		{
			if (instanceId != null) message += "; InstanceId: " + instanceId;
			string token = new ClientTokenGrantHelper(ServiceFabricManager.GetConfigParameter("Config", "FileProcessingConfig", "AuthorityURL")).Token;
			Logger.PostError("CSS.Connector.FileProcessing", method, message, "", "", token);
		}
	}
}