using System;
using System.Collections.Generic;

namespace CSS.Connector.FileProcessors.Vanguard
{
    /// <summary>
    /// Represents a bank loan (from record type 05 of the Pledged Securities file)
    /// </summary>
    public class BankLoan
    {
		/// <summary>
		/// Business (from file timestamp)
		/// </summary>
		public DateTime BusinessDate;
		
		/// <summary>
		/// Bank Number from the file (2001 = BoNY; 2300 = OCC)
		/// </summary>
		public string BankNumber;

		/// <summary>
		/// Name of Bank
		/// </summary>
		public string BankName;

		/// <summary>
		/// Total Amount of Loan
		/// </summary>
		public Decimal LoanAmount;

		/// <summary>
		/// Total Collateral Value
		/// </summary>
		public Decimal CollateralValue;

		/// <summary>
		/// Total Market Value
		/// </summary>
		public Decimal MarketValue;

		/// <summary>
		/// Pledged securites within this bank loan
		/// </summary>
		public List<PledgedSecurity> PledgedSecurities;
	}
}
