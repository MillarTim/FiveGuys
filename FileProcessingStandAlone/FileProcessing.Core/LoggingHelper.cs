using System;
using System.IO;

namespace CSS.Connector.FileProcessing.Core
{

	public class LoggingHelper
	{
		public static bool UseVerboseLogging { get; set; }

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
					Log(method, logMessage, instanceId);
				}
				// Only log less important status messages if verbose logging is on
				if (loggingSeverity > 1 && !UseVerboseLogging) return;
				// This will log errors and status messages to the FileEventLogs table in the Connector DB (so errors will be logged to both tables).
				Log(method, logMessage, instanceId);
			}

			catch (Exception)
			{
			}

			return;
		}

		private void Log(string method, string message, string instanceId)
		{
			if (instanceId != null) message += "; InstanceId: " + instanceId;

			// TODO: switch to using lazy load & close/dispose in destructor, etc.
			using (StreamWriter writer = new StreamWriter(@"c:\temp\logging\log.txt"))
			{
				writer.WriteLine(method + ": " + message);
			}
		}
	}
}