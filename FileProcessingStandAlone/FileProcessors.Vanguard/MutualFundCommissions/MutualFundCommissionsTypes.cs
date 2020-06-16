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

namespace CSS.Connector.FileProcessors.Vanguard.MutualFundCommissions

{

public class MutualFundCommission : PhysicalRecord
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

public Commission Commission { get; set; }
}


public class Commission : PhysicalRecord
{
public string SystemCode { get; set; } // max length 1; required False
public string RecordType { get; set; } // max length 2; required False
public string SequenceNumber { get; set; } // max length 1; required False
public string MroRecipientNumber { get; set; } // max length 4; required False
public string Literal1 { get; set; } // max length 11; required False
public string TransmissionDate { get; set; } // max length 8; required False
private DateTime? _transmissionDateFormatted;
public DateTime TransmissionDateFormatted
{
	get
	{
		if (_transmissionDateFormatted == null)
		{
			_transmissionDateFormatted = DateTime.Parse(Formatter.Format(TransmissionDate, @"(\d{2})(\d{2})(\d{4})~$3/$1/$2~DateTime"));
		}
		return (DateTime)_transmissionDateFormatted;
	}
}

public string AutorouteFileId { get; set; } // max length 8; required False
public string Literal2 { get; set; } // max length 10; required False
public SecurityHeader SecurityHeader { get; set; }
public BlockingRecord BlockingRecord { get; set; }
}


public class SecurityHeader : PhysicalRecord
{
public string SystemCode { get; set; } // max length 1; required True
public string RecordType { get; set; } // max length 2; required True
public string ReceivingFirmNumber { get; set; } // max length 4; required True
public string AgentForReceivingFirm { get; set; } // max length 4; required True
public string DeliveringFirmFundNumber { get; set; } // max length 4; required True
public string SecurityIssueCountryCode { get; set; } // max length 2; required True
public string SecurityIssueId { get; set; } // max length 9; required True
public string SecurityIssueCheckDigit { get; set; } // max length 1; required True
public string FundFirmProcessingDate { get; set; } // max length 8; required True
private DateTime? _fundFirmProcessingDateFormatted;
public DateTime FundFirmProcessingDateFormatted
{
	get
	{
		if (_fundFirmProcessingDateFormatted == null)
		{
			_fundFirmProcessingDateFormatted = DateTime.Parse(Formatter.Format(FundFirmProcessingDate, @"(\d{2})(\d{2})(\d{4})~$3/$1/$2~DateTime"));
		}
		return (DateTime)_fundFirmProcessingDateFormatted;
	}
}

public string CommissionType { get; set; } // max length 2; required True
public string DebitCreditIndicator { get; set; } // max length 1; required True
public string DebitReasonCode { get; set; } // max length 1; required True
public string SettlementIndicator { get; set; } // max length 1; required True
public string RecordDate { get; set; } // max length 8; required True
private DateTime? _recordDateFormatted;
public DateTime RecordDateFormatted
{
	get
	{
		if (_recordDateFormatted == null)
		{
			_recordDateFormatted = DateTime.Parse(Formatter.Format(RecordDate, @"(\d{2})(\d{2})(\d{4})~$3/$1/$2~DateTime"));
		}
		return (DateTime)_recordDateFormatted;
	}
}

public string SecurityIssueIdIndecator { get; set; } // max length 1; required True
public string RejectCode1 { get; set; } // max length 2; required True
public string RejectCode2 { get; set; } // max length 2; required True
public string RejectCode3 { get; set; } // max length 2; required True
public string RejectCode4 { get; set; } // max length 2; required True
public string RejectCode5 { get; set; } // max length 2; required True
public string RejectCode6 { get; set; } // max length 2; required True
public string RejectCode7 { get; set; } // max length 2; required True
public BlockingRecord BlockingRecord { get; set; }
}


