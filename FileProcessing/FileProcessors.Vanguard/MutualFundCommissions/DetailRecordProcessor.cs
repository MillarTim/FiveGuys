using System;
using System.IO;
using System.Xml.Serialization;

using CSS.Connector.FileProcessing;
using CSS.Connector.FileProcessing.Core;

namespace CSS.Connector.FileProcessors.Vanguard.MutualFundCommissions
{
	public class DetailRecordProcessor : IRecordProcessor
	{
		private bool _firstRecordHasBeenProcessed = false;

		FileService _fileService;
		FileService FileService
		{
			get { if (_fileService == null) _fileService = new FileService(); return _fileService; }
		}

		MutualFundCommission _mutualFundCommission;

		public void ProcessRecord(string recordXml)
		{
			Detail detail = null;
			try
			{
				FileEventProcessors.TotalRecords++;
				_mutualFundCommission = (MutualFundCommission)(new XmlSerializer(typeof(MutualFundCommission))).Deserialize(new StringReader(recordXml));
				if (!_firstRecordHasBeenProcessed) ProcessFirstRecord();
				detail = (Detail)_mutualFundCommission.Commission.SecurityHeader.BlockingRecord;

				StockAndCashMovement movement = new StockAndCashMovement();

				string key = "202-ALL";
				movement.TransactionType = FileService.GetTypeMapping(key, "TRT");
				movement.AccountNumber = FileService.GetTypeMapping(key, "ACC");
				if (movement.TransactionType == null || movement.AccountNumber == null)
				{
					(new LoggingHelper()).Log(string.Format("Transaction Type {0} not found in TypeMappings table. Record skipped. {1}", key, recordXml), FileEventProcessors.InstanceId, 1, true);
					return;
				}

				movement.Cusip = _mutualFundCommission.Commission.SecurityHeader.SecurityIssueId;

				movement.Amount = (!string.IsNullOrWhiteSpace(detail.CommissionAmount) ?
					detail.CommissionAmountFormatted :
					0.0M);

				movement.Quantity = (string.IsNullOrWhiteSpace(detail.ShareQuantityOfTrade) ? 0.0M : detail.ShareQuantityOfTradeFormatted);

				// Since this is the backside, reverse sign for credits(ind=2), not debits (see CSSCmNSCC::Process202::UpdateWrkXps line 288)
				// Scratch the above.  After testing it was decided to reverse the sign on the debits
				if (_mutualFundCommission.Commission.SecurityHeader.DebitCreditIndicator == "1")
				{
					movement.Amount *= -1.0M;
					movement.Quantity *= -1.0M;
				}

				movement.PostingDate = movement.SettlementDate = _mutualFundCommission.Commission.SecurityHeader.FundFirmProcessingDateFormatted;

				movement.Trailer = string.Format("202:{0}({1});{2}",
					detail.CustomerAccount,
					detail.CustomerAccountIndicator == "1" ? "Dlr" : "Fnd",
					_mutualFundCommission.Commission.SecurityHeader.CommissionType
					);

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
			FileEventProcessors.FileTimestamp = _mutualFundCommission.DateTimeCreatedFormatted;
			int fileNumber = 0;
			int.TryParse(_mutualFundCommission.ApplicationMultiCycleCounter, out fileNumber);
			FileEventProcessors.FileNumber = fileNumber;
		}
	}
}
