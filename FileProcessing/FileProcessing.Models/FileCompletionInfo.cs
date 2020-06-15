using System;

namespace CSS.Connector.FileProcessing
{
    /// <summary>
    /// This class contains the details of an inbound file process including total records,
    /// the number of records processed, and the number of records that failed.
    /// </summary>
    public class FileCompletionInfo
    {
        /// <summary>
        /// Gets and sets the total number of records in the the inbound file
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// Gets and sets the number of records processed successfully from an inbound file
        /// </summary>
        public int RecordsProcessed { get; set; }

        /// <summary>
        /// Gets and sets the number of records that received errors during processing
        /// </summary>
        public int RecordsFailed { get; set; }

		/// <summary>
		/// Gets and sets the number of records that received errors during processing
		/// </summary>
		public bool FatalErrorOccurred { get; set; }

		/// <summary>
		/// Gets and sets the number of records that received errors during processing
		/// </summary>
		public string FatalErrorMessage { get; set; }

		/// <summary>
		/// Gets and sets the file timestamp which is in the 01 Header record of the files
		/// </summary>
		public DateTime FileTimestamp { get; set; }

		/// <summary>
		/// Gets and sets the file sequence number (1 or more) which is in the 01 Header record of the files
		/// </summary>
		public int FileNumber { get; set; }

		/// <summary>
		/// In case a file comes in with only header info, save it to be sent to the EndOfFile event so Header Info can be used if needed for an otherwise empty file
		/// </summary>
		public string OrphanedHeaderData { get; set; }
	}
}