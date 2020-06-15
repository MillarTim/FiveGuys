using System.Collections.Generic;

namespace CSS.Connector.FileProcessing.Core.Models
{
    /// <summary>
    /// File processing configuration for files that will be parsed using the CSS.Connector.FileProcessing.Parsing namespace
    /// </summary>
    public class FileProcessorConfig
    {
        /// <summary>
        /// The name of the processor.  Ties to the FileDefinitions.Processor column
        /// </summary>
        public string Processor { get; set; }

        /// <summary>
        /// XML configuration for file processor including record processors
        /// </summary>
        public string Config { get; set; }
    }
}
