namespace CSS.Connector.FileProcessing.Models
{
    /// <summary>
    /// Base class for all file event processing classes.  Currently supported events are Begining of File and End of File events.
    /// </summary>
    public interface IFileEventProcessors
    {
		/// <summary>
		/// Processes the beginning of File event given the file path and name.
		/// </summary>
		/// <param name="fileName">Contains the file path and name being processed.</param>
		void BeginningOfFileEventProcessor(string fileName, string instanceId);

		/// <summary>
		/// Processes the end of File event given the status info (FileCompletionInfo) of the processing of the file thus far
		/// </summary>
		/// <param name="completionInfo">Contains the completion info of the file being processed. May be updated in the call.</param>
		void EndOfFileEventProcessor(ref FileCompletionInfo fileCompletionInfo);
	}
}