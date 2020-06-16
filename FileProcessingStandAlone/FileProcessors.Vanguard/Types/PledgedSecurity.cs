using System;

namespace CSS.Connector.FileProcessors.Vanguard
{
    /// <summary>
    /// Represents a pledged security within a bank loan (from record type 09 of the Pledged Securities file)
    /// </summary>
    public class PledgedSecurity
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
		/// Name of the security
		/// </summary>
		public string SecurityName;

		/// <summary>
		/// Market Value
		/// </summary>
		public Decimal MarketValue;

		/// <summary>
		/// Collateral Value
		/// </summary>
		public Decimal CollateralValue;

		/// <summary>
		/// Quantity
		/// </summary>
		public Decimal Quantity;

		/// <summary>
		/// Market price
		/// </summary>
		public Decimal Price;
	}
}
