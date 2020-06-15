using System;
using System.IO;
using System.Xml.Serialization;

using CSS.Connector.FileProcessing;
using CSS.Connector.FileProcessing.Core;

namespace CSS.Connector.FileProcessors.Vanguard.BetaReport1
{
	public class DetailRecordProcessor : IRecordProcessor
	{
		private bool _firstRecordHasBeenProcessed = false;

		FileService _fileService;
		FileService FileService
		{
			get { if (_fileService == null) _fileService = new FileService(); return _fileService; }
		}

		BetaReport1 _betaReport1;

		public void ProcessRecord(string recordXml)
		{
			Detail detail = null;
			try
			{
				FileEventProcessors.TotalRecords++;
				_betaReport1 = (BetaReport1)(new XmlSerializer(typeof(BetaReport1))).Deserialize(new StringReader(recordXml));
				if (!_firstRecordHasBeenProcessed) ProcessFirstRecord();
				detail = (Detail)_betaReport1.BlockingRecord;
				MovementServiceHelper movementHelper = new MovementServiceHelper();
				DateTime postingDate = movementHelper.GetNextSettlementDate(detail.ProcessDateFormatted);
				movementHelper.PostBetaReport1Totals(
					postingDate,
					detail.LongMarketValueFormatted,
					detail.ShortMarketValueFormatted);
			}

			catch(Exception e)
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
			FileEventProcessors.FileTimestamp = _betaReport1.DateTimeStampFormatted;
			int fileNumber = 0;
			int.TryParse(_betaReport1.FileNumber, out fileNumber);
			FileEventProcessors.FileNumber = fileNumber;
		}
	}
}
