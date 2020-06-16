/*====================================================================
NOTE:
This code was auto generated by the FixedLengthFile CSharpCode Generator
This file was created in the directory: 
     C:\TFS\Bronze\Enterprise\CSS\Sdk\Utilities\FixedLengthFile\FlatFileDefinitionFiles\Vanguard
====================================================================*/

using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace CSS.Connector.FileProcessors.Vanguard.UtcTradeCapture

{

public partial class UtcTradeCapture : PhysicalRecord
{
public string HeaderId { get; set; } // max length 6; required False
public string ApplicationDate { get; set; } // max length 8; required False
public string ProductNumber { get; set; } // max length 3; required False
public string ProductDescription { get; set; } // max length 20; required False
public string MultiCycleTransmissionCounter { get; set; } // max length 2; required False
public string PossDupeIndicator { get; set; } // max length 1; required False
public string ProductId { get; set; } // max length 8; required False
public string ApplicationMultiCycleCounter { get; set; } // max length 2; required False
public string RecipientId { get; set; } // max length 7; required False
public string Constant1 { get; set; } // max length 1; required False
public string RecordCount { get; set; } // max length 9; required False
public string SenderId { get; set; } // max length 4; required False
public string Constant { get; set; } // max length 3; required False
public string DateTimeCreated { get; set; } // max length 16; required False
private DateTime? _dateTimeCreatedFormatted;
public DateTime DateTimeCreatedFormatted
{
	get
	{
		if (_dateTimeCreatedFormatted == null)
		{
			_dateTimeCreatedFormatted = DateTime.Parse(Formatter.Format(DateTimeCreated, @"(\d{4})(\d{2})(\d{2}).{3}(.{5})~$1/$2/$3T$4:00~DateTime"));
		}
		return (DateTime)_dateTimeCreatedFormatted;
	}
}

public BlockingRecord BlockingRecord { get; set; }
}


public partial class Detail : BlockingRecord
{
public string TradeReportTransType { get; set; } // max length 1; required False
public string PriceType { get; set; } // max length 1; required False
public string SecurityIdSymbol { get; set; } // max length 15; required False
public string SecurityId { get; set; } // max length 12; required False
public string SecurityType { get; set; } // max length 2; required False
public string SettlementLocation { get; set; } // max length 2; required False
public string ShareQuantity { get; set; } // max length 11; required False
private long? _shareQuantityFormatted;
public long ShareQuantityFormatted
{
	get
	{
		if (_shareQuantityFormatted == null)
		{
			_shareQuantityFormatted = long.Parse(Formatter.Format(ShareQuantity, @"(.*)~$1~long"));
		}
		return (long)_shareQuantityFormatted;
	}
}

public string UnitPrice { get; set; } // max length 13; required False
private decimal? _unitPriceFormatted;
public decimal UnitPriceFormatted
{
	get
	{
		if (_unitPriceFormatted == null)
		{
			_unitPriceFormatted = decimal.Parse(Formatter.Format(UnitPrice, @"(.*)~$1~decimal"));
		}
		return (decimal)_unitPriceFormatted;
	}
}

public string Market { get; set; } // max length 3; required False
public string TradeDate { get; set; } // max length 8; required False
private DateTime? _tradeDateFormatted;
public DateTime TradeDateFormatted
{
	get
	{
		if (_tradeDateFormatted == null)
		{
			_tradeDateFormatted = DateTime.Parse(Formatter.Format(TradeDate, @"(\d{4})(\d{2})(\d{2})~$1/$2/$3~DateTime"));
		}
		return (DateTime)_tradeDateFormatted;
	}
}

public string TransactionTime { get; set; } // max length 9; required False
public string SettlementType { get; set; } // max length 1; required False
public string SettlementDate { get; set; } // max length 8; required False
private DateTime? _settlementDateFormatted;
public DateTime SettlementDateFormatted
{
	get
	{
		if (_settlementDateFormatted == null)
		{
			_settlementDateFormatted = DateTime.Parse(Formatter.Format(SettlementDate, @"(\d{4})(\d{2})(\d{2})~$1/$2/$3~DateTime"));
		}
		return (DateTime)_settlementDateFormatted;
	}
}

public string SideIndicator { get; set; } // max length 1; required False
public string OrderId { get; set; } // max length 32; required False
public string MarketControlNumber { get; set; } // max length 32; required False
public string ClientOrderId { get; set; } // max length 32; required False
public string OriginalExecutionId { get; set; } // max length 32; required False
public string ExecutionId { get; set; } // max length 32; required False
public string ClearingBroker { get; set; } // max length 8; required False
public string EnteringExecutingBroker { get; set; } // max length 8; required False
public string IntroducingBroker { get; set; } // max length 8; required False
public string BadgeId { get; set; } // max length 8; required False
public string ContraClearingBroker { get; set; } // max length 8; required False
public string ContraEnteringExecutingBroker { get; set; } // max length 8; required False
public string ContraIntroducingBroker { get; set; } // max length 8; required False
public string ContraBadgeId { get; set; } // max length 8; required False
public string SubmittedByParticipant { get; set; } // max length 1; required False
public string AccountNumber { get; set; } // max length 32; required False
public string AccountType { get; set; } // max length 1; required False
public string StepInStepOutIndicator { get; set; } // max length 1; required False
public string OddLotIndicator { get; set; } // max length 1; required False
public string ClearingInstruction { get; set; } // max length 1; required False
public string TradeInputSource { get; set; } // max length 2; required False
public string Currency { get; set; } // max length 3; required False
public string OrderCapacity { get; set; } // max length 1; required False
public string GrossTradeAmount { get; set; } // max length 17; required False
private decimal? _grossTradeAmountFormatted;
public decimal GrossTradeAmountFormatted
{
	get
	{
		if (_grossTradeAmountFormatted == null)
		{
			_grossTradeAmountFormatted = decimal.Parse(Formatter.Format(GrossTradeAmount, @"(.*)~$1~decimal"));
		}
		return (decimal)_grossTradeAmountFormatted;
	}
}

public string NetMoney { get; set; } // max length 17; required False
private decimal? _netMoneyFormatted;
public decimal NetMoneyFormatted
{
	get
	{
		if (_netMoneyFormatted == null)
		{
			_netMoneyFormatted = decimal.Parse(Formatter.Format(NetMoney, @"(.*)~$1~decimal"));
		}
		return (decimal)_netMoneyFormatted;
	}
}

public string DonTCountIndicator { get; set; } // max length 1; required False
public string CommissionExplicitFee { get; set; } // max length 9; required False
private decimal? _commissionExplicitFeeFormatted;
public decimal CommissionExplicitFeeFormatted
{
	get
	{
		if (_commissionExplicitFeeFormatted == null)
		{
			_commissionExplicitFeeFormatted = decimal.Parse(Formatter.Format(CommissionExplicitFee, @"(.*)~$1~decimal"));
		}
		return (decimal)_commissionExplicitFeeFormatted;
	}
}

public string OtherFees { get; set; } // max length 7; required False
private decimal? _otherFeesFormatted;
public decimal OtherFeesFormatted
{
	get
	{
		if (_otherFeesFormatted == null)
		{
			_otherFeesFormatted = decimal.Parse(Formatter.Format(OtherFees, @"(.*)~$1~decimal"));
		}
		return (decimal)_otherFeesFormatted;
	}
}

public string Taxes { get; set; } // max length 8; required False
private decimal? _taxesFormatted;
public decimal TaxesFormatted
{
	get
	{
		if (_taxesFormatted == null)
		{
			_taxesFormatted = decimal.Parse(Formatter.Format(Taxes, @"(.*)~$1~decimal"));
		}
		return (decimal)_taxesFormatted;
	}
}

public string AccruedInterestAmount { get; set; } // max length 10; required False
private decimal? _accruedInterestAmountFormatted;
public decimal AccruedInterestAmountFormatted
{
	get
	{
		if (_accruedInterestAmountFormatted == null)
		{
			_accruedInterestAmountFormatted = decimal.Parse(Formatter.Format(AccruedInterestAmount, @"(.*)~$1~decimal"));
		}
		return (decimal)_accruedInterestAmountFormatted;
	}
}

public string LiquidityIndicator { get; set; } // max length 1; required False
public string NsccRejectCodes { get; set; } // max length 2; required False
}


[System.Xml.Serialization.XmlInclude(typeof(Detail))]
public class BlockingRecord : PhysicalRecord
{
}

public class PhysicalRecord : object
{
[System.Xml.Serialization.XmlAttribute]
public int PhysicalFileRecordNumber { get; set; }
}

internal static class Formatter
{
	readonly static string codeCharsPositive = "{ABCDEFGHI";
	readonly static string codeCharsNegative = "}JKLMNOPQR";

	internal static string Format(string stringToFormat, string formattingString)
	{
		string outputString = stringToFormat;
		string[] formattingStrings = formattingString.Split('~');
		// formattingStrings should have 3 or 4 components. 3 for dates & 4 for decimalls
		int formattingStringCount = formattingStrings.Count();
		if (formattingStringCount != 3 && formattingStringCount != 4) throw new DataException(string.Format("Formatting string ({0}) is invalid.  It must be tilde delimited with three or four components.  See code for more details.", formattingString));

		if (!Regex.IsMatch(outputString, formattingStrings[0]))
			throw new DataException(string.Format("String to format ({0}) does not match expected format ({1}).", stringToFormat, formattingString));

		outputString = Regex.Replace(outputString, formattingStrings[0], formattingStrings[1]);

		// IBM binary coded decimal where last digit defines least significant digit and sign of the entire number
		if (formattingStrings[2] == "EBCDIC_BCD")
		{
			string rightMostDigit = outputString[outputString.Length - 1].ToString();
			outputString = outputString.Substring(0, outputString.Length - 1);
			int index = codeCharsNegative.IndexOf(rightMostDigit);
			if (index > -1)
				// from e-mail received from Vanguard on 2019-08-28, ignore sign on number and only use field (dr/cr) to determine sign
				outputString = /*"-" +*/ outputString + index.ToString();
			else
				outputString += codeCharsPositive.IndexOf(rightMostDigit).ToString();
			formattingStrings[2] = "Decimal"; // Once data has been fixed, it's a normal decimal
		}

		if (formattingStringCount == 4)
		{
			TypeCode typeCode = (TypeCode)Enum.Parse(typeof(TypeCode), formattingStrings[2], true);
			outputString = string.Format(formattingStrings[3], Convert.ChangeType(outputString, typeCode));
		}
		return outputString;
	}
}
}
