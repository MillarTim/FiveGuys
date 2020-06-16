using System;
using System.Collections.Generic;

namespace CSS.Connector.FileProcessors.Vanguard
{
    /// <summary>
    /// Base class for Cash and StockAndCash movement
    /// </summary>
    public class BaseMovement
    {
		/// <summary>The key (link) value for this transaction. </summary>        
		public decimal TransactionId;

		/// <summary>The four digit clearing number by which to look up the account</summary>
		public string ClearingNumber;

		/// <summary>The account number for this transaction. </summary>        
		public string AccountNumber;

		/// <summary>The bank account number for this transaction. </summary>        
		public string BankAccountNumber;

		/// <summary>The account category for this transaction. </summary>        
		public string AccountCategory;

		/// <summary>The account name for this transaction. </summary>        
		public string AccountName;

		/// <summary>The account type for this transaction. </summary>        
		public string AccountType;

		/// <summary>The business or posting date of this transaction. </summary>
		public DateTime? PostingDate;

		/// <summary>The settlement date for this transaction. </summary>        
		public DateTime? SettlementDate;

		/// <summary>The type of transaction, from def_trt. </summary>        
		public string TransactionType;

		/// <summary>The fund type for this transaction, from def_fnd.</summary>
		public string FundType;

		/// <summary>The monetary amount for this transaction.</summary>
		public decimal Amount;

		/// <summary>The trailer text for this transaction.</summary>
		public string Trailer;

		/// <summary>The source process that is initiating this transaction. </summary>        
		public string SubSource;

		/// <summary>The user that initiated this transaction.</summary>
		public string User;
	}
}
