using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
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
	internal static class InboundFileProcessor
	{
		#region member variables

		//        public static object _moveFileLock = new object();

		//		public static object _queueLock = new object();

		private static object _criticalLock = new object();

		private static Dictionary<string, InboundFile> InboundFiles = new Dictionary<string, InboundFile>();

		public static bool IsProcessing = false;

		public static bool IsCleanupRunning = false;

		private static bool _stopProcessing = false;

		private static bool _stopCleaningUp = false;

		// If file move to processed folder fails, store move info here
		//		public static System.Collections.Concurrent.ConcurrentQueue<FileMoveQueueItem> _fileMoveQueueItems = new System.Collections.Concurrent.ConcurrentQueue<FileMoveQueueItem>();

		internal static int WaitTime { get; set; }
		internal static string MonitoringFolder { get; set; }
		internal static string ProcessingFolder { get; set; }
		internal static string ProcessedFolder { get; set; }

		private static LoggingHelper _loggingHelper;
		private static LoggingHelper LoggingHelper
		{
			get
			{
				if (_loggingHelper == null) _loggingHelper = new LoggingHelper();
				return _loggingHelper;
			}
		}
		#endregion

		public static void StopProcessingFiles()
		{
			_stopProcessing = true;
			_stopCleaningUp = true;
			int loopCount = 0; // don't let it try forever
			// files may still be processing, so let this stop cleanly before restarting file monitoring, etc.
			while (loopCount++ < 60 && (IsProcessing || IsCleanupRunning)) Thread.Sleep(WaitTime);
			LoggingHelper.Log("IFP::StopProcessingFiles", string.Format("File processing stopped.  Loop count was {0}.", loopCount), 2);
		}

		public static void TestEligibilityAndAddToProcessingQueue(string filePathAndName, bool shouldMoveFile)
		{
			string fileName = System.IO.Path.GetFileName(filePathAndName);
			string filePath = System.IO.Path.GetDirectoryName(filePathAndName);
			FileDefinition fileDefinition = null;

			lock (_criticalLock)
			{
				if (InboundFiles.ContainsKey(fileName))                     return; // Processing already started or attempted on file; If that's true, probably another event fired due to ready-only flag being set, etc.

				if (!TestAndSetFileAttributes(filePathAndName))             return; // File attributes were not in proper state (i.e. read-only flag was already set)

				if (!IsFileDefined(fileName, filePath, out fileDefinition)) return; // Definition not found for this file in FileDefinitions table

				InboundFiles.Add(fileName, new InboundFile(false, DateTime.Now, filePath, filePathAndName, fileDefinition));
			}

			LoggingHelper.Log("IFP::TestEligibility", string.Format("File {0} added to list of files for processing from thread {1}.", fileName, Thread.CurrentThread.ManagedThreadId), 2);
		}

		/// <summary>
		/// Only process the file if it meets certain criteria, like it is not read-only
		/// </summary>
		/// <param name="filePathAndName">file (with path) to process</param>
		/// <returns>true if file is eligible</returns>
		private static bool TestAndSetFileAttributes(string filePathAndName)
		{
			int currentFileAttributes = (int)File.GetAttributes(filePathAndName);

			// bitwise compare to make sure file type is normal or archive & not a directory or a read only or a hidden file
			bool areAttributesSetToAllowProcessing = (
				0 != (int)(((int)FileAttributes.Normal | (int)FileAttributes.Archive) & currentFileAttributes) &&
				0 == (int)(((int)FileAttributes.ReadOnly | (int)FileAttributes.Hidden | (int)FileAttributes.Directory) & currentFileAttributes)
			);

			if (!areAttributesSetToAllowProcessing) return false;

			File.SetAttributes(filePathAndName, FileAttributes.ReadOnly);
			LoggingHelper.Log("IFW::TestAndSetFileAttributes", filePathAndName + " set to read-only.", 2);
			return true;
		}

		// Determine if file type can be determined based on its name compared to a FileDefinitions table row
		private static bool IsFileDefined(string fileName, string filePath, out FileDefinition fileDefinition)
		{
			var fileDefinitions = (new FileService()).FindFilesByFileName(fileName);
			if (fileDefinitions.Count != 1)
			{
				LoggingHelper.Log("IFP::IsFileDefined", string.Format("Found {0} matching file types in table FileDefinitions for {1}. No row (or multiple rows) found where RegexNameExpression matched the file name. The File will not be processed.", fileDefinitions.Count.ToString(), fileName), 1);
				fileDefinition = null;
				MoveFile(fileName, filePath, ProcessedFolder, true, true);	// already in _criticalLock from caller
				return false;
			}
			fileDefinition = fileDefinitions[0];
			return true;
		}

		/// <summary>
		/// This method will run continuously to clean up file entries from a dictionary once a certain time period has passed since the item was added.
		/// </summary>
		public static void CleanupDictionary()
		{
			if (IsCleanupRunning) return;
			IsCleanupRunning = true;
			LoggingHelper.Log("CleanupDictionary", string.Format("Dictionary Cleanup process started."), 1);
			while (true)
			{
				try
				{
					if (_stopCleaningUp)
					{
						_stopCleaningUp = IsCleanupRunning = false;
						LoggingHelper.Log("CleanupDictionary", string.Format("Dictionary Cleanup process stopped."), 1);
						return;
					}

					Thread.Sleep(WaitTime);
					lock (_criticalLock)
					{
						if (InboundFiles.Count > 0)
						{
							// Remove entries in this dictionary where the file has been processed and it's been over 15 seconds since the file has been detected.
							var filesToRemove = InboundFiles.Where(file => file.Value.ProcessComplete && (DateTime.Now - file.Value.DateTimeDetected).TotalSeconds > 15).Select(file => file.Key).ToList();
							filesToRemove.ForEach(fileName =>
							{
								InboundFiles.Remove(fileName);
								LoggingHelper.Log("CleanupDictionary", string.Format("File {0} removed from list of files being processed.", fileName), 2);
							});
						}
					}
				}
				catch (Exception e)
				{
					LoggingHelper.Log("CleanupDictionary", "Error: " + e.ToString(), 1);
				}
			}
		}

		/// <summary>
		/// This method will run continuously to look for files to be processed whose names were inserted into a dictionary
		/// </summary>
		public static void ProcessFiles()
		{
			if (IsProcessing) return;
			IsProcessing = true;
			LoggingHelper.Log("ProcessFiles", "File processing started.", 1);
			while (true)
			{
				try
				{
					if (_stopProcessing)
					{
						_stopProcessing = IsProcessing = false;
						LoggingHelper.Log("ProcessFiles", "File processing stopped.", 1);
						return;
					}

					KeyValuePair<string, InboundFile> fileToProcess = new KeyValuePair<string, InboundFile>();
					lock (_criticalLock)
					{
						if (InboundFiles.Count > 0)
							fileToProcess = InboundFiles.Where(file => !file.Value.ProcessComplete).OrderBy(file => file.Value.DateTimeDetected).FirstOrDefault();
					}

					if (fileToProcess.Key != null) // Key will be null if no Detected files to process in Dictionary (which were not already processed)
					{
						ProcessFile(fileToProcess.Key, fileToProcess.Value);
						lock (_criticalLock) { fileToProcess.Value.ProcessComplete = true; }
					}

					Thread.Sleep(WaitTime);
				}
				catch (Exception e)
				{
					LoggingHelper.Log("ProcessFiles", string.Format("Error occurred in ProcessFiles: {0}.", e.ToString()), 1);
				}
			}
		}

		private static void ProcessFile(string fileName, InboundFile inboundFile)
		{
			FileService FileService = new FileService();
            FileInstance fileInstance = new FileInstance();
            fileInstance.FileId = inboundFile.FileDefinition.Id;
            fileInstance.BeginTime = DateTime.Now.ToLocalTime();
            fileInstance = FileService.SaveFileInstance(fileInstance);
			LoggingHelper.Log("ProcessFiles", "Processing " + fileName + ", thread id " + Thread.CurrentThread.ManagedThreadId, fileInstance.InstanceId, 2);

			if (inboundFile.FileDefinition.UseHashCodeDuplicateDetection)
            {
                fileInstance.HashCode = FileService.CreateFileHash(inboundFile.FilePathAndName);

				if (FileService.IsDuplicateRun(inboundFile.FileDefinition.Id, fileInstance.HashCode))
				{
					fileInstance.Message = string.Format("Duplicate run detected for file name {0}, file id {1} and hash code {2}.  File was not processed.", fileName, inboundFile.FileDefinition.Id, fileInstance.HashCode);
					fileInstance = FileService.SaveFileInstance(fileInstance);
					LoggingHelper.Log("ProcessFile", fileInstance.Message, fileInstance.InstanceId, 1);
					lock (_criticalLock) { MoveFile(fileName, inboundFile.FilePath, ProcessedFolder, true, true); }
					return;
				}
			}

			// If column UseFileParsing in table FileDefinitions for this file type is false and column Processor contains a class name,
			//   call the ProcessFile method of that class to process the file.  The ProcessFile method must return type FileCompletionInfo
			//   and handle all errors, returning fatal errors in the FileCompletionInfo object.
			if (!inboundFile.FileDefinition.UseFileParsing && inboundFile.FileDefinition.Processor.Contains(','))
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
				string fileProcessorConfigXml = FileService.GetFileProcessorConfig(inboundFile.FileDefinition.Processor);
				FileCompletionInfo
				fileCompletionInfo          = 
				(new Parsing.Processor.RecordBlockingFileProcessor(fileProcessorConfigXml)).ProcessFile(inboundFile.FilePathAndName, fileInstance.InstanceId);
				fileInstance.Successful     = !fileCompletionInfo.FatalErrorOccurred;
				fileInstance.Message        =  fileCompletionInfo.FatalErrorMessage;
				fileInstance.FileDate       =  fileCompletionInfo.FileTimestamp;
				fileInstance.SequenceNumber =  fileCompletionInfo.FileNumber;
			}

			fileInstance.EndTime = DateTime.Now.ToLocalTime();

			// move file to processed location (with date sub-folder).  Use Error folder if an error occurred
			lock (_criticalLock) { MoveFile(fileName, inboundFile.FilePath, ProcessedFolder, true, !fileInstance.Successful); }

			LoggingHelper.Log("ProcessFiles", string.Format("In IFP:ProcessFile, processing finished.  Moving file {0} from {1} to {2}. {3}",
				fileName,
				inboundFile.FilePath,
				ProcessedFolder,
				(fileInstance.Successful ?
					"File was successfully processed." :
					"Error(s) occurred: " + fileInstance.Message)
				), fileInstance.InstanceId, (fileInstance.Successful ? 2 : 1));

			FileService.SaveFileInstance(fileInstance);
		}

		private static string MoveFile(string fileName, string fromFolder, string toFolder, bool isFinalDestination, bool shouldMoveToErrorFolder/*, int retryCount = 0*/)
		{
			// lock (_moveFileLock)   <-- single threaded, so shouldn't need      // to keep multiple processes from fighting over file names
			{
				string fullToFolder = toFolder;
				// Instead of moving file to just the Processed folder, put it in a sub folder with date
				if (isFinalDestination)
				{
					fullToFolder = Path.Join(fullToFolder, DateTime.Now.ToString("yyyy-MM-dd"));
					if (shouldMoveToErrorFolder) fullToFolder = Path.Join(fullToFolder, "Errors");
					if (!Directory.Exists(fullToFolder)) Directory.CreateDirectory(fullToFolder);
				}

				string fromFileName = Path.Join(fromFolder  , fileName);
				string toFileName   = Path.Join(fullToFolder, fileName);

				// if file already exists in 'move to' location, rename that file to end with _# so file just processed can be moved.
				if (File.Exists(toFileName))
				{
					string fileSuffix = ""; int fileSuffixNumber = 0;
					do
					{
						if (fileSuffixNumber > 10)
						{
							string error = string.Format("File not moved. Tried {0} through {1}", fileName, fileName + fileSuffix);
							LoggingHelper.Log("MoveFile", error, 1);
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
					LoggingHelper.Log("MoveFile", string.Format("Error moving file from {0} to {1}.  Error: {2}", fromFileName, toFileName, e.ToString()), 1);
					if (!isFinalDestination) return fromFileName;    // If move fails & file hasn't been processed yet, just process it from FTP location
																	 // Then, once file has been processed, it will be moved directly to processed folder

					// If moving to the final destination (processed folder) fails add to memory queue to be tried later
					/* TODO: remove when everything is working
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

		/*
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
					LoggingHelper.Log(string.Format("In RetryFileMove.  Moving {0} from {1} to {2}. Try {3}. Wait {4}.", fileName, fromFolder, ProcessedFolder, queueItem.RetryCount, waitTime), 2);
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
				LoggingHelper.Log(string.Format("Error in RetryFileMove.  Error: {0}", e.ToString()), 1);
			}
		}*/
	}
}