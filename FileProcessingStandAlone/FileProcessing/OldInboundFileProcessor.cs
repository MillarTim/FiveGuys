using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSS.Cloud.Framework;
using CSS.Connector.FileProcessing.Core;
using CSS.Connector.FileProcessing.Models;

namespace CSS.Connector.FileProcessing
{
    /// <summary>
    /// This class is responsible for processing files detected by the InboundFileWatcherService.  The
    /// class and assembly name for .NET code to call is determined from the FileDefinitions table.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal class OldInboundFileProcessor
    {
        #region member variables

        public static object _moveFileLock = new object();

		public static object _queueLock = new object();

		// If file move to processed folder fails, store move info here
		public static System.Collections.Concurrent.ConcurrentQueue<FileMoveQueueItem> _fileMoveQueueItems = new System.Collections.Concurrent.ConcurrentQueue<FileMoveQueueItem>();

		private int WaitTime { get; set; }

		internal static string MonitoringFolder { get; set; }
		internal static string ProcessingFolder { get; set; }
		internal static string ProcessedFolder { get; set; }

		//private InboundFileProcessor() { }

		/*public InboundFileProcessor(int waitTime)
		{
			WaitTime = waitTime;
		}*/

		private LoggingHelper _loggingHelper;
		private LoggingHelper LoggingHelper
		{
			get
			{
				if (_loggingHelper == null) _loggingHelper = new LoggingHelper();
				return _loggingHelper;
			}
		}
		#endregion

		/// <summary>
		/// Move a file to the In-Process folder and call Processor to process it there
		/// </summary>
		/// <param name="filePathAndName">Full file name</param>
		/// <param name="fileName">Just File portion of file name</param>
		internal void MoveAndProcessFile(string filePathAndName, string fileName)
		{
			string newFullFileName = MoveFile(fileName, MonitoringFolder, ProcessingFolder, false);
			ProcessFile(newFullFileName, fileName);
		}

		/// <summary>
		/// Call to COM or .NET code to process the file.
		/// </summary>
		/// <param name="filePathAndName">to pass along to called code</param>
		/// <param name="fileName">file name without path, returned in ProcessFileReturn</param>
		/// <returns>a structure containing:
		/// - FileName being processed (for async callback func to know which file results to log)
		/// - WasSuccessful to indicate if file processing was error free
		/// - Status contains text of success or failure of processing</returns>
		internal protected void ProcessFile(string filePathAndName, string fileName)
		{
			FileDefinition fileDefinition = null;
			if (!IsFileEligibleToBeProcessed(fileName, out fileDefinition)) return; // nothing to do since file is not defined

			FileCompletionInfo fileCompletionInfo /*TODO: REMOVE THIS ==>*/ = null;
			FileService FileService = new FileService();
            FileInstance fileInstance = new FileInstance();
            fileInstance.FileId = fileDefinition.Id;
            fileInstance.BeginTime = DateTime.Now.ToLocalTime();
            fileInstance = FileService.SaveFileInstance(fileInstance);
			LoggingHelper.Log("In IFP::ProcessFile for " + filePathAndName + ", thread id " + Thread.CurrentThread.ManagedThreadId, fileInstance.InstanceId, 2);

			if (fileDefinition.UseHashCodeDuplicateDetection)
            {
                fileInstance.HashCode = FileService.CreateFileHash(filePathAndName);

				if (FileService.IsDuplicateRun(fileDefinition.Id, fileInstance.HashCode))
				{
					fileInstance.Message = string.Format("Duplicate run detected for file name {0}, file id {1} and hash code {2}.  File was not processed.", fileName, fileDefinition.Id, fileInstance.HashCode);
					fileInstance = FileService.SaveFileInstance(fileInstance);
					LoggingHelper.Log(fileInstance.Message, fileInstance.InstanceId, 1);
					return;
				}
			}

			// If column UseFileParsing in table FileDefinitions for this file type is false and column Processor contains a class name,
			//   call the ProcessFile method of that class to process the file.  The ProcessFile method must return type FileCompletionInfo
			//   and handle all errors, returning fatal errors in the FileCompletionInfo object.
			if (!fileDefinition.UseFileParsing && fileDefinition.Processor.Contains(','))
			{
				// TODO: Combine fileProcReturn (FileInstance) with FileCompletionInfo & clean up logic below
				/*
				IFileProcessor processor = (IFileProcessor)Activator.CreateInstance(Type.GetType(fileDefinition.Processor, true, true));
				var fileProcReturn = processor.ProcessFile(filePathAndName, fileInstance);
				*/
			}
			// Otherwise, the file will be parsed (see Parsing.Parser & Parsing.Processor namespaces) and processed based on
			//   other configuration including the Parsing.Parser.FileProcessorConfig XML data stored in the FileProcessorConfig table
			//   in the row corresponding to the processor specified in FileDefinitions table for a given file type
			// All errors will be handled in the RecordBlockingFileFileProcessor method and returned in the ProcessFileReturnType
			else
			{
				string fileProcessorConfigXml = FileService.GetFileProcessorConfig(fileDefinition.Processor);
				fileCompletionInfo = (new Parsing.Processor.RecordBlockingFileProcessor(fileProcessorConfigXml)).ProcessFile(filePathAndName, fileInstance.InstanceId);
				fileInstance.Successful     = !fileCompletionInfo.FatalErrorOccurred;
				fileInstance.Message        =  fileCompletionInfo.FatalErrorMessage;
				fileInstance.FileDate       =  fileCompletionInfo.FileTimestamp;
				fileInstance.SequenceNumber =  fileCompletionInfo.FileNumber;
			}

			fileInstance.EndTime = DateTime.Now.ToLocalTime();
           
			// only move file to processed location if processing was successful & no error was thrown
			if (fileInstance.Successful)
			{
				string currentFolder = Path.GetDirectoryName(filePathAndName);
				LoggingHelper.Log(string.Format(CultureInfo.InvariantCulture, "In IFP:ProcessFile, moving file {0} from {1} to {2}. ", fileName, currentFolder, ProcessedFolder), fileInstance.InstanceId, 2);
				MoveFile(fileName, currentFolder, ProcessedFolder, true);
			}

			LoggingHelper.Log("In IFP::ProcessFile file processing complete" + filePathAndName, fileInstance.InstanceId, 2);

			FileService.SaveFileInstance(fileInstance);
		}

		// Determine if file type can be determined based on its name compared to FileDefinitions rows
		private bool IsFileEligibleToBeProcessed(string fileName, out FileDefinition fileDefinition)
		{
			var fileDefinitions = (new FileService()).FindFilesByFileName(fileName);
			if (fileDefinitions.Count != 1)
			{
				LoggingHelper.Log("WillBeDeleted", string.Format("In IFP::IsFileEligibleToBeProcessed, found {0} matching file types in table FileDefinitions for {1}. Now row found where RegexNameExpression matched the file name. The File will not be processed.", fileDefinitions.Count.ToString(), fileName), 1);
				fileDefinition = null;
				return false;
			}
			fileDefinition = fileDefinitions[0];
			return true; 
		}

		private string MoveFile(string fileName, string fromFolder, string toFolder, bool isFinalDestination, int retryCount = 0)
		{
			lock (_moveFileLock)         // to keep multiple processes from fighting over file names
			{
				string fullToFolder = toFolder;
				// Instead of moving file to just the Processed folder, put it in a sub folder with date
				if (isFinalDestination)
				{
					fullToFolder = Path.Join(fullToFolder, DateTime.Now.ToString("yyyy-MM-dd"));
					if (!Directory.Exists(fullToFolder)) Directory.CreateDirectory(fullToFolder);
				}

				string fromFileName = Path.Join(fromFolder, fileName);
				string toFileName = Path.Join(fullToFolder, fileName);

				// if file already exists in 'move to' location, rename that file to end with _# so file just processed can be moved.
				if (File.Exists(toFileName))
				{
					string fileSuffix = ""; int fileSuffixNumber = 0;
					do
					{
						if (fileSuffixNumber > 10)
						{
							string error = string.Format("File not moved. Tried {0} through {1}", fileName, fileName + fileSuffix);
							LoggingHelper.Log("WillBeDeleted", error, 1);
							throw new Exception(error);
						}
						fileSuffix = "_" + (++fileSuffixNumber).ToString(CultureInfo.InvariantCulture);
					} while (File.Exists(toFileName + fileSuffix));
					toFileName += fileSuffix;

				}
				// move file to processed folder.  Since this has been problematic occasionally, put in try, catch to pinpoint problems
				try
				{
					// Getting a lot of these errors when multiple files arrive at the same time:
					//   System.IO.IOException: An unexpected network error occurred
					File.Move(fromFileName, toFileName);
				}
				catch (Exception e)
				{
					LoggingHelper.Log("WillBeDeleted", string.Format("Error moving file from {0} to {1}.  Error: {2}", fromFileName, toFileName, e.ToString()), 1);
					/*
					if (!isFinalDestination) return fromFileName;    // If move fails & file hasn't been processed yet, just process it from FTP location
																	 // Then, once file has been processed, it will be moved directly to processed folder

					// If moving to the final destination (processed folder) fails add to memory queue to be tried later
					FileMoveQueueItem dummyItem = null;
					bool queueWasEmpty;
					lock (_queueLock)
					{
						queueWasEmpty = !_fileMoveQueueItems.TryPeek(out dummyItem);
						_fileMoveQueueItems.Enqueue(new FileMoveQueueItem(fromFileName, toFileName, ++retryCount));
						if (queueWasEmpty) Task.Run(() => (new InboundFileProcessor(WaitTime)).RetryFileMove());
					}
					*/
				}

				return toFileName;
			}
		}

		private void RetryFileMove()
		{
			try
			{
				FileMoveQueueItem queueItem = null;
				int loopCount = 0;
				while (loopCount++ < 100)
				{
					lock (_queueLock)
					{
						if (!_fileMoveQueueItems.TryDequeue(out queueItem)) return;  //Nothing to do since queue is empty
					}
					int waitTime = queueItem.RetryCount * WaitTime;
					Thread.Sleep(waitTime);
					string fileName = Path.GetFileName(queueItem.FromFile);
					string fromFolder = Path.GetDirectoryName(queueItem.FromFile);
					LoggingHelper.Log("WillBeDeleted", string.Format("In RetryFileMove.  Moving {0} from {1} to {2}. Try {3}. Wait {4}.", fileName, fromFolder, ProcessedFolder, queueItem.RetryCount, waitTime), 2);
					this.MoveFile(
						Path.GetFileName(queueItem.FromFile),
						Path.GetDirectoryName(queueItem.FromFile),
						ProcessedFolder,
						true,
						queueItem.RetryCount);
				}
			}
			catch (Exception e)
			{
				LoggingHelper.Log("WillBeDeleted", string.Format("Error in RetryFileMove.  Error: {0}", e.ToString()), 1);
			}
		}
	}
}