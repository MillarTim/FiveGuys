using System;

namespace CSS.Connector.FileProcessing.Parsing.Parser
{
    /// <summary>
    /// Contains details about an inbound transmission file.
    /// </summary>
    public class FileInfo
    {
        /// <summary>
        /// Initializes a new instance of the FileInfo class
        /// </summary>
        public FileInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the FileInfo class using the details passed
        /// </summary>
        /// <param name="type">The FileProcessor type name used to process the specified file.</param>
        /// <param name="date">The file date from the header record.</param>
        /// <param name="typeId">Three digit ID of the file (e.g. 084 = Mutual Funds Networking File).</param>
        /// <param name="sequenceNumber">The number of instances the file type ID has been sent.</param>
        /// <param name="source">The source of the file (e.g. DTC, NSC).</param>
        /// <param name="fullName">The full name, include path, of the file.</param>
        /// <param name="autoRouteNumber">Auto Route Number/Product Number</param>
        /// <param name="shortName">The file name, excluding the path, of the file.</param>
        public FileInfo(
            string type,
            DateTime date,
            string typeId,
            int sequenceNumber,
            string source,
            string fullName,
            string autoRouteNumber,
            string shortName)
        {
            TypeString = type;
            Date = date;
            TypeId = typeId;
            SequenceNumber = sequenceNumber;
            Source = source;
            FullName = fullName;
            AutoRouteNumber = autoRouteNumber;
            ShortName = shortName;
        }

        /// <summary>
        /// Gets or sets a string that contains the FileProcessor type name used to process the specified file.
        /// </summary>
        public string TypeString { get; set; }

        /// <summary>
        /// Gets or sets the file date from the header record.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the three digit ID of the file (e.g. 084 = Mutual Funds Networking File).
        /// </summary>
        public string TypeId { get; set; }

        /// <summary>
        /// Gets or sets the number of instance of the file type ID has been sent for the <see cref="Date"/>
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the source of the file (e.g. DTC, NSC).
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// Gets or sets the full name, including path, of the file.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the automatic route number.
        /// </summary>
        /// <value>
        /// The automatic route number / product number.
        /// </value>
        public string AutoRouteNumber { get; set; }

        /// <summary>
        /// Gets or sets the file name, excluding the path, of the file.
        /// </summary>
        public string ShortName { get; set; }
    }
}