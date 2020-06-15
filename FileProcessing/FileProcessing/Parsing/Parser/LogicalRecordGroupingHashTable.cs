using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;

namespace CSS.Connector.FileProcessing.Parsing.Parser
{
	class LogicalRecordGroupingHashTable : Hashtable
	{
		private LogicalRecordGroupingHashTable()
		{
		}
		#region Public Constructor
		// seems like intential violation of this rule so suppress errorMessage was added
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public LogicalRecordGroupingHashTable(string resourceName, Assembly assembly)
		{
			LoadFromResource(resourceName, assembly);
		}
		#endregion

		#region Private variables and properties
		// Key to hashtable is record type.  Data will contain xml depth level and record group name.
		// From this, it can be determined when to write record grouping starting and ending tags
		internal struct RecordGroupingEntry
		{
			public int XmlDepthLevel;          // nested logical depth of physical record
			public String RecordGroupName;     // will be used as xml tag around grouped records
			public int RecordBlockingFactor;   // If delegate is used, max records to send to delegate at once
			public bool IsRepeatingGroup;	   // Group tags will be used when group is marked as repeating (i.e. blocks/block)
			public string RepeatingGroupName;  // Group tag name (plural of repeating element name)
			//public bool ContextIsRequired;	   // Record type cannot be determined from regex alone.  Must also consider which logical record group is being processed.

			public RecordGroupingEntry(int xmlDepthLevel, string recordGroupName, int recordBlockingFactor, bool isRepeatingGroup /*, bool contextIsRequired*/)
			{
				this.XmlDepthLevel = xmlDepthLevel;
				this.RecordGroupName = recordGroupName;
				this.RecordBlockingFactor = recordBlockingFactor;
				this.IsRepeatingGroup = isRepeatingGroup;
				this.RepeatingGroupName = (IsRepeatingGroup ? NameConvert.ToPluralForm(recordGroupName) : string.Empty);
				//				this.ContextIsRequired = contextIsRequired;
			}
		}

		private Hashtable _aliasMapping;
		private Hashtable AliasMapping
		{
			get
			{
				if (_aliasMapping == null) _aliasMapping = new Hashtable();
				return _aliasMapping;
			}
			//			set {_aliasMapping = value;}
		}
		#endregion

		#region Public properties with their variables
		private string _rootNodeName;
		public string RootNodeName
		{
			get { return _rootNodeName; }
			private set { _rootNodeName = value; }
		}

		private bool _ContainsBlocking;
		public bool ContainsBlocking
		{
			get { return _ContainsBlocking; }
			private set { _ContainsBlocking = value; }
		}

		private bool _useGroupCaptures;
		public bool UseGroupCaptures
		{
			get { return _useGroupCaptures; }
			private set { _useGroupCaptures = value; }
		}

		#endregion
		#region Private Functions
		private void LoadFromResource(string resourceName, Assembly assembly)
		{
			using (XmlTextReader reader = new XmlTextReader(OpenFile(resourceName, assembly)))
			{
				while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.Element)
					{
						if (reader.Depth == 0)
						{
							RootNodeName = reader.Name;
							UseGroupCaptures = Convert.ToBoolean(reader.GetAttribute("useGroupCaptures"), CultureInfo.InvariantCulture);
						}
						else
						{
							string recordGroupName = reader.GetAttribute("recordGroupName");
							int blockingFactor = Convert.ToInt16(reader.GetAttribute("recordBlockingFactor"), CultureInfo.InvariantCulture);
							bool isRepeatingGroup = Convert.ToBoolean(reader.GetAttribute("isRepeatingGroup"), CultureInfo.InvariantCulture);
							string alias = reader.GetAttribute("alias");
							string recordTypeContext = reader.GetAttribute("context");

							this.Add(
								reader.Name,
								new RecordGroupingEntry(
									reader.Depth,
									recordGroupName,
									blockingFactor,
									isRepeatingGroup));
							AddAliasMapping(alias, recordTypeContext, reader.Name);
							if (blockingFactor > 0) ContainsBlocking = true;
						}
					}
				}
			}
		}

		// for backwards compatibility alias & recordTypeContext may both be present.
		// if recordTypeContext is present it will also contain the alias, so the alias will be ignored
		private void AddAliasMapping(string aliasString, string recordTypeContextString, string recordType)
		{
			string alias = null;
			string recordTypeContext = null;
			bool useContext = false;
			if (recordTypeContextString != null && recordTypeContextString.Length > 0 && recordTypeContextString.Contains("~"))
			{
				int tildePosition = recordTypeContextString.IndexOf("~", StringComparison.Ordinal);
				recordTypeContext = recordTypeContextString.Substring(0, tildePosition);
				if (tildePosition + 1 < recordTypeContextString.Length)
				{
					alias = recordTypeContextString.Substring(tildePosition + 1);
				}
				useContext = true;
			}
			else
			{
				alias = aliasString;
			}
			if (alias != null && alias.Length > 0)
			{
				string[] aliases = alias.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string a in aliases)
				{
					AliasMapping.Add((useContext ? recordTypeContext + "~" + a : a), recordType);
				}
			}
		}

		public string GetRecordTypeFromAlias(string alias, string[] stack)
		{
			string recordType = null;

			// If an alias was used, get the record type from it,
			if (AliasMapping.ContainsKey(alias))
			{
				recordType = (string)AliasMapping[alias];
			}

			if (recordType == null)
			{
				// If this is a record type where we need the context of the current record group name, look that up in the dictionary
				recordType = LookForRecordTypeInContext(alias, stack);
			}

			if (recordType == null)
			{
				if (this.ContainsKey(alias))
				{
					// otherwise just return the value passed in (which will be the record type).
					recordType = alias;
				}
			}

			return (recordType);
		}

		// Unfortunately, since we're still trying to find the current record type, we may have to look through the stack
		//   for the context belonging to this record type
		private string LookForRecordTypeInContext(string alias, string[] stack)
		{
			foreach (string stackItem in stack)
			{
				string aliasInContext = stackItem + "~" + alias;
				if (AliasMapping.Contains(aliasInContext)) return (string)AliasMapping[aliasInContext];
			}
			return null;
		}

		private static StreamReader OpenFile(string resourceName, Assembly assembly)
		{
			try
			{
				Stream stream = assembly.GetManifestResourceStream(resourceName);
				return new StreamReader(stream);
			}
			catch (ArgumentNullException)
			{
				throw new FileLoadException(Errors.ResourceNotFound + resourceName);
			}
		}
		#endregion

		#region Public functions
		public int GetXmlDepthLevel(string TableKey)
		{
			return ((RecordGroupingEntry)this[TableKey]).XmlDepthLevel;
		}

		public string GetRecordGroupName(string TableKey)
		{
			return ((RecordGroupingEntry)this[TableKey]).RecordGroupName;
		}

		public int GetRecordBlockingFactor(string TableKey)
		{
			return ((RecordGroupingEntry)this[TableKey]).RecordBlockingFactor;
		}

		public bool IsRepeatingGroup(string TableKey)
		{
			return ((RecordGroupingEntry)this[TableKey]).IsRepeatingGroup;
		}

		public string GetRepeatingGroupName(string TableKey)
		{
			return ((RecordGroupingEntry)this[TableKey]).RepeatingGroupName;
		}
		#endregion
	}
}