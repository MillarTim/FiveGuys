using System;

namespace CSS.Connector.FileProcessing.Parsing.Parser
{
    /// <summary>
    /// Represents the event data for a text reader record
    /// </summary>
    public class RecordTextReaderEventArgs : EventArgs
    {
        #region Private Members
        private int _recordNumber;
        private string _currentLine;
        private string _currentRecordType;
        private string _currentRecordGroup;
        private bool _isBlockingRecord;
        #endregion

        internal RecordTextReaderEventArgs(
            int recordNumber,
            string currentLine,
            string currentRecordType,
            string currentRecordGroup,
            bool isBlockingRecord)
        {
            _recordNumber = recordNumber;
            _currentLine = currentLine;
            _currentRecordType = currentRecordType;
            _currentRecordGroup = currentRecordGroup;
            _isBlockingRecord = isBlockingRecord;
        }

        #region Public Properties
        /// <summary>
        /// Gets the number of the record being processed.
        /// </summary>
        public int RecordNumber
        {
            get { return _recordNumber; }
        }

        /// <summary>
        /// Gets the body of the current record.
        /// </summary>
        public string CurrentLine
        {
            get { return _currentLine; }
        }

        /// <summary>
        /// Gets the current record type.
        /// </summary>
        public string CurrentRecordType
        {
            get { return _currentRecordType; }
        }

        /// <summary>
        /// Gets the current record group.
        /// </summary>
        public string CurrentRecordGroup
        {
            get { return _currentRecordGroup; }
        }

        /// <summary>
        /// Gets a flag indicating if the record is a blocking record.
        /// </summary>
        public bool IsBlockingRecord
        {
            get { return _isBlockingRecord; }
        }
        #endregion
    }
}