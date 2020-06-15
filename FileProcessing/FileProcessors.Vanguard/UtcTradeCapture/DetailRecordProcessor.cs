using System;
using System.IO;
using System.Xml.Serialization;

using CSS.Connector.FileProcessing;
using CSS.Connector.FileProcessing.Core;

namespace CSS.Connector.FileProcessors.Vanguard.UtcTradeCapture
{
	public class DetailRecordProcessor : IRecordProcessor
	{
		private bool _firstRecordHasBeenProcessed = false;

		FileService _fileService;
		FileService FileService
		{
			get { if (_fileService == null) _fileService = new FileService(); return _fileService; }
		}

		UtcTradeCapture _utcTradeCapture;

		public void ProcessRecord(string recordXml)
		{
			Detail detail = null;
			try
			{
				FileEventProcessors.TotalRecords++;
				_utcTradeCapture = (UtcTradeCapture)(new XmlSerializer(typeof(UtcTradeCapture))).Deserialize(new StringReader(recordXml));
				if (!_firstRecordHasBeenProcessed) ProcessFirstRecord();
				detail = (Detail)_utcTradeCapture.BlockingRecord;

				string trtKey = "673-ALL";
				string transactionType = new FileService().GetTypeMapping(trtKey, "TRT");
				if (string.IsNullOrWhiteSpace(transactionType)) throw new Exception("TRT key for 673-ALL not found in TypeMappings table.");

				StockAndCashMovement movement = new StockAndCashMovement();

				movement.TransactionType = transactionType;
				movement.ClearingNumber = detail.ClearingBroker.Substring(4); // Clearing broker appears to be last four characters of the field
				if      (detail.SecurityId.Length > 10) movement.Cusip = detail.SecurityId.Substring(2, 9); // CUSIP appears to be preceded by 2 char country code & followed by a check digit
				else if (detail.SecurityId.Length < 10) movement.Cusip = detail.SecurityId;                 // If no country code or check digit, just assume the whole thing is CUSIP
				else                                    movement.Cusip = detail.SecurityId.Substring(0, 9); // Limit the size to 9 in case there's a check digit for some reason

				// Codes 1, 3, & B are for buys.  2, 4, 5, 6 for sells.  Reverse sign on sells
				int multiplier = ("2456".Contains(detail.SideIndicator) ? -1 : 1);

				movement.Amount = detail.GrossTradeAmountFormatted * multiplier;
				movement.Quantity = detail.ShareQuantityFormatted * multiplier;
				movement.PostingDate = DateTime.Parse(_utcTradeCapture.DateTimeCreatedFormatted.ToString("yyyy-MM-dd"));
				// 99991231 for when and if issued.  Use current date
				movement.SettlementDate = (detail.SettlementDate == "99991231" ? DateTime.Now.Date : detail.SettlementDateFormatted);
				movement.Trailer = string.Format("673 file, seq {0} rec {1}", _utcTradeCapture.ApplicationMultiCycleCounter, detail.PhysicalFileRecordNumber);

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
			FileEventProcessors.FileTimestamp = _utcTradeCapture.DateTimeCreatedFormatted;
			int fileNumber = 0;
			int.TryParse(_utcTradeCapture.ApplicationMultiCycleCounter, out fileNumber);
			FileEventProcessors.FileNumber = fileNumber;
		}
	}
}