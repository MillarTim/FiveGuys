using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

namespace CSS.Connector.FileProcessing
{
	/// <summary>
	/// Class to "look for" files placed in the "in folder" defined by def_trn.  On a different
	/// thread (in class InboundFileProcessor), the file type will be determined.  The program ID will be
	/// retrieved from def_cmt and that program will be called to process the file.  Upon successful completion,
	/// this class (InboundFileWatcher) will move the file to the "processed folder" also defined in def_trn.
	/// </summary>
	internal class InboundFileWatcher
	{
		internal const string PROCESS_NAME = "InboundFileWatcher";

		[System.ComponentModel.Browsable(false)]
		public event ErrorEventHandler OnFileWatcherError = delegate { };

		private Core.LoggingHelper _loggingHelper;
		private Core.LoggingHelper LoggingHelper
		{
			get
			{
				if (_loggingHelper == null) _loggingHelper = new Core.LoggingHelper();
				return _loggingHelper;
			}
		}

		private static System.IO.FileSystemWatcher _fileSystemWatcherForChangedFilesInFtpFolder;
		private static System.IO.FileSystemWatcher _fileSystemWatcherForCreatedFilesInFtpFolder;
		private static System.IO.FileSystemWatcher _fileSystemWatcherForChangedFilesInProcessingFolder;
		private static System.IO.FileSystemWatcher _fileSystemWatcherForCreatedFilesInProcessingFolder;

		// objects to lock for critical sections of code
		public static object _ProcessFileLock = new object();

		// file locations ([0] = 'FTP folder', [1] = 'processing folder', [2] = 'processed folder')
		private string[] _folderNames;
		public static string Path;
		internal enum Folders
		{
			FTP,
			PROCESSING,
			PROCESSED
		}

		#region properties

		internal string[] FolderNames
		{
			get
			{
				if (_folderNames == null)
				{
					_folderNames = DataHelper.GetFolderNames();
					LoggingHelper.Log("IFW::FolderNames", string.Format("Folders: FTP={0}; Processing={1}; Processed={2}", _folderNames[0], _folderNames[1], _folderNames[2]), 1);
				}
				return _folderNames;
			}
		}

		private DataHelper _dataHelper;   // houses all functions that use db
		internal DataHelper DataHelper
		{
			get
			{
				if (_dataHelper == null)
				{
					_dataHelper = new DataHelper();
				}
				return _dataHelper;
			}
		}

		// Wait time in miliseconds before looking for existing files in the watched folders without the read-only flag set.
		//   This would be for files that arrived while the service was down.
		private int InitialWaitTime { get; set; }

		// Wait time in miliseconds before retry if file cannot be processed immediately
		//   (i.e. large file being copied in still has lock on it--or event fires before file actually shows up in directory)
		private int WaitTime { get; set; }

		// Retry count for failed attempts to get access to process the file
		private int RetryCount { get; set; }

		IConfigurationSection _configurationSection;
		#endregion

		public InboundFileWatcher()
		{
		}

		~InboundFileWatcher()
		{
			Stop();
		}

		public void RestartWatcher(/*object sender, ErrorEventArgs e*/)
		{
			Stop();
			Start();
		}

