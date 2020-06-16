using System;
using System.Collections.Generic;
using System.IO;
using CSS.Connector.FileProcessing.Parsing.Parser;

namespace CSS.Connector.FileProcessing.Parsing.Processor
{
    /// <summary>
    /// Reusable FileProcessor class that will process one logical record at a time.
    ///		In the Inbound File Schema editor, a blocking record can be identified in an inbound file, such that for this record, blocks of data will be
    ///		sent to the processing program.  It is a judgment call of what to make the blocking record.  In the case of the 156 position file, there is a
    ///		header record for the CUSIP that could be used as the blocking record.  In this case, all the accounts under the given CUSIP will be sent to
    ///		the processing program in one block of XML.  This could take up a lot of memory depending on how many accounts are in the CUSIP.  Another alternative
    ///		would be to make the account record the blocking record.  In that case, a block of XML for each account will be sent to the processing program when the
    ///		end of the blocking record has been reached (i.e. a higher level record, like a trailer, or another blocking record, or end of file, has been read).
    ///		Included with each detail account, would be the header product xml that applies to that account.  This approach would less likely cause memory issues.
    ///		In this case, each account block of XML wouldn't know about the other accounts under the CUSIP.
    /// See this document for more details:  $/Gold/Enterprise/CSS/SDK/Utilities/FixedLengthFile/Setting up a program to process a fixed length file using CSS File Processing.docx
    /// </summary>
    public class RecordBlockingFileProcessor : FileProcessor
    {
        #region Private Methods
        private FileCompletionInfo CompletionInfo = new FileCompletionInfo();

        private void parser_RecordBlockComplete(object sender, GenericFileParserEventArgs e)
        {
            if (this.RecordProcessorConfigOptions.ContainsKey(e.RecordType) &&
                this.RecordProcessorConfigOptions[e.RecordType].IgnoreRecordType)
                return;
            if (this.RecordProcessors.ContainsKey(e.RecordType))
            {
				this.RecordProcessors[e.RecordType].ProcessRecord(e.DataBlock);
				CompletionInfo.TotalRecords += 1;
            }
            else
            {
                throw new KeyNotFoundException(string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "There are no record processors configured for record type {0}.",
                    e.RecordType));
            }
        }

		private void parser_HeaderOnlyFile(object sender, GenericFileParserEventArgs e)
		{
			CompletionInfo.OrphanedHeaderData = e.DataBlock;
		}
		#endregion

		/// <summary>
		/// Initializes a new instance of the RecordBlockingFileProcessor class for the given config data.
		/// </summary>
		/// <param name="fileProcessorConfigXml">Serialized FileProcessorConfig data.</param>
		public RecordBlockingFileProcessor(string fileProcessorConfigXml)
		: base(fileProcessorConfigXml)
		{
		}

        /// <summary>
        /// Initializes a new instance of the RecordBlockingFileProcessor class using the specified file configuration
        /// </summary>
        /// <param name="config">The file configuration the RecordBlockingFileProcess will process.</param>
        public RecordBlockingFileProcessor(FileProcessorConfig config)
            : base(config)
        {
        }

        /// <summary>
        /// Processes the specified file.
        /// </summary>
        /// <param name="fileName">The name of location of the file to process.</param>
        /// <returns>The file processing completion details.</returns>
        public override FileCompletionInfo ProcessFile(string fileName, string instanceId)
        {
			// Shouldn't need try/catch here since called method will handle that
			using (StreamReader reader = new StreamReader(fileName))
			{
				return ProcessFile(reader, fileName, instanceId);
			}
		}

		/// <summary>
		/// Processes the provided file data.
		/// </summary>
		/// <param name="reader">A reader for the file data to process.</param>
		/// <param name="fileName">The name of the file being processed.</param>
		/// <returns>The file processing completion details.</returns>
		public override FileCompletionInfo ProcessFile(TextReader reader, string fileName, string instanceId)
        {
			try
			{
				GenericFileParser parser = new GenericFileParser(
					FileConfig,
					false);
				parser.RecordBlockComplete += new EventHandler<GenericFileParserEventArgs>(parser_RecordBlockComplete);
				parser.HeaderOnlyFile      += new EventHandler<GenericFileParserEventArgs>(parser_HeaderOnlyFile);
				CallProcessorForBeginningOfFile(fileName, instanceId);
				parser.ParseFile(reader);
				return CompletionInfo;
			}
			catch (Exception e)
			{
				CompletionInfo.FatalErrorOccurred = true;
				CompletionInfo.FatalErrorMessage += "!" + e.ToString();
				return CompletionInfo;
			}
			finally
			{
				try
				{
					CallProcessorForEndOfFile(ref CompletionInfo);
				}
				catch (Exception e)
				{
					CompletionInfo.FatalErrorOccurred = true;
					CompletionInfo.FatalErrorMessage += "!" + e.ToString();
				}
			}
		}

		private void CallProcessorForBeginningOfFile(string fileName, string instanceId)
		{
			if (string.IsNullOrEmpty(FileConfig.BeginningOfFileProcessorTypeName)) return; // Do nothing if config does not call for this
			((IFileEventProcessors)Activator.CreateInstance(Type.GetType(FileConfig.BeginningOfFileProcessorTypeName, true, true))).BeginningOfFileEventProcessor(fileName, instanceId);
		}

		private void CallProcessorForEndOfFile(ref FileCompletionInfo completionInfo)
		{
			if (string.IsNullOrEmpty(FileConfig.EndOfFileProcessorTypeName)) return; // Do nothing if config does not call for this
			((IFileEventProcessors)Activator.CreateInstance(Type.GetType(FileConfig.EndOfFileProcessorTypeName, true, true))).EndOfFileEventProcessor(ref completionInfo);
		}
	}
}