public class GrandTotalTrailer : BlockingRecord
{
public string SystemCode { get; set; } // max length 1; required False
public string RecordType { get; set; } // max length 2; required False
public string ReceivingFirmFundNumber { get; set; } // max length 4; required False
public string DeliveringFirmFundNumber { get; set; } // max length 4; required False
public string TotalRecords { get; set; } // max length 9; required False
private int? _totalRecordsFormatted;
public int TotalRecordsFormatted
{
	get
	{
		if (_totalRecordsFormatted == null)
		{
			_totalRecordsFormatted = int.Parse(Formatter.Format(TotalRecords, @"(\d{9})~$1~int"));
		}
		return (int)_totalRecordsFormatted;
	}
}

public string TotalDebitDollarAmount { get; set; } // max length 16; required False
private Decimal? _totalDebitDollarAmountFormatted;
public Decimal TotalDebitDollarAmountFormatted
{
	get
	{
		if (_totalDebitDollarAmountFormatted == null)
		{
			_totalDebitDollarAmountFormatted = Decimal.Parse(Formatter.Format(TotalDebitDollarAmount, @"(\d{14})(\d{2})~$1.$2~Decimal"));
		}
		return (Decimal)_totalDebitDollarAmountFormatted;
	}
}

public string TotalCreditDollarAmount { get; set; } // max length 16; required False
private Decimal? _totalCreditDollarAmountFormatted;
public Decimal TotalCreditDollarAmountFormatted
{
	get
	{
		if (_totalCreditDollarAmountFormatted == null)
		{
			_totalCreditDollarAmountFormatted = Decimal.Parse(Formatter.Format(TotalCreditDollarAmount, @"(\d{14})(\d{2})~$1.$2~Decimal"));
		}
		return (Decimal)_totalCreditDollarAmountFormatted;
	}
}

public string RejectCode1 { get; set; } // max length 2; required False
public string RejectCode2 { get; set; } // max length 2; required False
public string RejectCode3 { get; set; } // max length 2; required False
public string RejectCode4 { get; set; } // max length 2; required False
public string RejectCode5 { get; set; } // max length 2; required False
public string RejectCode6 { get; set; } // max length 2; required False
public string RejectCode7 { get; set; } // max length 2; required False
}


public class Detail : BlockingRecord
{
public string SystemCode1 { get; set; } // max length 1; required False
public string RecordType1 { get; set; } // max length 2; required False
public string SequenceNumber1 { get; set; } // max length 1; required False
public string NetworkingControlIndicator { get; set; } // max length 1; required False
public string CommissionAmount { get; set; } // max length 16; required False
private Decimal? _commissionAmountFormatted;
public Decimal CommissionAmountFormatted
{
	get
	{
		if (_commissionAmountFormatted == null)
		{
			_commissionAmountFormatted = Decimal.Parse(Formatter.Format(CommissionAmount, @"(\d{14})(\d{2})~$1.$2~Decimal"));
		}
		return (Decimal)_commissionAmountFormatted;
	}
}

public string CustomerAccountIndicator { get; set; } // max length 1; required False
public string CustomerAccount { get; set; } // max length 20; required False
public string SsnEinNumberIndicator { get; set; } // max length 1; required False
public string SsnEinNumber { get; set; } // max length 9; required False
public string BranchIdNumber { get; set; } // max length 9; required False
public string AccountRepresentativeNumber { get; set; } // max length 9; required False
public string WrapAccountIndicator { get; set; } // max length 1; required False
public string NameIndicator { get; set; } // max length 1; required False
public string Name { get; set; } // max length 15; required False
public string SocialCode { get; set; } // max length 2; required False
public string SystemCode2 { get; set; } // max length 1; required False
public string RecordType2 { get; set; } // max length 2; required False
public string SequenceNumber2 { get; set; } // max length 1; required False
public string NameIndicator2 { get; set; } // max length 1; required False
public string Name2 { get; set; } // max length 15; required False
public string CommissionRate { get; set; } // max length 5; required False
private Decimal? _commissionRateFormatted;
public Decimal CommissionRateFormatted
{
	get
	{
		if (_commissionRateFormatted == null)
		{
			_commissionRateFormatted = Decimal.Parse(Formatter.Format(CommissionRate, @"(\d{1})(\d{4})~$1.$2~Decimal"));
		}
		return (Decimal)_commissionRateFormatted;
	}
}

public string GrossAmountOfTradeTotalSecurityAssets { get; set; } // max length 16; required False
private Decimal? _grossAmountOfTradeTotalSecurityAssetsFormatted;
public Decimal GrossAmountOfTradeTotalSecurityAssetsFormatted
{
	get
	{
		if (_grossAmountOfTradeTotalSecurityAssetsFormatted == null)
		{
			_grossAmountOfTradeTotalSecurityAssetsFormatted = Decimal.Parse(Formatter.Format(GrossAmountOfTradeTotalSecurityAssets, @"(\d{14})(\d{2})~$1.$2~Decimal"));
		}
		return (Decimal)_grossAmountOfTradeTotalSecurityAssetsFormatted;
	}
}

public string ShareQuantityOfTrade { get; set; } // max length 13; required False
private Decimal? _shareQuantityOfTradeFormatted;
public Decimal ShareQuantityOfTradeFormatted
{
	get
	{
		if (_shareQuantityOfTradeFormatted == null)
		{
			_shareQuantityOfTradeFormatted = Decimal.Parse(Formatter.Format(ShareQuantityOfTrade, @"(\d{9})(\d{4})~$1.$2~Decimal"));
		}
		return (Decimal)_shareQuantityOfTradeFormatted;
	}
}

public string PricePerShare { get; set; } // max length 12; required False
private Decimal? _pricePerShareFormatted;
public Decimal PricePerShareFormatted
{
	get
	{
		if (_pricePerShareFormatted == null)
		{
			_pricePerShareFormatted = Decimal.Parse(Formatter.Format(PricePerShare, @"(\d{6})(\d{6})~$1.$2~Decimal"));
		}
		return (Decimal)_pricePerShareFormatted;
	}
}

public string CurrencyIndicator { get; set; } // max length 3; required False
public string TradeDate { get; set; } // max length 8; required False
private DateTime? _tradeDateFormatted;
public DateTime TradeDateFormatted
{
	get
	{
		if (_tradeDateFormatted == null)
		{
			_tradeDateFormatted = DateTime.Parse(Formatter.Format(TradeDate, @"(\d{2})(\d{2})(\d{4})~$3/$1/$2~DateTime"));
		}
		return (DateTime)_tradeDateFormatted;
	}
}

public string TransactionType { get; set; } // max length 1; required False
public string AssetseligibleForTrails { get; set; } // max length 16; required False
private Decimal? _assetseligibleForTrailsFormatted;
public Decimal AssetseligibleForTrailsFormatted
{
	get
	{
		if (_assetseligibleForTrailsFormatted == null)
		{
			_assetseligibleForTrailsFormatted = Decimal.Parse(Formatter.Format(AssetseligibleForTrails, @"(\d{14})(\d{2})~$1.$2~Decimal"));
		}
		return (Decimal)_assetseligibleForTrailsFormatted;
	}
}

public string SystemCode3 { get; set; } // max length 1; required True
public string RecordType3 { get; set; } // max length 2; required True
public string SequenceNumber3 { get; set; } // max length 1; required True
public string PlanAccountNumber { get; set; } // max length 20; required True
public string PlanAccountNumberIndicator { get; set; } // max length 1; required True
public string CustomerAccountIndicator2 { get; set; } // max length 1; required True
public string CustomerAccount2 { get; set; } // max length 20; required True
}


