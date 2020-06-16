using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

namespace CSS.Connector.FileProcessing.Parsing.Parser
{
    /// <summary>
    /// Parses fixed length flat files based on pre-defined file layouts
    /// </summary>
    public class GenericFileParser
    {
        #region Private Fields
        private FileProcessorConfig _fileProcessorConfig;
        private Assembly _resourcesAssembly;
        private string _regexResourceName;
        private string _logicalRecordGroupingResourceName;
        private bool _sendDataBlocksToDelegate;
        private bool _shouldRecordTypeTagsBeCreated;

        private RegexDictionary _regexDictionary;
        private RegexDictionary RegexDictionary
        {
            get
            {
                if (_regexDictionary == null)
                {
                    _regexDictionary = new RegexDictionary(_regexResourceName, _resourcesAssembly);
                }
                return _regexDictionary;
            }
        }

        private XmlDataWriter _writer;
        private XmlDataWriter Writer
        {
            get
            {
                if (_writer == null) Writer = DataWriter;
                return _writer;
            }
            set { _writer = value; }
        }

        private XmlDataWriter _dataWriter;
        private XmlDataWriter DataWriter
        {
            get
            {
                if (_dataWriter == null) _dataWriter = new XmlDataWriter(_fileProcessorConfig.XmlNameSpace);
                return _dataWriter;
            }
            set { _dataWriter = value; }
        }

        private XmlDataWriter _headerWriter;
        private XmlDataWriter HeaderWriter
        {
            get
            {
                if (_headerWriter == null) _headerWriter = new XmlDataWriter(_fileProcessorConfig.XmlNameSpace);
                return _headerWriter;
            }
        }

        private HeaderCollection _headerCollection;
        internal HeaderCollection HeaderCollection
        {
            get
            {
                if (_headerCollection == null) _headerCollection = new HeaderCollection();
                return _headerCollection;
            }
        }

        private const string RecordTypeRegexName = "RecordType";
        #endregion

