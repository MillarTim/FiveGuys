using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CSS.Connector.FileProcessing.Parsing.Parser
{
	internal class RecordTextReader
	{
		#region Member variables and properties
		private bool _useBlocking;
		private bool _useGroupCaptures;
		private bool _ignoreUndefinedRecordTypes;
		private bool _processingHeaderData;
		private int _physicalFileRecordNumber;
		private long _blockingGroupCount;
		private string _currentLine;
		private string _currentRecordType;
		private string _currentGroup;
		private string _recordTypeRegularExpression;
		private string _logicalRecordGroupingResourceName;
		private Assembly _resourcesAssembly;

		private LogicalRecordGroupingHashTable _recordGroupingHashTable;
		private LogicalRecordGroupingHashTable RecordGroupingHashTable
		{
			get
			{
				if (_recordGroupingHashTable == null)
				{
					_recordGroupingHashTable = new LogicalRecordGroupingHashTable(_logicalRecordGroupingResourceName, _resourcesAssembly);
				}
				return _recordGroupingHashTable;
			}
		}

		private Stack<string> _recordTypeStack;
		private Stack<string> RecordTypeStack
		{
			get
			{
				if (_recordTypeStack == null)
				{
					_recordTypeStack = new Stack<string>();
				}
				return _recordTypeStack;
			}
			//			set { _recordTypeStack = value; }
		}
		#endregion Member variables and properties

		#region Constructors
		private RecordTextReader()
		{
		}

		public RecordTextReader(
			string recordTypeRegularExpression,
			string logicalRecordGroupingResourceName,
			Assembly resourcesAssembly,
			bool useBlocking,
			bool ignoreUndefinedRecordTypes)
		{
			_recordTypeRegularExpression = recordTypeRegularExpression;
			_logicalRecordGroupingResourceName = logicalRecordGroupingResourceName;
			_resourcesAssembly = resourcesAssembly;
			_processingHeaderData = _useBlocking = useBlocking;
			_ignoreUndefinedRecordTypes = ignoreUndefinedRecordTypes;
			if (useBlocking && !RecordGroupingHashTable.ContainsBlocking)
			{
				throw new ConfigurationErrorsException(Errors.MissingBlockingInformation);
			}
			_useGroupCaptures = RecordGroupingHashTable.UseGroupCaptures;
		}
		#endregion Constructors

		#region Events
		public event EventHandler<RecordTextReaderEventArgs> LineWasRead;
		public event EventHandler<RecordTextReaderEventArgs> StartOfHeaderGroup;
		public event EventHandler<RecordTextReaderEventArgs> EndOfHeaderGroup;
		public event EventHandler<RecordTextReaderEventArgs> HeaderNoLongerApplicable;
		public event EventHandler<RecordTextReaderEventArgs> StartOfDataGroup;
		public event EventHandler<RecordTextReaderEventArgs> EndOfDataGroup;
		public event EventHandler<RecordTextReaderEventArgs> StartOfRepeatingGroup;
		public event EventHandler<RecordTextReaderEventArgs> EndOfRepeatingGroup;
		public event EventHandler<RecordTextReaderEventArgs> StartOfBlock;
		public event EventHandler<RecordTextReaderEventArgs> EndOfBlock;
		public event EventHandler<RecordTextReaderEventArgs> HeaderOnlyFile;
		#endregion Events

		#region Public functions
		/// <summary>
		/// Read each line of the reader passed in.  Events of type RecordTextReaderEventArgs will be raised when
		/// a line is read and when header, group, and block events occur.  See fixed length schema editor to
		/// define record layouts with these characteristics.
		/// </summary>
		/// <param name="reader">The text reader to read.  (Can be string or stream reader.)</param>
		public void ReadRecordData(TextReader reader)
		{
			_physicalFileRecordNumber = -1;
			string fileElementName = RecordGroupingHashTable.RootNodeName;
			string line;

			while ((line = reader.ReadLine()) != null)
			{
				this._currentLine = line;

				// Some files from NSCC contain x00 instead of a space (x20, Dec20).  Replace with space to avoid error.
				if (this._currentLine.Contains("\0"))
				{
					this._currentLine = this._currentLine.Replace("\0", " ");
				}

				_physicalFileRecordNumber++;
				ProcessCurrentLine();
			}
			// _currentRecordType = fileElementName;	// to insure everything gets cleaned up
			// Use the record type in the bottom of the stack instead of the File Type because they may not match
			// and with the commented line above, an error is thrown if they don't match
			if (RecordTypeStack.Count < 1) return;
			_currentRecordType = (RecordTypeStack.ToArray())[RecordTypeStack.Count - 1];
			CheckAndProcessHeaderOnlyFile();
			ClosePreviousGroup();		// at EOF, write any record blocks that are pending
		}
		#endregion Public functions

		#region Private functions
		private void ProcessCurrentLine()
		{
			try
			{
				string recType = GetRecordTypeOfCurrentLine();

				if (recType == null)
				{
					if (_ignoreUndefinedRecordTypes)
					{
						return;
					}

					throw new System.Configuration.ConfigurationErrorsException("The record type could not be determined.");
				}
				// Record type may not be able to be determined without knowing record group being processed, so pass that in if it exists.
				this._currentRecordType = RecordGroupingHashTable.GetRecordTypeFromAlias(recType, RecordTypeStack.ToArray());

				if (this._currentRecordType == null)
				{
					if (_ignoreUndefinedRecordTypes)
					{
						return;
					}

					throw new System.Configuration.ConfigurationErrorsException(string.Format(CultureInfo.InvariantCulture, "Record type {0} has not been defined.", recType));
				}

				ProcessRecordGrouping();

				if (LineWasRead != null)
				{
					LineWasRead(this, GetEventArgs());
				}
			}
			catch (Exception e)
			{
				e.Data["CurrentLine"] = this._currentLine;
				e.Data["PhysicalRecordNumber"] = _physicalFileRecordNumber.ToString(CultureInfo.InvariantCulture);
				throw;
			}
		}

		private void ProcessRecordGrouping()
		{
			if (RecordGroupingHashTable.Contains(_currentRecordType))
			{
				bool isFirstRecordOfRepeatingGroup = IsFirstRecordOfRepeatingGroup();
				ClosePreviousGroup();

				if (isFirstRecordOfRepeatingGroup)
				{
					StartOfRepeatingGroup(this, GetEventArgs(true));
				}

				// this group starting tag (if it's not null) must be written to xml & saved to stack for later closing (ending tag)
				_currentGroup = RecordGroupingHashTable.GetRecordGroupName(_currentRecordType);

				if (_currentGroup != null)
				{
					ProcessStartOfGroup();
				}
			}
		}

		private bool IsLastRecordOfRepeatingGroup()
		{
			return
				RecordTypeStack.Count > 0 &&
				_currentRecordType != RecordTypeStack.Peek() &&
				RecordGroupingHashTable.IsRepeatingGroup(RecordTypeStack.Peek());
		}

		private bool IsFirstRecordOfRepeatingGroup()
		{
			return RecordGroupingHashTable.IsRepeatingGroup(_currentRecordType) &&
				(RecordTypeStack.Count == 0 ||
				(_currentRecordType != RecordTypeStack.Peek()));
		}

		private void ClosePreviousGroup()
		{
			while (
				RecordTypeStack.Count > 0 &&
				RecordGroupingHashTable.GetXmlDepthLevel(_currentRecordType) <=
				RecordGroupingHashTable.GetXmlDepthLevel(RecordTypeStack.Peek()))
			{
				ProcessEndOfGroup();
			}
		}

		private void ProcessStartOfGroup()
		{
			bool thisIsABlockingLevelNode = ThisIsABlockingLevelNode(_currentRecordType);
			if (_processingHeaderData)
			{
				EndOfHeaderGroup(this, GetEventArgs());
				if (thisIsABlockingLevelNode)
				{
					_processingHeaderData = false;
				}
				else
				{
					StartOfHeaderGroup(this, GetEventArgs());
				}
			}
			if (thisIsABlockingLevelNode && _blockingGroupCount == 0)
			{
				StartOfBlock(this, GetEventArgs());
			}

			if (!_processingHeaderData)
			{
				StartOfDataGroup(this, GetEventArgs());
			}

			RecordTypeStack.Push(_currentRecordType);
		}

		private void ProcessEndOfGroup()
		{
			string recordTypeOfPreviousGroup = RecordTypeStack.Peek();

			_currentGroup = RecordGroupingHashTable.GetRecordGroupName(recordTypeOfPreviousGroup);	// Group name is needed for certain functions like EndOfDataGroup

			if (!_processingHeaderData)
			{
				EndOfDataGroup(this, GetEventArgs());
			}

			if (IsLastRecordOfRepeatingGroup())
			{
				EndOfRepeatingGroup(this, GetEventArgs());
			}

			if (ThisIsABlockingLevelNode(recordTypeOfPreviousGroup))
			{
				if (RecordGroupingHashTable.GetXmlDepthLevel(_currentRecordType) <
					RecordGroupingHashTable.GetXmlDepthLevel(recordTypeOfPreviousGroup))
				{
					_processingHeaderData = true;
					EndOfBlock(this, GetEventArgs());
					_blockingGroupCount = 0;
				}
				else if (recordTypeOfPreviousGroup != _currentRecordType ||
					_blockingGroupCount + 1 >= RecordGroupingHashTable.GetRecordBlockingFactor(recordTypeOfPreviousGroup))
				{
					EndOfBlock(this, GetEventArgs());
					_blockingGroupCount = 0;
				}
				else
				{
					_blockingGroupCount++;
				}
			}

			RecordTypeStack.Pop();

			if (_processingHeaderData &&
				RecordGroupingHashTable.GetXmlDepthLevel(_currentRecordType) <
				RecordGroupingHashTable.GetXmlDepthLevel(recordTypeOfPreviousGroup))
			{
				this.HeaderNoLongerApplicable(this, GetEventArgs());
			}
		}

		private string GetRecordTypeOfCurrentLine()
		{
			// data identifying the record type in this file may not be contiguous, so concatenate group captures instead of using just the match
			if (_useGroupCaptures)
			{
				string concatenatedGroupCaptures = string.Empty;
				Regex.Replace(_currentLine, _recordTypeRegularExpression, delegate(Match replaceMatch)
				{
					// Note that Group 0 is excluded below as it represents the Match of the entire expression
					for (int i = 1; i < replaceMatch.Groups.Count; i++)
					{
						for (int j = 0; j < replaceMatch.Groups[i].Captures.Count; j++)
						{
							concatenatedGroupCaptures += replaceMatch.Groups[i].Captures[j].Value;
						}
					}
					return null; // The actual replace value isn't needed, just return the concatenated group captures
				});
				return concatenatedGroupCaptures;
			}

			Match match = Regex.Match(_currentLine, _recordTypeRegularExpression);
			if (match.Success) // Use Group Captures is not needed, just return the regex match to identify the record
			{
				return match.Value;
			}

			return null;  // No match found
		}

		private RecordTextReaderEventArgs GetEventArgs()
		{
			return GetEventArgs(false);
		}

		private RecordTextReaderEventArgs GetEventArgs(bool isForRepeatingGroup)
		{
			// allow recordType to be passed in because end of group functions are probably more interested
			// in recordType being popped off the stack than new record read
			return new RecordTextReaderEventArgs(
				(isForRepeatingGroup ? -1 : _physicalFileRecordNumber),	// there is no record number for repeating group tags
				_currentLine,
				_currentRecordType,
				(isForRepeatingGroup ? RecordGroupingHashTable.GetRepeatingGroupName(_currentRecordType) : _currentGroup),
				(RecordGroupingHashTable.GetRecordBlockingFactor(_currentRecordType) > 0 ? true : false));
		}

		private bool ThisIsABlockingLevelNode(string recordType)
		{
			if (_useBlocking && RecordGroupingHashTable.GetRecordBlockingFactor(recordType) > 0)
			{
				return true;
			}

			return false;
		}

		private void CheckAndProcessHeaderOnlyFile()
		{
			if (RecordTypeStack.Count > 0 &&
				RecordGroupingHashTable.GetXmlDepthLevel(_currentRecordType) <=
				RecordGroupingHashTable.GetXmlDepthLevel(RecordTypeStack.Peek()) &&
				_processingHeaderData)
				HeaderOnlyFile(this, GetEventArgs());
		}
		#endregion Private functions
	}
}