namespace CSS.Connector.FileProcessing.Parsing.Parser
{
/// <summary>Represents a collection of RecordProcessorConfig objects.</summary>
public partial class RecordProcessorConfigCollection : System.Collections.ObjectModel.Collection<RecordProcessorConfig>
{
    /// <summary>Initializes a new instance of RecordProcessorConfigCollection.</summary>
    /// <param name="list">The list that is wrapped by the new collection.</param>
    public RecordProcessorConfigCollection(System.Collections.Generic.IList<RecordProcessorConfig> list) : 
            base(list)
    {
    }
    
    /// <summary>Initializes a new instance of RecordProcessorConfigCollection.</summary>
    public RecordProcessorConfigCollection()
    {
    }
}
}
