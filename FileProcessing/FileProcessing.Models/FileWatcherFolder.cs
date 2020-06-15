using System;
using System.Collections.Generic;
using System.Text;

namespace CSS.Connector.FileProcessing.Models
{
    /// <summary>
    /// Represents which folders the file watcher will monitor.
    /// </summary>
    public class FileWatcherFolder
    {
        /// <summary>
        /// UNC path of the folder to monitor.
        /// </summary>
        public string WatchingPath { get; set; }

		/// <summary>
		/// UNC path where the file is located while it is being processed.
		/// </summary>
		public string InProcessPath { get; set; }

		/// <summary>
		/// UNC path of where to move the processed files.
		/// </summary>
		public string ProcessedPath { get; set; }
    }
}
