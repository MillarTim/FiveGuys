using System;
using System.Collections.Generic;

namespace CSS.Connector.FileProcessors.Vanguard
{
    /// <summary>
    /// Represents a bank loan (from record type 05 of the Pledged Securities file)
    /// </summary>
    public class ExpectedActivity
    {
		/// <summary>
		/// Account
		/// </summary>
		public string AccountNumber;
		
		/// <summary>
		/// Amount
		/// </summary>
		public Decimal Amount;

		/// <summary>
		/// Trailer text
		/// </summary>
		public string Trailer;

		/// <summary>
		/// Transaction type code from appropriate row in TypeMappings
		/// </summary>
		public string  TransactionType;

		/// <summary>
		/// Certain transactions like the State tax need to be posted on the next settlement date
		/// </summary>
		public DateTime? SettlementDate;
	}
}
