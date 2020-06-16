using System.Collections.ObjectModel;
using System.Xml;

namespace CSS.Connector.FileProcessing.Parsing.Parser
{
    // If data blocks are being sent to delegate, a collection of this class (one element for each logical header record) will be used to hold
    //   data fragments for the header information.  These fragments could be xml fragments containing the field values from the header records.
    internal class HeaderData
    {
        public string RecordGroupingName;
        public int PhysicalFileRecordNumber;
        public string DataFragment;
        private HeaderData()
        {
        }
        public HeaderData(string recordGroupingName, int physicalFileRecordNumber, string dataFragment)
        {
            this.RecordGroupingName = recordGroupingName;
            this.PhysicalFileRecordNumber = physicalFileRecordNumber;
            this.DataFragment = dataFragment;
        }
    }

    internal class HeaderCollection : Collection<HeaderData>
    {
        public HeaderCollection()
        {
        }

        public void WriteHeaderXml(XmlDataWriter writer)
        {
            for (int i = 0; i < this.Count; i++)
            {
                writer.WriteGroupStart(this[i].RecordGroupingName, this[i].PhysicalFileRecordNumber, false);
                writer.WriteNode(new XmlTextReader(this[i].DataFragment, XmlNodeType.Element, null));
                //writer.WriteNode(new XmlTextReader(this[i].DataFragment, XmlNodeType.Element, context));
            }
        }

        public void Add(string recordGroupingName, int physicalFileRecordNumber, string dataFragment)
        {
            this.Add(new HeaderData(/*NameConvert.ToCamelCase(*/recordGroupingName/*)*/, physicalFileRecordNumber, dataFragment));
        }

        public void UpdateLastDataFragment(string dataFragment)
        {
            this[this.Count - 1].DataFragment = dataFragment;
        }

        public void RemoveLastHeader()
        {
            if (this.Count > 0) this.RemoveAt(this.Count - 1);
        }
    }
}