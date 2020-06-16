using System;
using System.IO;
using System.Xml.Serialization;

using CSS.Connector.FileProcessing;
using CSS.Connector.FileProcessing.Core;

namespace CSS.Connector.FileProcessors.Vanguard.SettlementBalances
{
	public class DetailRecordProcessor : IRecordProcessor
	{
		private bool _firstRecordHasBeenProcessed = false;

		FileService _fileService;
		FileService FileService
		{
			get { if (_fileService == null) _fileService = new FileService(); return _fileService; }
		}

		SettlementBalance _settlementBalance;

		public void ProcessRecord(string recordXml)
		{
			Detail detail = null;
			try
			{
				FileEventProcessors.TotalRecords++;
				_settlementBalance = (SettlementBalance)(new XmlSerializer(typeof(SettlementBalance))).Deserialize(new StringReader(recordXml));
				if (!_firstRecordHasBeenProcessed) ProcessFirstRecord();
				detail = (Detail)_settlementBalance.BlockingRecord;

				// Looking for only the last Timestamp entry in this file for either NSCC or DTCC account total rows.
				if (detail.MroBalanceTypeOrderNum != "006" && detail.MroBalanceTypeOrderNum != "010") return;
				string balanceSource = (detail.MroBalanceTypeOrderNum == "006" ? "DTC" : "NSC");
				decimal multiplier = (detail.MroNetDbCrInd == "D" ? -1.0M : 1.0M);
				FileEventProcessors.SettlementBalanceDictionary.Add(detail.MroParticipantId, detail.MroActivityCode, detail.MroSubActivityCode, balanceSource, detail.MroTimestampFormatted, detail.MroNetBalanceFormatted * multiplier);
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
			FileEventProcessors.FileTimestamp = _settlementBalance.DateTimeCreatedFormatted;
			int fileNumber = 0;
			int.TryParse(_settlementBalance.ApplicationMultiCycleCounter, out fileNumber);
			FileEventProcessors.FileNumber = fileNumber;
		}
	}
}