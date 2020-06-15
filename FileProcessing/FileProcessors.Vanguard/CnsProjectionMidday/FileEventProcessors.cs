using System;
using CSS.Connector.FileProcessing;
using CSS.Connector.FileProcessing.Core;

namespace CSS.Connector.FileProcessors.Vanguard.CnsProjectionMidday
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
		internal static decimal DollarTotal { get; set; }
		internal static DateTime? SettlementDate { get; set; }

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
			DollarTotal = 0.0M;
			SettlementDate = null;
		}

		/// <summary>
		/// Method to process the end of file processing event.
		/// </summary>
		/// <param name="fileCompletionInfo">Status and counts of file processing.</param>
		public void EndOfFileEventProcessor(ref FileCompletionInfo fileCompletionInfo)
		{
			bool didErrorOccur = false;
			string error = null;

			try
			{
				fileCompletionInfo.FileTimestamp = FileTimestamp;
				fileCompletionInfo.FileNumber = FileNumber;
				fileCompletionInfo.TotalRecords = TotalRecords;
				fileCompletionInfo.RecordsFailed = ErrorRecords;
				fileCompletionInfo.RecordsProcessed = TotalRecords - ErrorRecords;

				// Post accumulated amount from file
				if (DollarTotal == 0.0M) return;
				FileService fileService = new FileService();
				string transactionType = fileService.GetTypeMapping("358-ALL", "TRT");
				string accountNumber = fileService.GetTypeMapping("358-ALL", "ACC");

				if (transactionType == null || accountNumber == null)
				{
					didErrorOccur = true;
					error = "File Identifier 358-ALL not found in TypeMappings.";
				}
				string trailer = string.Format("358, {0} File Input", FileNumber);

				new MovementServiceHelper().PostExpectedBalance(DollarTotal, trailer, transactionType, accountNumber, settlementDate:SettlementDate);
			}

			catch (Exception e)
			{
				didErrorOccur = true;
				error = e.ToString();
			}

			if (ErrorRecords > 0 || didErrorOccur)
			{
				if (string.IsNullOrWhiteSpace(error) && ErrorRecords > 0)
					error = string.Format("Error processing file {0}, errors encountered on {1} out of {2} records. See errors in log for more details.",
						FileName,
						ErrorRecords,
						TotalRecords);

				fileCompletionInfo.FatalErrorOccurred = true;
				fileCompletionInfo.FatalErrorMessage = error;
			}

			DollarTotal = 0.0M;
		}
	}
}
