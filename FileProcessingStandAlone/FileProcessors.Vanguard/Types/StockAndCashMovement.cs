using System;
using System.Collections.Generic;

namespace CSS.Connector.FileProcessors.Vanguard
{
    /// <summary>
    /// Represents stock and cash movement
    /// </summary>
	public class StockAndCashMovement : BaseMovement
	{
		/// <summary>The security cusip for this transaction. </summary>  
		public string Cusip;

		/// <summary>The security quantity for this transaction. </summary>  
		public decimal Quantity;
	}
}
