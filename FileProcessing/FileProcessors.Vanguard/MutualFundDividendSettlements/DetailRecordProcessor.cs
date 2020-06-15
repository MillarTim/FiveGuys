using System;
using System.IO;
using System.Xml.Serialization;

using CSS.Connector.FileProcessing;
using CSS.Connector.FileProcessing.Core;

namespace CSS.Connector.FileProcessors.Vanguard.MutualFundDividendSettlements
{
	public class DetailRecordProcessor : IRecordProcessor
	{
		private bool _firstRecordHasBeenProcessed = false;

		FileService _fileService;
		FileService FileService
		{
			get { if (_fileService == null) _fileService = new FileService(); return _fileService; }
		}

		MutualFundDividendSettlement _mutualFundDividendSettlement;

		public void ProcessRecord(string recordXml)
		{
			Detail detail = null;
			try
			{
				FileEventProcessors.TotalRecords++;
				_mutualFundDividendSettlement = (MutualFundDividendSettlement)(new XmlSerializer(typeof(MutualFundDividendSettlement))).Deserialize(new StringReader(recordXml));
				if (!_firstRecordHasBeenProcessed) ProcessFirstRecord();
				detail = (Detail)_mutualFundDividendSettlement.DividendSettlement.BlockingRecord;


				StockAndCashMovement movement = new StockAndCashMovement();

				string key = "193-ALL";
				movement.TransactionType = FileService.GetTypeMapping(key, "TRT");
				movement.AccountNumber = FileService.GetTypeMapping(key, "ACC");
				if (movement.TransactionType == null || movement.AccountNumber == null)
				{
					(new LoggingHelper()).Log(string.Format("Transaction Type {0} not found in TypeMappings table. Record skipped. {1}", key, recordXml), FileEventProcessors.InstanceId, 1, true);
					return;
				}

				movement.Amount = (!string.IsNullOrWhiteSpace(detail.SettlementValue) ?
					detail.SettlementValueFormatted :
					0.0M);

				movement.Cusip = detail.SecurityIssueNumber;

				movement.Quantity = 0.0M;

				// Since this is the backside, reverse sign for credits(ind=4), not debits
				// Scratch the above.  After testing it was decided to reverse the sign on the debits
				if (detail.PayReceiveCode == "2")
				{
					movement.Amount *= -1.0M;
					movement.Quantity *= -1.0M;
				}

				movement.PostingDate = (detail.RecordDate != "00000000" && !string.IsNullOrWhiteSpace(detail.RecordDate) ? detail.RecordDateFormatted : DateTime.Now.Date);
				movement.SettlementDate = detail.SettlementDateFormatted;

				movement.Trailer = string.Format("193:");

				new MovementServiceHelper().PostForecastStockAndCash(movement);
			}

			catch(Exception e)
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
			FileEventProcessors.FileTimestamp = _mutualFundDividendSettlement.DateTimeCreatedFormatted;
			int fileNumber = 0;
			int.TryParse(_mutualFundDividendSettlement.ApplicationMultiCycleCounter, out fileNumber);
			FileEventProcessors.FileNumber = fileNumber;
		}
	}
}
