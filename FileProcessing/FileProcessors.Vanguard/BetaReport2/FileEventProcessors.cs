using System;
using CSS.Connector.FileProcessing;

namespace CSS.Connector.FileProcessors.Vanguard.BetaReport2
{
	public class FileEventProcessors : IFileEventProcessors
	{
		internal static string FilePathAndName { get; set; }
		internal static string FileName { get { return System.IO.Path.GetFileName(FilePathAndName); } }
		internal static string InstanceId { get; set; }
		internal static DateTime FileTimestamp { get; set; }
		internal static int FileNumber { get; set; }
		internal static bool DetailRecordFound { get; set; } // If no detail records found, read header at end to get date & seq number of file for Processing history screen
		internal static int TotalRecords { get; set; }
		internal static int ErrorRecords { get; set; }

		/// <summary>
		/// Method to process when file processing begins.
		/// </summary>
		/// <param name="filePath">The full name and path of the file to process.</param>
		public void BeginningOfFileEventProcessor(string filePath, string instanceId)
		{
			FilePathAndName = filePath;
			InstanceId = instanceId;
			// Static values will be retained from prior run, so initialize:
			FileTimestamp = DateTime.MinValue;
			FileNumber = 0;
			DetailRecordFound = false;
			TotalRecords = 0;
			ErrorRecords = 0;
		}

		/// <summary>
		/// Method to process the end of file processing event.
		/// </summary>
		/// <param name="fileCompletionInfo">Status and counts of file processing.</param>
		public void EndOfFileEventProcessor(ref FileCompletionInfo fileCompletionInfo)
		{
			fileCompletionInfo.FileTimestamp = FileTimestamp;
			fileCompletionInfo.FileNumber = FileNumber;
			fileCompletionInfo.TotalRecords = TotalRecords;
			fileCompletionInfo.RecordsFailed = ErrorRecords;
			fileCompletionInfo.RecordsProcessed = TotalRecords - ErrorRecords;
			if (ErrorRecords > 0)
			{
				fileCompletionInfo.FatalErrorOccurred = true;
				fileCompletionInfo.FatalErrorMessage = 
					string.Format("Error processing file {0}, errors encountered on {1} out of {2} records. See errors in log for more details.",
						FileName,
						ErrorRecords,
						TotalRecords);
			}
		}
	}
}
