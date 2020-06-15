using System;
using System.Collections.Generic;
using System.Text;

namespace CSS.Connector.FileProcessing.Core.Models
{
    /// <summary>
    /// Represents a definition of a flat file.
    /// </summary>
    public class FileDefinition
    {
        /// <summary>
        /// Numeric id of the file.
        /// </summary>
        public int Id { get; set; }

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
        
		/// <summary>
        /// Whether or not to compute an MD5 hash of the raw file bytes to determine if the file was run before.
        /// </summary>
        public bool UseHashCodeDuplicateDetection { get; set; }

		/// <summary>
		/// True to use built-in file parsing where XML data blocks are passed to a record processor (ProcessRecord) one logical block at a time.
		/// If this is false, the processing code must parse the data and contain a ProcessFile method.
		/// </summary>
		public bool UseFileParsing { get; set; }

        /// <summary>
        /// List of FileInstances
        /// </summary>
        public List<FileInstance> FileInstances { get; set; }
		
		/// <summary>
        /// The fully qualified .NET assembly name to process this file.
        /// </summary>
        public string Processor { get; set; }

    }
}
