namespace CSS.Connector.FileProcessing.Parsing.Processor
{
    /// <summary>
    /// Base class for all record processing classes
    /// </summary>
    public interface IRecordProcessor
    {
        /// <summary>
        /// Processes an string containing an XML record for an inbound file.
        /// </summary>
        /// <param name="recordXml">Contains the XML representation of the flat file record.</param>
        void ProcessRecord(string recordXml);
    }
}