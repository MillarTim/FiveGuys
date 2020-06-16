using System;
using System.IO;
using System.Xml.Serialization;

using CSS.Connector.FileProcessing;
using CSS.Connector.FileProcessing.Core;

namespace CSS.Connector.FileProcessors.Vanguard.CnsProjectionMidday
{
	public class DetailRecordProcessor : IRecordProcessor
	{
		private bool _firstRecordHasBeenProcessed = false;

		FileService _fileService;
		FileService FileService
		{
			get { if (_fileService == null) _fileService = new FileService(); return _fileService; }
		}

		CnsProjectionMidday _cnsProjectionMidday;

		public void ProcessRecord(string recordXml)
		{
			Detail detail = null;
			try
			{
				FileEventProcessors.TotalRecords++;
				_cnsProjectionMidday = (CnsProjectionMidday)(new XmlSerializer(typeof(CnsProjectionMidday))).Deserialize(new StringReader(recordXml));
				if (!_firstRecordHasBeenProcessed) ProcessFirstRecord();
				detail = (Detail)_cnsProjectionMidday.CnsProjection.BlockingRecord;

				// For now, only accumulate the Dollar total to be written at end of this file.  See FileEventProcessors::EndOfFileEventProcessor
				FileEventProcessors.DollarTotal -= detail.TomorrowsProjectedMarketValueFormatted;
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
			FileEventProcessors.FileTimestamp = _cnsProjectionMidday.ApplicationDateFormatted;
			int fileNumber = 0;
			int.TryParse(_cnsProjectionMidday.ApplicationMultiCycleCounter, out fileNumber);
			FileEventProcessors.FileNumber = fileNumber;
			FileEventProcessors.SettlementDate = _cnsProjectionMidday.CnsProjection.SettlementDateFormatted;
		}
	}
}