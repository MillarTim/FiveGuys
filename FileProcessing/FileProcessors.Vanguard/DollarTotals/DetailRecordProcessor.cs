using System;
using System.IO;
using System.Xml.Serialization;

using CSS.Connector.FileProcessing;
using CSS.Connector.FileProcessing.Core;

namespace CSS.Connector.FileProcessors.Vanguard.DollarTotals
{
	public class DetailRecordProcessor : IRecordProcessor
	{
		private bool _firstRecordHasBeenProcessed = false;

		FileService _fileService;
		FileService FileService
		{
			get { if (_fileService == null) _fileService = new FileService(); return _fileService; }
		}

		DollarTotals _dollarTotals;

		public void ProcessRecord(string recordXml)
		{
			Detail detail = null;
			try
			{
				FileEventProcessors.TotalRecords++;
				_dollarTotals = (DollarTotals)(new XmlSerializer(typeof(DollarTotals))).Deserialize(new StringReader(recordXml));
				if (!_firstRecordHasBeenProcessed) ProcessFirstRecord();
				detail = (Detail)_dollarTotals.BlockingRecord;

				if (detail.DebitAggregateDollarTotalFormatted != 0.0M || detail.CreditAggregateDollarTotalFormatted != 0.0M)
				{
					ExpectedActivity expectedActivity = new ExpectedActivity();
					expectedActivity.TransactionType = FileService.GetTypeMapping(detail.FileIdentifier, "TRT");
					if (expectedActivity.TransactionType == null)
					{
						(new LoggingHelper()).Log(string.Format("File Identifier {0} not found in TypeMappings. Record skipped. {1}", detail.FileIdentifier, recordXml), FileEventProcessors.InstanceId, 1, true);
						return;
					}
					expectedActivity.AccountNumber = FileService.GetTypeMapping(detail.FileIdentifier, "ACC");
					expectedActivity.Trailer = string.Format("{0}, {1} File Input", detail.FileIdentifier, detail.FileNumber);
					if (detail.FileIdentifier == "VMCTAXST") // State tax will be dated on next settlement date
					{
						expectedActivity.SettlementDate = new MovementServiceHelper().GetNextSettlementDate(DateTime.Parse(_dollarTotals.DateTimeStampFormatted.ToString("yyyy-MM-dd")));
					}

					if (detail.DebitAggregateDollarTotalFormatted != 0.0M)
					{
						expectedActivity.Amount = detail.DebitAggregateDollarTotalFormatted * -1.0M;
						if (detail.FileIdentifier == "VMCTAXSBAL") new MovementServiceHelper().PostExpectedBalance(expectedActivity.Amount, expectedActivity.Trailer, expectedActivity.TransactionType, expectedActivity.AccountNumber);
						else new MovementServiceHelper().PostExpectedActivity(expectedActivity);
					}
					if (detail.CreditAggregateDollarTotalFormatted != 0.0M)
					{
						expectedActivity.Amount = detail.CreditAggregateDollarTotalFormatted;
						if (detail.FileIdentifier == "VMCTAXSBAL") new MovementServiceHelper().PostExpectedBalance(expectedActivity.Amount, expectedActivity.Trailer, expectedActivity.TransactionType, expectedActivity.AccountNumber);
						else new MovementServiceHelper().PostExpectedActivity(expectedActivity);
					}
				}
			}

			catch (Exception e)
			{
				FileEventProcessors.ErrorRecords++;
				(new LoggingHelper()).Log("ProcessRecord",
					string.Format("Error processing file {0}, record {1}, {2}",
					FileEventProcessors.FileName,
					detail?.PhysicalFileRecordNumber,
					e.ToString()
					), FileEventProcessors.InstanceId, 1, true);
			}
		}

		private void ProcessFirstRecord()
		{
			_firstRecordHasBeenProcessed = true;
			FileEventProcessors.DetailRecordFound = true;
			FileEventProcessors.FileTimestamp = _dollarTotals.DateTimeStampFormatted;
			int fileNumber = 0;
			int.TryParse(_dollarTotals.FileNumber, out fileNumber);
			FileEventProcessors.FileNumber = fileNumber;
		}
	}
}
