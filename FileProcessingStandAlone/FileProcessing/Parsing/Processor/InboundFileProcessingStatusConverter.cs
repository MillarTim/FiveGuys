// TODO: Remove this class
/*
using System;
using CSS.Framework.DataAccess.DataMapping;

namespace CSS.Connector.FileProcessing.Parsing.Processor
{
    /// <summary>
    /// Converts strings to InboundFileProcessingStatus objects and single letter codes
    /// </summary>
    public class InboundFileProcessingStatusConverter : Converter
    {
        /// <summary>
        /// Converts a string representation of an InboundFileProcessingStatus enumeration value
        /// to a single letter code
        /// </summary>
        /// <param name="input">A string containing the text representation of a InboundFileProcessingStatus member value</param>
        /// <returns>
        /// A single letter code:
        /// <list type="bulleted">
        /// <item>Complete = C</item>
        /// <item>Failed = F</item>
        /// <item>InProgress = P</item>
        /// <item>Received = R</item>
        /// <item>Otherwise NULL</item>
        /// </list>
        /// </returns>
        public override object ToData(object input)
        {
            InboundFileProcessingStatus type = new InboundFileProcessingStatus();
            string status = input as string;
            if (status == null)
            {
                type = InboundFileProcessingStatus.Unknown;
            }
            else
            {
                type = (InboundFileProcessingStatus)Enum.Parse(type.GetType(), status);
            }

            switch (type)
            {
                case InboundFileProcessingStatus.Complete:
                    return "C";
                case InboundFileProcessingStatus.Failed:
                    return "F";
                case InboundFileProcessingStatus.InProgress:
                    return "P";
                case InboundFileProcessingStatus.Received:
                    return "R";
                default:
                    return null;

            }
        }

        /// <summary>
        /// Converts the input string to an instance of the InboundFileProcessingStatus class.
        /// </summary>
        /// <param name="input">String containing the text representation of the enumeration value (e.g. Received).</param>
        /// <returns>An instance of the InboundFileProcessingStatus class.</returns>
        /// <remarks>
        /// This method is not implemented yet.
        /// </remarks>
        public override object ToObject(object input)
        {
            throw new NotImplementedException();
        }
    }
}
*/