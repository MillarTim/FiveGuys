using System;
using CSS.Connector.FileProcessing;
using CSS.Connector.FileProcessing.Core;

namespace CSS.Connector.FileProcessors.Vanguard.SettlementBalances
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
		internal static DateTime LastDtccTimeStamp { get; set; }
		internal static decimal? LastDtccAmount { get; set; }
		internal static DateTime LastNsccTimeStamp { get; set; }
		internal static decimal? LastNsccAmount { get; set; }
		internal static SettlementBalanceDictionary SettlementBalanceDictionary { get; set; }

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
			LastDtccTimeStamp = DateTime.MinValue;
			LastDtccAmount = null;
			LastNsccTimeStamp = DateTime.MinValue;
			LastNsccAmount = null;
			if (SettlementBalanceDictionary == null) SettlementBalanceDictionary = new SettlementBalanceDictionary();
			else SettlementBalanceDictionary.Clear();
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

				FileService fileService = new FileService();
				MovementServiceHelper movementsService = new MovementServiceHelper();
				var settlementBalances = SettlementBalanceDictionary.GetBalances();
				settlementBalances.ForEach(balance =>
				{
					string trtKey = "338-" + balance.BalanceSource + "-" + balance.ActivityCode + "-" + balance.ActivitySubCode;
					string transactionType = fileService.GetTypeMapping(trtKey, "TRT");
					if (string.IsNullOrWhiteSpace(transactionType)) return; // If activity code not defined in TypeMappings, just skip the balance

					string trailer = string.Format("338 file, seq {0}, trtkey {1}", FileNumber, (trtKey.Length > 4 ? trtKey.Substring(4) : trtKey));

					movementsService.PostExpectedBalance(
						balance.Amount,
						trailer,
						transactionType,
						clearingId: balance.ClearingId);
				});
				

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

			SettlementBalanceDictionary.Clear();
		}
	}
}
