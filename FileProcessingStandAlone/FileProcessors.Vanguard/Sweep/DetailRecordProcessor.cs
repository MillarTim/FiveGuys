using System;
using System.IO;
using System.Xml.Serialization;

using CSS.Connector.FileProcessing;
using CSS.Connector.FileProcessing.Core;

namespace CSS.Connector.FileProcessors.Vanguard.Sweep
{
	public class DetailRecordProcessor : IRecordProcessor
	{
		private bool _firstRecordHasBeenProcessed = false;

		FileService _fileService;
		FileService FileService
		{
			get { if (_fileService == null) _fileService = new FileService(); return _fileService; }
		}

		Sweep _sweep;

		public void ProcessRecord(string recordXml)
		{
			try
			{
				FileEventProcessors.TotalRecords++;
				_sweep = (Sweep)(new XmlSerializer(typeof(BlockingRecord))).Deserialize(new StringReader(recordXml));
				if (!_firstRecordHasBeenProcessed) ProcessFirstRecord();
				decimal amount = 0.0M;
				foreach (var detail in _sweep.Details)
				{
					amount += detail.TotalWireAmountForSellsFormatted - detail.TotalWireAmountForBuysFormatted;
				}

				new MovementServiceHelper().PostExpectedBalance(
					amount,
					string.Format("{0}, {1} File Input", _sweep.FileIdentifier, _sweep.FileNumber),
					FileService.GetTypeMapping(_sweep.FileIdentifier, "TRT"),
					FileService.GetTypeMapping(_sweep.FileIdentifier, "ACC")
					);
			}

			catch (Exception e)
			{
				FileEventProcessors.ErrorRecords++;
				(new LoggingHelper()).Log("ProcessRecord",
					string.Format("Error processing file {0}, record {1}, {2}",
					FileEventProcessors.FileName,
					_sweep?.PhysicalFileRecordNumber,
					e.ToString()
					), FileEventProcessors.InstanceId, 1, true);
			}
		}

		private void ProcessFirstRecord()
		{
			_firstRecordHasBeenProcessed = true;
			FileEventProcessors.DetailRecordFound = true;
			FileEventProcessors.FileTimestamp = _sweep.DateTimeStampFormatted;
			int fileNumber = 0;
			int.TryParse(_sweep.FileNumber, out fileNumber);
			FileEventProcessors.FileNumber = fileNumber;
		}
	}
}
