using System;

using CSS.Connector.FileProcessing.Core;

namespace CSS.Connector.FileProcessing
{
    /// <summary>
    /// This class is used to read from and write to the DB.
    /// </summary>
    internal class DataHelper
    {
        public DataHelper()
        {
        }

        /// <summary>
        /// Gets the incoming, processing and processed folder paths for the InboundFileWatcher.
        /// </summary>
        /// <returns>An array of folder names.  The first element will contain the inbound folder (where
        /// the file watcher will look for files).  The second element will contain file folder where files
		/// will be located during processing.  File watcher will also be watching for files in this folder.
		/// The third element will contain the folder for the processed files(where the file will be moved
		/// upon successful completion of processing.</returns>
        public string[] GetFolderNames()
        {
            FileService fileSvc = new FileService();
            var folders = fileSvc.GetFileWatcherFolders();
            if (folders.Count == 0)
            {
                throw new ApplicationException("No FileDefinition Watcher folders defined in database.");
            }
            string[] result = new string[3];
            result[0] = folders[0].WatchingPath;
            result[1] = folders[0].InProcessPath;
            result[2] = folders[0].ProcessedPath;
#if DEBUG
			// Use folders on local machine for debugging so strip machine name away
			for (int i = 0; i < result.Length; i++)
			{
				// Include dash "-" in characters to remove; May need to add more characters going forward
				result[i] = System.Text.RegularExpressions.Regex.Replace(result[i], @"\\\\[-\w]+", "");
			}
#endif
			return result;
        }

        /// <summary>
        /// Writes a stop row to the batch log table.
        /// </summary>
        /// <param name="endingInfo">Text indicating what processing is ending</param>
        /// <param name="fileId">The ID for the inbound file to be processed.</param>
        /// <param name="errorInfo">Text indicating the error that occurred.  Blank if processing
        /// completed normally.</param>
        public void WriteStopRowToLog(string endingInfo, string fileId, string errorInfo)
        {
			/* TODO: Log in a different manner
            if (!String.IsNullOrEmpty(errorInfo))
            {
                BatchLogger.WriteStopErrorRow(errorInfo, string.Empty, false);
            }
            else
            {
                BatchLogger.WriteStopRow(false);
            }
			*/
        }
    }
}