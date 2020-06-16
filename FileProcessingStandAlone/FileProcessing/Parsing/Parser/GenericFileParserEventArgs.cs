using System;

namespace CSS.Connector.FileProcessing.Parsing.Parser
{
    /// <summary>
    /// Represents event data for the GenericFileParser
    /// </summary>
    public class GenericFileParserEventArgs : EventArgs
    {
        #region Prviate Members
        private string _recordType;
        private string _dataBlock;
        #endregion

        internal GenericFileParserEventArgs(string recordType, string dataBlock)
        {
            _recordType = recordType;
            _dataBlock = dataBlock;
        }

        #region Public Properties
        /// <summary>
        /// Gets the record type
        /// </summary>
        public string RecordType
        {
            get { return _recordType; }
        }

        /// <summary>
        /// Gets the data for the record
        /// </summary>
        public string DataBlock
        {
            get { return _dataBlock; }
        }
        #endregion
    }
}