using System;
using System.IO;
using System.Xml.Serialization;

using CSS.Connector.FileProcessing;
using CSS.Connector.FileProcessing.Core;

namespace CSS.Connector.FileProcessors.Vanguard.BetaReport2
{
	public class DetailRecordProcessor : IRecordProcessor
	{
		private bool _firstRecordHasBeenProcessed = false;

		FileService _fileService;
		FileService FileService
		{
			get { if (_fileService == null) _fileService = new FileService(); return _fileService; }
		}

		BetaReport2 _betaReport2;

		public void ProcessRecord(string recordXml)
		{
			Detail detail = null;
			try
			{
				FileEventProcessors.TotalRecords++;
				_betaReport2 = (BetaReport2)(new XmlSerializer(typeof(BetaReport2))).Deserialize(new StringReader(recordXml));
				if (!_firstRecordHasBeenProcessed) ProcessFirstRecord();
				detail = (Detail)_betaReport2.BlockingRecord;
				MovementServiceHelper movementHelper = new MovementServiceHelper();
				DateTime postingDate = movementHelper.GetNextSettlementDate(detail.DateFormatted);
				movementHelper.PostBetaReport2Totals(
					postingDate,
					detail.FailsToDeliverFormatted,
					detail.StockBorrowFormatted,
					detail.FailsToReceiveFormatted,
					detail.StockLoanFormatted);
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
			FileEventProcessors.FileTimestamp = _betaReport2.DateTimeStampFormatted;
			int fileNumber = 0;
			int.TryParse(_betaReport2.FileNumber, out fileNumber);
			FileEventProcessors.FileNumber = fileNumber;
		}
	}
}
