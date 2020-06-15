using System;
using System.IO;
using System.Xml.Serialization;

using CSS.Connector.FileProcessing;
using CSS.Connector.FileProcessing.Core;

namespace CSS.Connector.FileProcessors.Vanguard.SecurityReport
{
	public class DetailRecordProcessor : IRecordProcessor
	{
		private bool _firstRecordHasBeenProcessed = false;

		FileService _fileService;
		FileService FileService
		{
			get { if (_fileService == null) _fileService = new FileService(); return _fileService; }
		}

		SecurityReport _securityReport;

		public void ProcessRecord(string recordXml)
		{
			int recordNumberWithError = 0;
			
			try
			{
				FileEventProcessors.TotalRecords++;
				_securityReport = (SecurityReport)(new XmlSerializer(typeof(SecurityReport))).Deserialize(new StringReader(recordXml));
				recordNumberWithError = ((Bank)_securityReport.BlockingRecord).PhysicalFileRecordNumber;
				if (!_firstRecordHasBeenProcessed) ProcessFirstRecord();
				BankLoan bankLoan = ConversionHelper.ConvertToServiceFabricBankLoan(_securityReport);
				new MovementServiceHelper().PostBankLoan(bankLoan);
			}

			catch (Exception e)
			{
				FileEventProcessors.ErrorRecords++;
				(new LoggingHelper()).Log("ProcessRecord",
					string.Format("Error processing file {0}, record {1}, {2}",
					FileEventProcessors.FileName,
					recordNumberWithError,
					e.ToString()
					), FileEventProcessors.InstanceId, 1, true);
			}
		}

		private void ProcessFirstRecord()
		{
			_firstRecordHasBeenProcessed = true;
			FileEventProcessors.DetailRecordFound = true;
			FileEventProcessors.FileTimestamp = _securityReport.DateTimeStampFormatted;
			int fileNumber = 0;
			int.TryParse(_securityReport.FileNumber, out fileNumber);
			FileEventProcessors.FileNumber = fileNumber;
		}
	}
}
