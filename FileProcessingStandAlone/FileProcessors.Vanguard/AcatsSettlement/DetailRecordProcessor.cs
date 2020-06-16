using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Xml.Serialization;

using CSS.Connector.FileProcessing;
using CSS.Connector.FileProcessing.Core;
using System;

namespace CSS.Connector.FileProcessors.Vanguard.AcatsSettlement
{
	public class DetailRecordProcessor : IRecordProcessor
	{
		private bool _firstRecordHasBeenProcessed = false;
		private const string _fileType = "719";

		private string _nextDayTrtCode;
		private string NextDayTrtCode
		{
			get
			{
				if (_nextDayTrtCode == null)
				{
					_nextDayTrtCode = FileService.GetTypeMapping(_fileType + "-NEXTDAY", "TRT");
					if (_nextDayTrtCode == null) throw new System.Data.DataException(string.Format("Transaction Type for {0} not found in TypeMappings table.", _fileType + "-NEXTDAY"));
				}
				return _nextDayTrtCode;
			}
		}

		private string _nextDayAccount;
		private string NextDayAccount
		{
			get
			{
				if (_nextDayAccount == null)
				{
					_nextDayAccount = FileService.GetTypeMapping(_fileType + "-NEXTDAY", "ACC");
					if (_nextDayAccount == null) throw new System.Data.DataException(string.Format("Account for {0} not found in TypeMappings table.", _fileType + "-FUTURE"));
				}
				return _nextDayAccount;
			}
		}

		private string _futureTrtCode;
		private string FutureTrtCode
		{
			get
			{
				if (_futureTrtCode == null)
				{
					_futureTrtCode = FileService.GetTypeMapping(_fileType + "-FUTURE", "TRT");
					if (_futureTrtCode == null) throw new System.Data.DataException(string.Format("Transaction Type for {0} not found in TypeMappings table.", _fileType + "-NEXTDAY"));
				}
				return _futureTrtCode;
			}
		}

		private string _futureAccount;
		private string FutureAccount
		{
			get
			{
				if (_futureAccount == null)
				{
					_futureAccount = FileService.GetTypeMapping(_fileType + "-FUTURE", "ACC");
					if (_futureAccount == null) throw new System.Data.DataException(string.Format("Account for {0} not found in TypeMappings table.", _fileType + "-FUTURE"));
				}
				return _futureAccount;
			}
		}

		private DateTime NextSettlementDate { get; set; }

		FileService _fileService;
		FileService FileService
		{
			get { if (_fileService == null) _fileService = new FileService(); return _fileService; }
		}

		AcatsSettlement _acatsSettlement;

