using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

using CSS.Connector.FileProcessing;
using CSS.Connector.FileProcessing.Core;

namespace CSS.Connector.FileProcessors.Vanguard.MutualFundSettlements
{
	public class DetailRecordProcessor : IRecordProcessor
	{
		private bool _firstRecordHasBeenProcessed = false;

		private bool _sameDaySettlement;

		FileService _fileService;
		FileService FileService
		{
			get { if (_fileService == null) _fileService = new FileService(); return _fileService; }
		}

		MutualFundSettlement _mutualFundSettlement;

		public void ProcessRecord(string recordXml)
		{
			Detail detail = null;
			try
			{
				FileEventProcessors.TotalRecords++;
				_mutualFundSettlement = (MutualFundSettlement)(new XmlSerializer(typeof(MutualFundSettlement))).Deserialize(new StringReader(recordXml));
				if (!_firstRecordHasBeenProcessed) ProcessFirstRecord();
				detail = (Detail)_mutualFundSettlement.Settlement.BlockingRecord;

				StockAndCashMovement movement = new StockAndCashMovement();

				string fileType = (_sameDaySettlement ? "364" : "365");
				string key = fileType + "-" + (detail.BuySellCode == "4" ? "CR" : "DR");
				movement.TransactionType = FileService.GetTypeMapping(key, "TRT");
				movement.AccountNumber = FileService.GetTypeMapping(key, "ACC");
				if (movement.TransactionType == null || movement.AccountNumber == null)
				{
					(new LoggingHelper()).Log(string.Format("Transaction Type {0} not found in TypeMappings table. Record skipped. {1}", key, recordXml), FileEventProcessors.InstanceId, 1, true);
					return;
				}

				movement.Cusip = detail.SecurityIssueId;

				movement.Amount = (!string.IsNullOrWhiteSpace(detail.SettlementMoneyAmount) ?
					detail.SettlementMoneyAmountFormatted :
					0.0M);

				movement.Quantity = (string.IsNullOrWhiteSpace(detail.ShareQuantity) ? 0.0M : detail.ShareQuantityFormatted);

				// Since this is the backside, reverse sign for credits(ind=4), not debits
				// Scratch the above.  After testing it was decided to reverse the sign on the debits
				if (detail.BuySellCode == "2")
				{
					movement.Amount *= -1.0M;
					movement.Quantity *= -1.0M;
				}

				movement.PostingDate = _mutualFundSettlement.Settlement.TransmissionDateFormatted;
				movement.SettlementDate = detail.SettlementDateFormatted;

				movement.Trailer = fileType + ":" + detail.ControlNumber;

				new MovementServiceHelper().PostForecastStockAndCash(movement);
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
			FileEventProcessors.FileTimestamp = _mutualFundSettlement.DateTimeCreatedFormatted;
			int fileNumber = 0;
			int.TryParse(_mutualFundSettlement.ApplicationSMultiCycleCounter, out fileNumber);
			FileEventProcessors.FileNumber = fileNumber;

			// 364 file is same day settlement file; 365 is next day settlement file;
			_sameDaySettlement = Regex.IsMatch(FileEventProcessors.FileName, @"(?<=^[^\.]*)364\.");
		}
	}
}