		public void Start()
		{
			try
			{
				IConfigurationBuilder builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
				IConfigurationRoot configuration = builder.Build();
				// _configurationSection = configuration.GetSection("Logging").GetSection("LogLevel").GetSection("Default");

				Core.LoggingHelper.UseVerboseLogging = (configuration.GetSection("LogLevel")?.Value == "Verbose");
				Core.LoggingHelper.UseFrameworkLogging = true;
				LoggingHelper.Log("IFW::Start", string.Format("{0} logging level.", (Core.LoggingHelper.UseVerboseLogging ? "Verbose" : "Normal")), 1);

				int retryCount = 0;
				int.TryParse(configuration.GetSection("RetryCount")?.Value, out retryCount);
				RetryCount = (retryCount < 0 || retryCount > 50 ? 10 : retryCount); // allow between 0 & 50, default is 10
				LoggingHelper.Log("IFW::Start", string.Format("Retry count: {0}.", RetryCount), 1);

				int waitTime = 0;
				int.TryParse(configuration.GetSection("WaitTime")?.Value, out waitTime);
				WaitTime = (waitTime < 100 || waitTime > 10000 ? 1000 : waitTime); // allow between .1 & 10 seconds, default is 1 sec
				LoggingHelper.Log("IFW::Start", string.Format("Wait time: {0} ms.", WaitTime), 1);

				int initialWaitTime = 0;
				int.TryParse(configuration.GetSection("InitialWaitTime")?.Value, out initialWaitTime);
				InitialWaitTime = (initialWaitTime < 1 || initialWaitTime > 120000 ? 45000 : initialWaitTime); // less than 2 min. 45 secs is default
				LoggingHelper.Log("IFW::Start", string.Format("Initial wait time: {0} ms.", InitialWaitTime), 1);

				InboundFileProcessor.MonitoringFolder = FolderNames[0];
				InboundFileProcessor.ProcessingFolder = FolderNames[1];
				InboundFileProcessor.ProcessedFolder = FolderNames[2];
				InboundFileProcessor.WaitTime = WaitTime;

				LoggingHelper.Log("IFW::Start", string.Format("Starting to watch for files in folders ({0}) and ({1}).", FolderNames[(int)Folders.FTP], FolderNames[(int)Folders.PROCESSING]), 1);

				_fileSystemWatcherForCreatedFilesInFtpFolder = new System.IO.FileSystemWatcher();
				_fileSystemWatcherForChangedFilesInFtpFolder = new System.IO.FileSystemWatcher();
				_fileSystemWatcherForCreatedFilesInProcessingFolder = new System.IO.FileSystemWatcher();
				_fileSystemWatcherForChangedFilesInProcessingFolder = new System.IO.FileSystemWatcher();

				((System.ComponentModel.ISupportInitialize)(_fileSystemWatcherForCreatedFilesInFtpFolder)).BeginInit();
				((System.ComponentModel.ISupportInitialize)(_fileSystemWatcherForChangedFilesInFtpFolder)).BeginInit();
				((System.ComponentModel.ISupportInitialize)(_fileSystemWatcherForCreatedFilesInProcessingFolder)).BeginInit();
				((System.ComponentModel.ISupportInitialize)(_fileSystemWatcherForChangedFilesInProcessingFolder)).BeginInit();

				_fileSystemWatcherForCreatedFilesInFtpFolder.EnableRaisingEvents = true;
				_fileSystemWatcherForChangedFilesInFtpFolder.EnableRaisingEvents = true;
				_fileSystemWatcherForCreatedFilesInProcessingFolder.EnableRaisingEvents = true;
				_fileSystemWatcherForChangedFilesInProcessingFolder.EnableRaisingEvents = true;

				((System.ComponentModel.ISupportInitialize)(_fileSystemWatcherForCreatedFilesInFtpFolder)).EndInit();
				((System.ComponentModel.ISupportInitialize)(_fileSystemWatcherForChangedFilesInFtpFolder)).EndInit();
				((System.ComponentModel.ISupportInitialize)(_fileSystemWatcherForCreatedFilesInProcessingFolder)).EndInit();
				((System.ComponentModel.ISupportInitialize)(_fileSystemWatcherForChangedFilesInProcessingFolder)).EndInit();

				_fileSystemWatcherForCreatedFilesInFtpFolder.Path = FolderNames[(int)Folders.FTP];
				_fileSystemWatcherForChangedFilesInFtpFolder.Path = FolderNames[(int)Folders.FTP];
				_fileSystemWatcherForCreatedFilesInProcessingFolder.Path = FolderNames[(int)Folders.PROCESSING];
				_fileSystemWatcherForChangedFilesInProcessingFolder.Path = FolderNames[(int)Folders.PROCESSING];

				// Files should be picked up for processing if:
				//  - A file has been moved or copied into the IN directory (or the Processing directory) -or-
				//  - The read-only attribute has been removed from a file in the IN (or Processing) directory
				// The reason there are two FileSystemWatcher variables, one for created and one for changed
				//   files, is:
				//   - If the NotifyFilter.Attributes is not specified, the files won't be picked up if the read-only
				//     flag is removed from a file.
				//   - But if NotifyFilter.Attributes is specified, for some reason, the event will not fire when
				//     a file is copied into the directory (it will still be picked up if it is moved, but not copied).

				_fileSystemWatcherForCreatedFilesInFtpFolder.Created += new FileSystemEventHandler(OnFileCreatedOrChangedInFtpFolder);
				_fileSystemWatcherForChangedFilesInFtpFolder.Changed += new FileSystemEventHandler(OnFileCreatedOrChangedInFtpFolder);
				_fileSystemWatcherForChangedFilesInFtpFolder.NotifyFilter = NotifyFilters.Attributes; // fire event if attribute changed

				_fileSystemWatcherForCreatedFilesInProcessingFolder.Created += new FileSystemEventHandler(OnFileCreatedOrChangedInProcessingFolder);
				_fileSystemWatcherForChangedFilesInProcessingFolder.Changed += new FileSystemEventHandler(OnFileCreatedOrChangedInProcessingFolder);
				_fileSystemWatcherForChangedFilesInProcessingFolder.NotifyFilter = NotifyFilters.Attributes; // fire event if attribute changed

				OnFileWatcherError += (sender, e) =>
				{
					LoggingHelper.Log("IFW::Start", string.Format("File Watcher error. Restarting Watcher. Sender: {0}; Object: {1}; Error: {2}" + sender.ToString(), e.ToString(), e.GetException()), 1);
					RestartWatcher(/*sender, e*/);
				};

				Task.Run(() => InboundFileProcessor.ProcessFiles()); // This process will monitor a dictionary for new file entries & process them on a different thread
				Task.Run(() => InboundFileProcessor.CleanupDictionary()); // This process remove detected files after processing is finished & a certain time has elapsed

				/*Task.Run(() =>*/
				this.ProcessPendingFiles()/*)*/;    // In case there are files that arrived while the service was down. This will pick up non read-only files.
			}
			catch (Exception ee)
			{
				try
				{
					LoggingHelper.Log("IFW::Start", "Error: " + ee.ToString(), 1);
				}
				catch { }
			}
		}

