using System;
using System.Collections.Generic;
using System.Linq;

/*
using EF = CSS.Cloud.Movements.Core.Models;  // Objects defined in this namespace are for Entity Framework and database usage
using SF = CSS.Cloud.Movements.Model;        // Objects defined in this namespace are for exposure to Service Fabric
using System;
*/
namespace CSS.Connector.FileProcessors.Vanguard.SecurityReport
{
	internal static class ConversionHelper
	{
		/*
		public static List<SF.BankLoan> ConvertToServiceFabricBankLoans(List<EF.BankLoan> bankLoans)
		{
			List<SF.BankLoan> outBankLoans = new List<SF.BankLoan>();
			bankLoans.ForEach(bankLoan => outBankLoans.Add(ConvertToServiceFabricBankLoan(bankLoan)));
			return outBankLoans;
		}
		*/

		public static BankLoan ConvertToServiceFabricBankLoan(SecurityReport securityReport)
		{
			Bank bankLoan = (Bank)securityReport.BlockingRecord;

			BankLoan outBankLoan          = new BankLoan();
			outBankLoan.BusinessDate      = securityReport.DateTimeStampFormatted.Date;
			outBankLoan.BankNumber        = bankLoan.BankNumber;
			outBankLoan.BankName          = bankLoan.BankName;
			outBankLoan.LoanAmount        = bankLoan.LoanAmountFormatted;
			outBankLoan.CollateralValue   = bankLoan.CollateralValueFormatted;
			outBankLoan.MarketValue       = bankLoan.MarketValueFormatted;
			// TODO:  Maybe convert Collection to List !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			List<Security> securities = new List<Security>(bankLoan.Securities);
			outBankLoan.PledgedSecurities = ConvertToServiceFabricPledgedSecurities(securities, outBankLoan.BusinessDate);
			return outBankLoan;
		}

		// TODO:  Maybe convert Collection to List
		private static List<PledgedSecurity> ConvertToServiceFabricPledgedSecurities(List<Security> pledgedSecurities, DateTime businessDate)
		{
			List<PledgedSecurity> outPledgedSecurities = new List<PledgedSecurity>();
			pledgedSecurities.GroupBy(g => new { g.BankNumber, g.SecurityName }).ForEach(
				pledgedSecurity => 
				{
					PledgedSecurity outPledgedSecurity = new PledgedSecurity();
					outPledgedSecurity.BusinessDate       = businessDate;
					outPledgedSecurity.BankNumber         = pledgedSecurity.Key.BankNumber;
					outPledgedSecurity.SecurityName       = pledgedSecurity.Key.SecurityName;
					outPledgedSecurity.MarketValue        = pledgedSecurity.Sum(x => x.MarketValueFormatted);
					outPledgedSecurity.CollateralValue    = pledgedSecurity.Sum(x => x.CollateralValueFormatted);
					outPledgedSecurity.Quantity           = pledgedSecurity.Sum(x => x.QuantityFormatted);
					outPledgedSecurity.Price              = pledgedSecurity.Min(x => x.MarketPriceFormatted); // Use Min since this should be the same on each row for a given security
					// It's rare to have more than one row per security, but it does happen
					outPledgedSecurities.Add(outPledgedSecurity);
				});
			return outPledgedSecurities;
		}
	}
}