public class SecurityTrailer : BlockingRecord
{
public string SystemCode { get; set; } // max length 1; required False
public string RecordType { get; set; } // max length 2; required False
public string DeliveringFirmFundNumber { get; set; } // max length 4; required False
public string SecurityIssueCountryCode { get; set; } // max length 2; required False
public string SecurityIssueId { get; set; } // max length 9; required False
public string SecurityIssueCheckDigit { get; set; } // max length 1; required False
public string TotalRecords { get; set; } // max length 8; required False
private int? _totalRecordsFormatted;
public int TotalRecordsFormatted
{
	get
	{
		if (_totalRecordsFormatted == null)
		{
			_totalRecordsFormatted = int.Parse(Formatter.Format(TotalRecords, @"(\d{8})~$1~int"));
		}
		return (int)_totalRecordsFormatted;
	}
}

public string DebitCreditIndicator { get; set; } // max length 1; required False
public string DollarAmount { get; set; } // max length 16; required False
private Decimal? _dollarAmountFormatted;
public Decimal DollarAmountFormatted
{
	get
	{
		if (_dollarAmountFormatted == null)
		{
			_dollarAmountFormatted = Decimal.Parse(Formatter.Format(DollarAmount, @"(\d{14})(\d{2})~$1.$2~Decimal"));
		}
		return (Decimal)_dollarAmountFormatted;
	}
}

public string RejectCode1 { get; set; } // max length 2; required False
public string RejectCode2 { get; set; } // max length 2; required False
public string RejectCode3 { get; set; } // max length 2; required False
public string RejectCode4 { get; set; } // max length 2; required False
public string RejectCode5 { get; set; } // max length 2; required False
public string RejectCode6 { get; set; } // max length 2; required False
public string RejectCode7 { get; set; } // max length 2; required False
}


[System.Xml.Serialization.XmlInclude(typeof(GrandTotalTrailer))]
[System.Xml.Serialization.XmlInclude(typeof(Detail))]
[System.Xml.Serialization.XmlInclude(typeof(SecurityTrailer))]
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
			{
				outputString = "-" + outputString + index.ToString();
			}
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
