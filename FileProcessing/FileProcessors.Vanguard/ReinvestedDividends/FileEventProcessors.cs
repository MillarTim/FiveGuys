using System;
using CSS.Connector.FileProcessing;

namespace CSS.Connector.FileProcessors.Vanguard.ReinvestedDividends
{
	public class FileEventProcessors : IFileEventProcessors
	{
		internal static string FilePathAndName { get; set; }
		internal static string FileName { get { return System.IO.Path.GetFileName(FilePathAndName); } }
		internal static string InstanceId { get; set; }
		internal static DateTime FileTimestamp { get; set; }
		internal static DateTime? _fileDate;
		internal static DateTime FileDate
		{
			get
			{
				if (_fileDate == null) _fileDate = new DateTime(FileTimestamp.Year, FileTimestamp.Month, FileTimestamp.Day);
				return (DateTime)_fileDate;

			}
		}
		internal static int FileNumber { get; set; }
		internal static bool DetailRecordFound { get; set; } // If no detail records found, read header at end to get date & seq number of file for Processing history screen
		internal static int TotalRecords { get; set; }
		internal static int ErrorRecords { get; set; }

		// fields for posting & related logic
		internal static string SecurityNumber { get; set; }
		internal static string PayableDate { get; set; }
		internal static DateTime PayableDateFormatted { get; set; }
		internal static decimal AggregatedReinvestedDividendAmountFormatted { get; set; }
		internal static bool IsPayableDateUnknown { get; set; }

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
			_fileDate = null;
			FileNumber = 0;
			DetailRecordFound = false;
			TotalRecords = 0;
			ErrorRecords = 0;
			ClearPostingFields();
			// Sometimes the RDIV file comes in late, so a manual entry may have been made earlier in the day for it.  However, once it comes in
			//   that manual entry needs to be zeroed out so the actual data will be shown.  This logic should take care of that:
			DateTime payableDate = DateTime.Now.Date;
			string payableDateString = payableDate.ToString("MM-dd-yy");
			FileEventProcessors.ProcessDetailData(
				"999999999",
				payableDateString,
				payableDate,
				0.0M,
				false);
		}

		internal static void ProcessDetailData(string securityNumber, string payableDate, DateTime payableDateFormatted, decimal aggregatedReinvestedDividendAmountFormatted, bool isPayableDateUnknown)
		{
			if (securityNumber != SecurityNumber || payableDate != PayableDate) PostDetailData();
			SecurityNumber                               = securityNumber;
			PayableDate                                  = payableDate;
			PayableDateFormatted                         = payableDateFormatted;
			AggregatedReinvestedDividendAmountFormatted += aggregatedReinvestedDividendAmountFormatted; // Add to prior rows' values for duplicate rows of same product (If no proir rows for this product it will be adding current amount to zero)
			IsPayableDateUnknown                         = isPayableDateUnknown;
		}

		private static void PostDetailData()
		{
			if (string.IsNullOrWhiteSpace(SecurityNumber)) return;
			new MovementServiceHelper().PostReinvestedDividend(
				PayableDateFormatted,
				FileDate,
				SecurityNumber,
				AggregatedReinvestedDividendAmountFormatted,
				IsPayableDateUnknown);
			ClearPostingFields();
		}

		private static void ClearPostingFields()
		{
			SecurityNumber = string.Empty;
			PayableDate = string.Empty;
			PayableDateFormatted = DateTime.MinValue;
			AggregatedReinvestedDividendAmountFormatted = 0.0M;
			IsPayableDateUnknown = false;
		}

		/// <summary>
		/// Method to process the end of file processing event.
		/// </summary>
		/// <param name="fileCompletionInfo">Status and counts of file processing.</param>
		public void EndOfFileEventProcessor(ref FileCompletionInfo fileCompletionInfo)
		{
			PostDetailData(); // Post data for the last security detail record
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
