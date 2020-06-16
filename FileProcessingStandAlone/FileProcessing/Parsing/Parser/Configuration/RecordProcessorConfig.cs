namespace CSS.Connector.FileProcessing.Parsing.Parser
{
/// <summary>Within a File Processor Configuration there will be one or more record processors.  Each one will contain configuration information on how a specific record type will be processed.</summary>
public partial class RecordProcessorConfig : CssObject
{
    /// <summary>Initializes a new instance of RecordProcessorConfig.</summary>
    public RecordProcessorConfig(){ }
    
    /// <summary>The record type name, such as FundAccountUpdate</summary>
    public string RecordType { get; set; }
    
    /// <summary>The assembly and class that contains the ProcessRecord method to process this particular record type.</summary>
    public string RecordProcessorTypeName { get; set; }
    
    /// <summary>True if there is no harm in reprocessing this record type.  No errors will be thrown if this is true.</summary>
    public bool CanBeReprocessed { get; set; }
    
    /// <summary>True if this record type is to be ignored.  No code will be called to process the record.</summary>
    public bool IgnoreRecordType { get; set; }
}
}
