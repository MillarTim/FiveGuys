using System;
using System.Collections.Generic;
using System.Linq;

namespace CSS.Connector.FileProcessors.Vanguard.SettlementBalances
{
	internal class SettlementBalanceDictionary
	{
		private IDictionary<string, DictionaryEntry> Dictionary { get; set; }

		public SettlementBalanceDictionary()
		{
			if (Dictionary == null) Dictionary = new Dictionary<string, DictionaryEntry>();
		}


		/// <summary>
		/// Add an entry to the Settlement Balances dictionary.  Since the file duplicates old data from earlier files, process the entire file
		///   but when a newer timestamp comes along, update the amount & timestamp, so at the end of the file, the latest values only will be posted in EndOfFileEventProcessor
		///   Keys to the record are Clearing ID & Activity code (translated to TRT code)
		/// </summary>
		/// <param name="clearingId"></param>
		/// <param name="activityCode"></param>
		/// <param name="timeStamp"></param>
		/// <param name="amount"></param>
		/// <param name="isDebit"></param>
		public void Add(string clearingId, string activityCode, string activitySubCode, string balanceSource, DateTime timeStamp, decimal amount)
		{
			string key = clearingId + "~" + activityCode + "~" + activitySubCode;
			if (Dictionary.ContainsKey(key))
			{
				DictionaryEntry entry = Dictionary[key];
				if (entry.TimeStamp < timeStamp)
				{
					entry.TimeStamp     = timeStamp;
					entry.Amount        = amount;
					entry.BalanceSource = balanceSource;
				}
				// Do nothing if this row is from a timestamp older than the one already captured. (Unlikely since the file seems to be in ascending timestamp sequence.)
			}
			else
			{
				DictionaryEntry entry = new DictionaryEntry();
				entry.TimeStamp       = timeStamp;
				entry.Amount          = amount;
				entry.BalanceSource   = balanceSource;
				Dictionary.Add(key, entry);
			}
		}

		public List<SettlementBalanceEntry> GetBalances()
		{
			return
			Dictionary.Select(item => {
				SettlementBalanceEntry entry = new SettlementBalanceEntry();
				var keys = item.Key.Split('~');
				entry.ClearingId             = keys[0];
				entry.ActivityCode           = keys[1];
				entry.ActivitySubCode        = keys[2];
				entry.Amount                 = item.Value.Amount;
				entry.BalanceSource          = item.Value.BalanceSource;
				return entry;
				}).ToList();
		}

		public void Clear()
		{
			Dictionary.Clear();
		}
	}

	internal class DictionaryEntry
	{
		public DateTime TimeStamp { get; set; }
		public Decimal Amount { get; set; }
		public string BalanceSource { get; set; }
	}

	internal class SettlementBalanceEntry
	{
		public string ClearingId { get; set; }
		public string ActivityCode { get; set; }
		public string ActivitySubCode { get; set; }
		public string BalanceSource { get; set; }
		public decimal Amount { get; set; }
	}
}
