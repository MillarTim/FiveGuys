using System;
using System.Collections.Generic;
using System.IO;

using CSS.Connector.FileProcessing.Parsing.Parser;

namespace CSS.Connector.FileProcessing.Parsing.Processor
{
    /// <summary>
    /// Base class for all file record processors to make sure they all implement the same interface as well
    /// as contain common reusable functionality.
    /// </summary>
    abstract public class FileProcessor
    {
        #region Private and Protected members
        private Dictionary<string, IRecordProcessor> _recordProcessors;
        /// <summary>
        /// A dictionary of record processors required by a particular file type.
        /// </summary>
        protected Dictionary<string, IRecordProcessor> RecordProcessors
        {
            get
            {
                if (_recordProcessors == null)
                {
                    _recordProcessors = new Dictionary<string, IRecordProcessor>(FileConfig.RecordProcessorConfigs.Count);
                    foreach (RecordProcessorConfig recordProcessorConfig in FileConfig.RecordProcessorConfigs)
                    {
                        if (!recordProcessorConfig.IgnoreRecordType)
                        {
							_recordProcessors.Add(
								recordProcessorConfig.RecordType,
								(IRecordProcessor)Activator.CreateInstance(Type.GetType(recordProcessorConfig.RecordProcessorTypeName, true, true)));
                        }
                    }
                }
                return _recordProcessors;
            }
        }

        private Dictionary<string, RecordProcessorConfig> _recordProcessorConfigOptions;
        /// <summary>
        /// A dictionary containing configuration options for the record processors used to process a particular file type
        /// </summary>
        protected Dictionary<string, RecordProcessorConfig> RecordProcessorConfigOptions
        {
            get
            {
                if (_recordProcessorConfigOptions == null)
                {
                    _recordProcessorConfigOptions = new Dictionary<string, RecordProcessorConfig>(FileConfig.RecordProcessorConfigs.Count);
                    foreach (RecordProcessorConfig recordProcessorConfig in FileConfig.RecordProcessorConfigs)
                    {
                        _recordProcessorConfigOptions.Add(recordProcessorConfig.RecordType, recordProcessorConfig);
                    }
                }
                return _recordProcessorConfigOptions;
            }
        }

        private FileProcessorConfig _fileConfig;
        internal FileProcessorConfig FileConfig
        {
            get { return _fileConfig; }
            private set { _fileConfig = value; }
        }
        #endregion

        #region Constructors
        private FileProcessor()
        {
        }

		/// <summary>
		/// Initializes a new instance of the RecordBlockingFileProcessor class for the given config data.
		/// </summary>
		/// <param name="fileProcessorConfigXml">Serialized FileProcessorConfig data.</param>
		public FileProcessor(string fileProcessorConfigXml)
		{
			System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(Parsing.Parser.FileProcessorConfig));
			using (StringReader textReader = new StringReader(fileProcessorConfigXml))
			{
				FileConfig = (FileProcessorConfig)ser.Deserialize(textReader);
			}
		}

		/// <summary>
		/// Initializes a new instance of the FileProcessor class using the provided file configuration
		/// </summary>
		/// <param name="config">The file processor configuration to use to create the FileProcessor</param>
		public FileProcessor(FileProcessorConfig config)
        {
            this.FileConfig = config;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Processes the specified inbound file
        /// </summary>
        /// <param name="fileName">The full file name, including name and location, of the file to be processed.</param>
        /// <returns>The file completion info from the processing including statistics</returns>
        public abstract FileCompletionInfo ProcessFile(string fileName, string instanceId);

		/// <summary>
		/// Processes the specified file data
		/// </summary>
		/// <param name="reader">A text reader for the file data.</param>
		/// <param name="fileName">The name of the file being processed.  Only used to pass in the BeginningOfFileProcessor.</param>
		/// <returns>The file completion info from the processing including statistics</returns>
		public abstract FileCompletionInfo ProcessFile(TextReader reader, string fileName, string instanceId);
        #endregion
    }
}