        #region Constructors
        private GenericFileParser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the GenericFileParser.
        /// </summary>
        /// <param name="fileProcessorConfig">File processing configuration from the embedded FileProcessor.Config file in the processing code for a particular file type.</param>
        /// <param name="shouldRecordTypeTagsBeCreated">If this is true, XML record tags will be created for flat file record types (i.e. F51).</param>
        public GenericFileParser(FileProcessorConfig fileProcessorConfig, bool shouldRecordTypeTagsBeCreated)
        {
            _fileProcessorConfig = fileProcessorConfig;
			_resourcesAssembly = Assembly.LoadFrom(fileProcessorConfig.ResourcesAssembly); //   "CSS.Connector.FileProcessors.Vanguard.dll");
            string startEmbeddedResourceName = fileProcessorConfig.ResourcesNamespace + "." + fileProcessorConfig.FileType;
            _regexResourceName = startEmbeddedResourceName + "RegularExpressions";
            _logicalRecordGroupingResourceName = startEmbeddedResourceName + "LogicalRecordGrouping.xml";
            _shouldRecordTypeTagsBeCreated = shouldRecordTypeTagsBeCreated;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Event that fires when processing a record block is complete.
        /// </summary>
        public event EventHandler<GenericFileParserEventArgs> RecordBlockComplete;

		/// <summary>
		/// Event that fires when file has been read, but only contains header info
		/// </summary>
		public event EventHandler<GenericFileParserEventArgs> HeaderOnlyFile;

		/// <summary>
		/// Converts flat file into XML format based on File Definition XML.  Use this function to convert the
		/// file in one pass where the output is written to the writer passed into this function.
		/// </summary>
		/// <param name="reader">A reader for the flat file</param>
		/// <param name="writer">A writer for the XML file</param>
		public void ParseFile(TextReader reader, XmlWriter writer)
        {
            this._sendDataBlocksToDelegate = false;
            this.DataWriter = new XmlDataWriter(writer, _fileProcessorConfig.XmlNameSpace);
            this.ReadFile(reader);
        }

        /// <summary>
        /// Converts flat file into XML based on File Definition XML.  Use this function to have blocks of
        /// the parsed data sent back to the caller through a delegate.  What designates a block is determined
        /// from the blocking level set up in the Fixed Length Schema Editor.  With each block, the header info
        /// that applies to that data block will be included.
        /// </summary>
        /// <param name="reader">The flat file reader</param>
        public void ParseFile(TextReader reader)
        {
            this._sendDataBlocksToDelegate = true;
            this.ReadFile(reader);
        }
        #endregion

        #region Private Methods
        private void ReadFile(TextReader reader)
        {
            RecordTextReader flatFileTextReader = new RecordTextReader(
                RegexDictionary[RecordTypeRegexName].ToString(),
                _logicalRecordGroupingResourceName,
                _resourcesAssembly,
                _sendDataBlocksToDelegate,
                _fileProcessorConfig.IgnoreUndefinedRecordTypes);

            SubscribeToReaderEvents(flatFileTextReader);
            flatFileTextReader.ReadRecordData(reader);
        }

        private void SubscribeToReaderEvents(RecordTextReader textReader)
        {
            textReader.LineWasRead              += new EventHandler<RecordTextReaderEventArgs>(reader_LineWasRead);
            textReader.StartOfHeaderGroup       += new EventHandler<RecordTextReaderEventArgs>(reader_StartOfHeaderGroup);
            textReader.EndOfHeaderGroup         += new EventHandler<RecordTextReaderEventArgs>(reader_EndOfHeaderGroup);
            textReader.HeaderNoLongerApplicable += new EventHandler<RecordTextReaderEventArgs>(reader_HeaderNoLongerApplicable);
            textReader.StartOfDataGroup         += new EventHandler<RecordTextReaderEventArgs>(reader_StartOfDataGroup);
            textReader.EndOfDataGroup           += new EventHandler<RecordTextReaderEventArgs>(reader_EndOfDataGroup);
            textReader.StartOfRepeatingGroup    += new EventHandler<RecordTextReaderEventArgs>(reader_StartOfDataGroup);
            textReader.EndOfRepeatingGroup      += new EventHandler<RecordTextReaderEventArgs>(reader_EndOfDataGroup);
            textReader.StartOfBlock             += new EventHandler<RecordTextReaderEventArgs>(reader_StartOfBlock);
            textReader.EndOfBlock               += new EventHandler<RecordTextReaderEventArgs>(reader_EndOfBlock);
			textReader.HeaderOnlyFile           += new EventHandler<RecordTextReaderEventArgs>(reader_HeaderOnlyFile);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)")]
        private void ParseLine(string line, string recordType)
        {
            if (RegexDictionary.ContainsKey(recordType))
            {
                Regex regex = RegexDictionary[recordType];
                Match match = regex.Match(line);
                if (!match.Success)
                {
                    throw new InvalidDataException(
                            string.Format(Errors.ErrorParsingLine, line, RegexDictionary[recordType]));
                }

                if (_shouldRecordTypeTagsBeCreated) Writer.WriteGroupStart(recordType);
                ParseFields(match, regex.GetGroupNames());
                if (_shouldRecordTypeTagsBeCreated) Writer.WriteGroupEnd();
            }
        }

        private void ParseFields(Match match, string[] elementNames)
        {
            if ((elementNames.Length == 1) && (elementNames[0] == "0"))
            {
                // This logic will execute if there are no field level matches
                // So don't output unless record type tags are being written
                if (_shouldRecordTypeTagsBeCreated) this.Writer.WriteValue(match.Groups[0].Value);
            }
            else
            {
                foreach (string elementName in elementNames)
                {
                    if (!Char.IsDigit(elementName, 0))
                    {
                        foreach (Capture capture in match.Groups[elementName].Captures)
                        {
                            this.Writer.WriteNameValuePair(elementName, capture.Value.Trim());
                        }
                    }
                }
            }
        }

        private void reader_LineWasRead(object sender, RecordTextReaderEventArgs e)
        {
            ParseLine(e.CurrentLine, e.CurrentRecordType);
        }

        private void reader_StartOfHeaderGroup(object sender, RecordTextReaderEventArgs e)
        {
            this.Writer = this.HeaderWriter;
            this.HeaderCollection.Add(e.CurrentRecordGroup, e.RecordNumber, "");
        }

        private void reader_EndOfHeaderGroup(object sender, RecordTextReaderEventArgs e)
        {
            if (HeaderWriter.IsThereXmlData()) this.HeaderCollection.UpdateLastDataFragment(HeaderWriter.GetXmlData());
            this.Writer = this.DataWriter;
        }

        private void reader_HeaderNoLongerApplicable(object sender, RecordTextReaderEventArgs e)
        {
            this.HeaderCollection.RemoveLastHeader();
        }

        private void reader_StartOfDataGroup(object sender, RecordTextReaderEventArgs e)
        {
            this.Writer.WriteGroupStart(e.CurrentRecordGroup, e.RecordNumber, e.IsBlockingRecord);
        }

        private void reader_EndOfDataGroup(object sender, RecordTextReaderEventArgs e)
        {
            this.Writer.WriteGroupEnd();
        }

        private void reader_StartOfBlock(object sender, RecordTextReaderEventArgs e)
        {
            this.HeaderCollection.WriteHeaderXml(this.Writer);
        }

        private void reader_EndOfBlock(object sender, RecordTextReaderEventArgs e)
        {
            if (this.RecordBlockComplete != null)
            {
                this.RecordBlockComplete(
                    this,
                    new GenericFileParserEventArgs(
                        e.CurrentRecordGroup,
                        this.DataWriter.GetXmlData()));
            }
        }

		private void reader_HeaderOnlyFile(object sender, RecordTextReaderEventArgs e)
		{
			if (this.HeaderOnlyFile != null)
			{
				if (HeaderWriter.IsThereXmlData()) this.HeaderCollection.UpdateLastDataFragment(HeaderWriter.GetXmlData());
				this.Writer = this.DataWriter;
				this.HeaderCollection.WriteHeaderXml(this.Writer);
				this.HeaderOnlyFile(
					this,
					new GenericFileParserEventArgs(
						e.CurrentRecordGroup,
						this.Writer.GetXmlData()));
			}
		}
		#endregion
	}
}