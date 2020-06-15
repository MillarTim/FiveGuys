using System;
using System.Collections.Generic;
using System.Text;

namespace CSS.Connector.FileProcessing.Core.Models
{
    /// <summary>
    /// Maps file IDs to accounts and transaction types, etc.
    /// </summary>
    public class TypeMapping
    {
        /// <summary>
        /// ID of the file (provided in the input files)
        /// </summary>
        public string FileId { get; set; }

        /// <summary>
        /// Key type, such as 'TRT' for transaction type or 'ACC' for account
        /// </summary>
        public string KeyType { get; set; }
  
		/// <summary>
        /// 'Input' or 'Output'
        /// </summary>
        public string Value { get; set; }
    }
}
