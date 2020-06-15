using System;
using System.IO;
using System.Xml.Serialization;

using CSS.Connector.FileProcessing;
using CSS.Connector.FileProcessing.Core;

namespace CSS.Connector.FileProcessors.Vanguard.MutualFundActivity
{
	public class DetailRecordProcessor : IRecordProcessor
	{
		private bool _firstRecordHasBeenProcessed = false;

		FileService _fileService;
		FileService FileService
		{
			get { if (_fileService == null) _fileService = new FileService(); return _fileService; }
		}

		MutualFundActivity _mutualFundActivity;

		public void ProcessRecord(string recordXml)
		{
			Detail detail = null;
			try
			{
				FileEventProcessors.TotalRecords++;
				_mutualFundActivity = (MutualFundActivity)(new XmlSerializer(typeof(MutualFundActivity))).Deserialize(new StringReader(recordXml));
				if (!_firstRecordHasBeenProcessed) ProcessFirstRecord();
				detail = (Detail)_mutualFundActivity.Activity.BlockingRecord;

				StockAndCashMovement movement = new StockAndCashMovement();

				if (detail.TransactionType == "50") return;  // These are balance records, not activity to post

				string key = "029-" + detail.TransactionType;
				movement.TransactionType = FileService.GetTypeMapping(key, "TRT");
				movement.AccountNumber = FileService.GetTypeMapping(key, "ACC");
				if (movement.TransactionType == null || movement.AccountNumber == null)
				{
					(new LoggingHelper()).Log(string.Format("Transaction Type {0} not found in TypeMappings table. Record skipped. {1}", key, recordXml), FileEventProcessors.InstanceId, 1, true);
					return;
				}

				movement.Amount = (!string.IsNullOrWhiteSpace(detail.DollarAmount) ?
					detail.DollarAmountFormatted :
					0.0M);

				movement.Cusip = detail.SecurityIssueIdNumber;

				movement.Quantity = (string.IsNullOrWhiteSpace(detail.ShareBalanceAmount) ? detail.ShareBalanceAmountExtendedFormatted : detail.ShareBalanceAmountFormatted);

				// Since this is the backside, reverse sign for credits(ind=2), not debits (see CSS.MutualFunds.Networking::ActivityRecordProcessor.cs line 229
				//   which shows quantities are reverse for debits for the front side & reversed again for backside posting -- StockServiceWrapper.cs line 178)
				// Scratch the above.  After testing it was decided to reverse the sign on the debits
				if (detail.DebitCreditIndicator == "1")
				{
					movement.Amount *= -1.0M;
					movement.Quantity *= -1.0M;
				}

				movement.PostingDate = movement.SettlementDate = detail.EffectiveDateFormatted;

				movement.Trailer = string.Format("029:{0}({1});{2}",
					detail.FirmFundidentificationNumber,
					detail.FirmFundIdentificationIndicator == "1" ? "FIN" : "BIN",
					detail.TransactionType);

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
			FileEventProcessors.FileTimestamp = _mutualFundActivity.DateTimeCreatedFormatted;
			int fileNumber = 0;
			int.TryParse(_mutualFundActivity.ApplicationMultiCycleCounter, out fileNumber);
			FileEventProcessors.FileNumber = fileNumber;
		}
	}
}