		public void ProcessRecord(string recordXml)
		{
			AcatsSettlementTransferRecord transfer = null;
			try
			{
				FileEventProcessors.TotalRecords++; // Total records and error records are kept track of at the Transfer record level (not the asset level records) since that's the blocking recrd.
				_acatsSettlement = (AcatsSettlement)(new XmlSerializer(typeof(AcatsSettlement))).Deserialize(new StringReader(recordXml));
				if (!_firstRecordHasBeenProcessed) ProcessFirstRecord();
				transfer = (AcatsSettlementTransferRecord)_acatsSettlement.Settlement.BlockingRecord;

				string ClrId = _acatsSettlement.RecipientId; // Clearing ID for this firm

				// Ignore Value/Free Indicators of "F" (free)
				var assets =
				transfer.ForeignAssets   .Where(a => a.ValueFreeIndicator != "F").Select(a => new { Typ = "For", Amt = (string.IsNullOrWhiteSpace(a.AssetAmount) ? 0.0M : a.AssetAmountFormatted), Qty = (string.IsNullOrWhiteSpace(a.AssetQuantity) ? 0.0M : a.AssetQuantityFormatted), a.AcatsControlNumber, a.TransferType, ProductKey = a.IsinSecurityIssueId, ShouldReverseSign = ShouldReverseSign(ClrId, a.OriginalDelivererNumber, a.PositionCode, a.SettlingLocation, a.DebitCreditIndicator), a.SettlingLocation, a.PhysicalFileRecordNumber }).Concat(
				transfer.InsuranceAssets .Where(a => a.ValueFreeIndicator != "F").Select(a => new { Typ = "Ins", Amt = (string.IsNullOrWhiteSpace(a.AssetAmount) ? 0.0M : a.AssetAmountFormatted), Qty = (string.IsNullOrWhiteSpace(a.AssetQuantity) ? 0.0M : a.AssetQuantityFormatted), a.AcatsControlNumber, a.TransferType, ProductKey = a.IsinSecurityIssueId, ShouldReverseSign = ShouldReverseSign(ClrId, a.OriginalDelivererNumber, a.PositionCode, a.SettlingLocation, a.DebitCreditIndicator), a.SettlingLocation, a.PhysicalFileRecordNumber }).Concat(
				transfer.MutualFundAssets.Where(a => a.ValueFreeIndicator != "F").Select(a => new { Typ = "Mut", Amt = (string.IsNullOrWhiteSpace(a.AssetAmount) ? 0.0M : a.AssetAmountFormatted), Qty = (string.IsNullOrWhiteSpace(a.AssetQuantity) ? 0.0M : a.AssetQuantityFormatted), a.AcatsControlNumber, a.TransferType, ProductKey = a.IsinSecurityIssueId, ShouldReverseSign = ShouldReverseSign(ClrId, a.OriginalDelivererNumber, a.PositionCode, a.SettlingLocation, a.DebitCreditIndicator), a.SettlingLocation, a.PhysicalFileRecordNumber }).Concat(
				transfer.OptionAssets    .Where(a => a.ValueFreeIndicator != "F").Select(a => new { Typ = "Opt", Amt = (string.IsNullOrWhiteSpace(a.AssetAmount) ? 0.0M : a.AssetAmountFormatted), Qty = (string.IsNullOrWhiteSpace(a.AssetQuantity) ? 0.0M : a.AssetQuantityFormatted), a.AcatsControlNumber, a.TransferType, ProductKey = GetOptionSymbol(a)   , ShouldReverseSign = ShouldReverseSign(ClrId, a.OriginalDelivererNumber, a.PositionCode, a.SettlingLocation, a.DebitCreditIndicator), a.SettlingLocation, a.PhysicalFileRecordNumber }).Concat(
				transfer.OtherAssets     .Where(a => a.ValueFreeIndicator != "F").Select(a => new { Typ = "Oth", Amt = (string.IsNullOrWhiteSpace(a.AssetAmount) ? 0.0M : a.AssetAmountFormatted), Qty = (string.IsNullOrWhiteSpace(a.AssetQuantity) ? 0.0M : a.AssetQuantityFormatted), a.AcatsControlNumber, a.TransferType, ProductKey = a.IsinSecurityIssueId, ShouldReverseSign = ShouldReverseSign(ClrId, a.OriginalDelivererNumber, a.PositionCode, a.SettlingLocation, a.DebitCreditIndicator), a.SettlingLocation, a.PhysicalFileRecordNumber }))))).ToList();

				bool didErrorOccurOnAssetRecords = false;
				assets.ForEach(asset =>
				{
					try
					{
						if (asset.SettlingLocation != "40") return; // Ignore all but cash rows for now
						StockAndCashMovement movement = new StockAndCashMovement();
						if (NextSettlementDate == transfer.SettlementDateFormatted)
						{
							movement.TransactionType = NextDayTrtCode;
							movement.AccountNumber = NextDayAccount;
						}
						else
						{
							movement.TransactionType = FutureTrtCode;
							movement.AccountNumber = FutureAccount;
						}

						// Even though the field name is CUSIP, it can store a different product identifyer, like Option Symbol
						movement.Cusip = asset.ProductKey;

						int multiplier = (asset.ShouldReverseSign ? -1 : 1);

						if (asset.SettlingLocation == "10" || asset.SettlingLocation == "40")
							movement.Amount = asset.Amt * multiplier;   // Only want amounts from cash & fundserv rows
						else
							movement.Quantity = asset.Qty * multiplier; // Quantities only from other rows
																		// movement.PostingDate = transfer.ProcessingDateFormatted; -no longer use processing date
																		// movement.PostingDate = movement.SettlementDate = transfer.SettlementDateFormatted; // logic prior to 2/13/2020
						movement.SettlementDate = NextSettlementDate;
						movement.Trailer = _fileType + ":" + asset.Typ + ";" + asset.AcatsControlNumber + ";" + asset.TransferType;
						new MovementServiceHelper().PostForecastStockAndCash(movement, string.IsNullOrWhiteSpace(movement.Cusip));
					}
					catch (Exception e)
					{
						didErrorOccurOnAssetRecords = true;
						(new LoggingHelper()).Log("ProcessRecord",
							string.Format("Error processing file {0}, record {1} (asset record), {2}",
							FileEventProcessors.FileName,
							asset?.PhysicalFileRecordNumber,
							e.ToString()
							), FileEventProcessors.InstanceId, 1, true);
					}
				});
				if (didErrorOccurOnAssetRecords) FileEventProcessors.ErrorRecords++;
			}
			catch (Exception e)
			{
				FileEventProcessors.ErrorRecords++;
				(new LoggingHelper()).Log("ProcessRecord",
					string.Format("Error processing file {0}, record {1} (transfer record), {2}",
					FileEventProcessors.FileName,
					transfer?.PhysicalFileRecordNumber,
					e.ToString()
					), FileEventProcessors.InstanceId, 1, true);
			}
		}

