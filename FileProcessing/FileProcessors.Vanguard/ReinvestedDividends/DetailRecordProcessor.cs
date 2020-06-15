using System;
using System.IO;
using System.Xml.Serialization;

using CSS.Connector.FileProcessing;
using CSS.Connector.FileProcessing.Core;

namespace CSS.Connector.FileProcessors.Vanguard.ReinvestedDividends
{
	public class DetailRecordProcessor : IRecordProcessor
	{
		private bool _firstRecordHasBeenProcessed = false;

		FileService _fileService;
		FileService FileService
		{
			get { if (_fileService == null) _fileService = new FileService(); return _fileService; }
		}

		ReinvestedDividends _reinvestedDividends;

		public void ProcessRecord(string recordXml)
		{
			// The detail records (File Identifier = 05) in this file are sorted by security number.
			//   Sometimes there are multiple rows for the same security.  So instead of posting the values
			//   for each detail (05) record, store the values and post when the Security Number changes or
			//   end of file it reached.  Amounts for the same Security Number and date will be accumulated.
			Detail detail = null;
			try
			{
				FileEventProcessors.TotalRecords++;
				_reinvestedDividends = (ReinvestedDividends)(new XmlSerializer(typeof(ReinvestedDividends))).Deserialize(new StringReader(recordXml));
				if (!_firstRecordHasBeenProcessed) ProcessFirstRecord();
				detail = (Detail)_reinvestedDividends.BlockingRecord;
				bool isPayableDateUnknown = detail.PayableDate == "99/99/99";
				if (isPayableDateUnknown)
				{
					detail.PayableDate = DateTime.Now.ToString("MM/dd/yy");
				}
				FileEventProcessors.ProcessDetailData(
					detail.SecurityNumber,
					detail.PayableDate,
					detail.PayableDateFormatted,
					detail.AggregatedReinvestedDividendAmountFormatted,
					isPayableDateUnknown);
			}

			catch (Exception e)
			{
				FileEventProcessors.ErrorRecords++;
				(new LoggingHelper()).Log("ProcessRecord",
					string.Format("Error processing file {0}, record {1}, {2}",
					FileEventProcessors.FileName,
					detail?.PhysicalFileRecordNumber,
					e.ToString()
					), FileEventProcessors.InstanceId, 1, true);
			}
		}

		private void ProcessFirstRecord()
		{
			_firstRecordHasBeenProcessed = true;
			FileEventProcessors.DetailRecordFound = true;
			FileEventProcessors.FileTimestamp = _reinvestedDividends.DateTimeStampFormatted;
			int fileNumber = 0;
			int.TryParse(_reinvestedDividends.FileNumber, out fileNumber);
			FileEventProcessors.FileNumber = fileNumber;
		}
	}
}
