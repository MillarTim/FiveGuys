using System;
using System.Collections.Generic;
using System.Text;

namespace CSS.Connector.FileProcessing.Core
{
    public class FileInstanceDisplay
    {
        /// <summary>
        /// A GUID of the instance.
        /// </summary>
        public string InstanceId { get; set; }
        /// <summary>
        /// The begin time that the file was registerd in the file watcher.
        /// </summary>
        public DateTime BeginTime { get; set; }
        /// <summary>
        /// The end time in which the file was completed.
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 32 character hex string that represents the fingerprint of the file.
        /// </summary>
        public string HashCode { get; set; }
        /// <summary>
        /// Foreign key to the FileDefintion.Id field.
        /// </summary>
        public int FileId { get; set; }
        /// <summary>
        /// Indicates whether or not the file run was successful or not.
        /// </summary>
        public bool Successful { get; set; }
		/// <summary>
		/// Indicates whether or not the file is currently being processed. Based on the EndTime and Message field both being null.
		/// </summary>
		public bool IsInProcess { get; set; }
		/// <summary>
		/// Information message about the file run.
		/// </summary>
		public string Message { get; set; }
        /// <summary>
        /// Sequence number of the file, if available.
        /// </summary>
        public int? SequenceNumber { get; set; }
        /// <summary>
        /// File date of the file, if available.
        /// </summary>
        public DateTime? FileDate { get; set; }
        /// <summary>
        /// Name of the file used from a business perspective.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 'Input' or 'Output'
        /// </summary>
        public string Direction { get; set; }
        /// <summary>
        /// Regular Expression that defines the file based on the file name.  e.g. File\d\d.txt will match File01.txt and File02.txt
        /// </summary>
        public string RegexNameExpression { get; set; }
    }
}