		private bool ShouldReverseSign(string clrId, string originalDelivererNumber, string positionCode, string settlingLocation, string debitCreditIndicator)
		{
			// Outgoing Transfer is where this firm is the Original Deliverer (ClrId == a.OriginalDelivererNumber)
			// For front side entry, reverse the sign when:
			//   Outgoing Transfer with a Long (L) Position Code *or* an Incoming Transfer with a Short (S) Position Code
			// But this is for a backside entry, so it's the opposite  <== this has been changed 3/12/2020

			// optionCategory is '0040' for cash balance info
			// debitCreditIndicator is 'C' or ' ' for credits and 'D' for debits for cash balances, but
			// positionCode         is 'L'        for credits and 'S' for debits (long/short) for products
			bool isOutgoing = clrId == originalDelivererNumber;
			bool isCredit = (settlingLocation == "40" ?
			                                            (debitCreditIndicator == "C" || string.IsNullOrWhiteSpace(debitCreditIndicator) ? true : false) :
			                                            (positionCode         == "L" || string.IsNullOrWhiteSpace(positionCode        ) ? true : false)); // Long for product is like Credit for cash

			return ((isOutgoing && isCredit || !isOutgoing && !isCredit));
		}

		private string GetOptionSymbol(OptionAsset a)
		{
			return
				a.OptionSymbol + " " +
				(string.IsNullOrWhiteSpace(a.OptionExpirationDate) || a.OptionExpirationDate.Length < 3 ? "" : a.OptionExpirationDate.Substring(2)) +
				a.OptionPCIndicator +
				FormatStrikePrice(a);
		}

		private string FormatStrikePrice(OptionAsset a)
		{
			if (string.IsNullOrWhiteSpace(a.OptionStrikePriceInteger) || string.IsNullOrWhiteSpace(a.OptionStrikePriceDecimal) ||
				a.OptionStrikePriceDecimal.Length < 2 || (a.OptionStrikePriceInteger + a.OptionStrikePriceDecimal).Substring(0, 7) == "0000000")
				return "";
			return a.OptionStrikePriceInteger.TrimStart('0') + "." + a.OptionStrikePriceDecimal.Substring(0, 2);
		}

		private void ProcessFirstRecord()
		{
			_firstRecordHasBeenProcessed = true;
			FileEventProcessors.DetailRecordFound = true;
			FileEventProcessors.FileTimestamp = _acatsSettlement.DateTimeCreatedFormatted;
			int fileNumber = 0;
			int.TryParse(_acatsSettlement.ApplicationMultiCycleCounter, out fileNumber);
			FileEventProcessors.FileNumber = fileNumber;
			_nextDayAccount = null;
			_nextDayTrtCode = null;
			_futureAccount = null;
			_futureTrtCode = null;
			NextSettlementDate = new MovementServiceHelper().GetNextSettlementDate(DateTime.Now);
		}
	}
}
