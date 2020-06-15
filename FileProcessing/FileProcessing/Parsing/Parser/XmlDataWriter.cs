using System.Text;
using System.Xml;

namespace CSS.Connector.FileProcessing.Parsing.Parser
{
    internal class XmlDataWriter
    {
        // The following StringBuilder is used to store xml from data records to be used for each block of data sent to delegate of caller.
        //   The StringBuilder is only used when ConvertToXml with one parameter is called.  The DataXmlWriter will be passed in from
        //   the caller if the file is processed in one pass, or will be re-created for each block of data if the delegate is used.
        private StringBuilder _xmlString;
        private StringBuilder xmlString
        {
            get
            {
                if (_xmlString == null) _xmlString = new StringBuilder();
                return _xmlString;
            }
        }

        private XmlWriter _XmlWriter;
        private XmlWriter XmlWriter
        {
            get
            {
                if (_XmlWriter == null || _XmlWriter.WriteState == WriteState.Closed)
                {
                    //					xmlString.Length = 0;
                    _XmlWriter = XmlTextWriter.Create(xmlString, GetWriterSettings());
                }
                return _XmlWriter;
            }
            set { _XmlWriter = value; }
        }

        private const string RecordNumberAttributeName = "PhysicalFileRecordNumber";
        private const string XsiPrefix = "xsi";
        private const string XsiNs = "http://www.w3.org/2001/XMLSchema-instance";
        private const string BlockingRecordElement = "blockingRecord";
        private const string TypeAttribute = "type";
        private string _xmlNamespace;

        private XmlDataWriter()
        {
        }

        public XmlDataWriter(string xmlNamespace)
        {
            _xmlNamespace = xmlNamespace;
        }

        public XmlDataWriter(XmlWriter xmlWriter, string xmlNamespace)
        {
            _xmlNamespace = xmlNamespace;
            XmlWriter = xmlWriter;
        }

        static private XmlWriterSettings GetWriterSettings()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.Encoding = Encoding.UTF8;
            settings.OmitXmlDeclaration = true;
            return settings;
        }

        public bool IsThereXmlData()
        {
            return (this.XmlWriter.WriteState != WriteState.Start && this.XmlWriter.WriteState != WriteState.Closed);
        }

        public string GetXmlData()
        {
            this.XmlWriter.Close();
            string xmlData = this.xmlString.ToString();
            this.xmlString.Length = 0;
            return xmlData;
        }

        public void WriteGroupStart(string groupName)
        {
            this.WriteGroupStart(groupName, 0, false);
        }

        public void WriteGroupStart(string groupName, int recordNumber, bool isBlockingRecord)
        {
            string elementName = (isBlockingRecord ? NameConvert.ToPascalCase(BlockingRecordElement) : /*NameConvert.ToCamelCase(*/groupName/*)*/);
            this.XmlWriter.WriteStartElement(elementName, _xmlNamespace);
            if (isBlockingRecord)
            {
                this.XmlWriter.WriteAttributeString(XsiPrefix, TypeAttribute, XsiNs, NameConvert.ToPascalCase(groupName));
            }
            if (recordNumber >= 0)
            {
                this.XmlWriter.WriteAttributeString(RecordNumberAttributeName, recordNumber.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
        }

        public void WriteGroupEnd()
        {
            this.XmlWriter.WriteEndElement();
        }

        public void WriteNameValuePair(string name, string value)
        {
            this.XmlWriter.WriteElementString(NameConvert.ToPascalCase(name), _xmlNamespace, value);
        }

        public void WriteValue(string value)
        {
            this.XmlWriter.WriteString(value);
        }

        public void WriteNode(XmlReader reader)
        {
            this.XmlWriter.WriteNode(reader, true);
        }
    }
}