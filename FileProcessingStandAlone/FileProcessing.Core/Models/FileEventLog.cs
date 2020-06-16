using System;
using System.Collections.Generic;
using System.Text;

namespace CSS.Connector.FileProcessing.Core.Models
{
    /// <summary>
    /// Event log information related to file processing.
    /// </summary>
    public class FileEventLog
    {
        /// <summary>
        /// Unique id of the log
        /// </summary>
        public int Id { get; set; }

		/// <summary>
		/// The GUID of the instance.  Uniquely identifies the file being processed.  May be null if the type of file cannot be determined.
		/// </summary>
		public string InstanceId { get; set; }

		/// <summary>
		/// Message to be logged.
		/// </summary>
		public string Message { get; set; }

        /// <summary>
        /// Local timestamp of the event time.
        /// </summary>
        public DateTime TimeStamp { get; set; }
    }
}