		private void Stop()
		{
			try
			{
				_folderNames = null;  // So they will be reread upon start

				OnFileWatcherError -= (sender, e) => RestartWatcher(/*sender, e*/);

				_fileSystemWatcherForCreatedFilesInFtpFolder.Created -= new FileSystemEventHandler(OnFileCreatedOrChangedInFtpFolder);
				_fileSystemWatcherForChangedFilesInFtpFolder.Changed -= new FileSystemEventHandler(OnFileCreatedOrChangedInFtpFolder);
				_fileSystemWatcherForCreatedFilesInProcessingFolder.Created -= new FileSystemEventHandler(OnFileCreatedOrChangedInProcessingFolder);
				_fileSystemWatcherForChangedFilesInProcessingFolder.Changed -= new FileSystemEventHandler(OnFileCreatedOrChangedInProcessingFolder);

				_fileSystemWatcherForCreatedFilesInFtpFolder.Dispose();
				_fileSystemWatcherForChangedFilesInFtpFolder.Dispose();
				_fileSystemWatcherForCreatedFilesInProcessingFolder.Dispose();
				_fileSystemWatcherForChangedFilesInProcessingFolder.Dispose();

				InboundFileProcessor.StopProcessingFiles();
			}

			catch (Exception ee)
			{
				try
				{
					LoggingHelper.Log("IFW::Stop", "Error: " + ee.ToString(), 1);
				}
				catch { }
			}
		}

		/// <summary>
		/// FileDefinition watcher has found a new file (or attribute change on existing file) in 'in folder'.  Pass
		/// the call on.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		private void OnFileCreatedOrChangedInFtpFolder(object source, FileSystemEventArgs e)
		{
			try
			{
				LoggingHelper.Log("IFW::OnFileCrOrChFtp", string.Format("File {0} detected in FTP folder.", e.FullPath), 2);
				ProcessFile(e.FullPath, true);
			}
			catch (Exception ee)
			{
				LoggingHelper.Log("IFW::OnFileCrOrChFtp", "Error:" + ee.ToString(), 1);
			}
		}

		/// <summary>
		/// FileDefinition watcher has found a new file (or attribute change on existing file) in 'in folder'.  Pass
		/// the call on.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		private void OnFileCreatedOrChangedInProcessingFolder(object source, FileSystemEventArgs e)
		{
			try
			{
				LoggingHelper.Log("IFW::OnFileCrOrChPr", string.Format("File {0} detected in Processing folder.", e.FullPath), 2);
				ProcessFile(e.FullPath, false);
			}
			catch (Exception ee)
			{
				LoggingHelper.Log("IFW::OnFileCrOrChPr", "Error:" + ee.ToString(), 1);
			}
		}

		/// <summary>
		/// Files may have been placed in 'in folder' while service was stopped.  So, on start up, each
		/// file must be checked and processed if conditions are met.  Wait 30 seconds from when service is started 
		/// since it may take a while for the watcher to kick in to pick up files.  That way if files arrive between
		/// when the service was started and the watcher begins detecting files, the sleep will allow this logic will pick up the files.
		/// </summary>
		private void ProcessPendingFiles()
		{
			try
			{
				Thread.Sleep(InitialWaitTime);

				string[] files = Directory.GetFiles(FolderNames[(int)Folders.PROCESSING]);
				for (int i = 0; i < files.GetLength(0); i++)
				{
					ProcessFile(files[i], false);
				}
				files = Directory.GetFiles(FolderNames[(int)Folders.FTP]);
				for (int i = 0; i < files.GetLength(0); i++)
				{
					ProcessFile(files[i], true);
				}
			}

			catch (Exception e)
			{
				LoggingHelper.Log("IFW::ProcessPendingFiles", "Error: " + e.ToString(), 1);
			}
		}

		/// <summary>
		/// Test Eligibility and Add to Processing Queue
		/// </summary>
		/// <param name="filePathAndName">The file (with path) to process</param>
		/// <param name="shouldMoveFile">Should move file to In-Process folder before processing.  False means file is already there.</param>
		private void ProcessFile(string filePathAndName, bool shouldMoveFile)
		{
			InboundFileProcessor.TestEligibilityAndAddToProcessingQueue(filePathAndName, shouldMoveFile);
		}
	}
}