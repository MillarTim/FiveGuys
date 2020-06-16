using System;
using System.Collections.Generic;
using System.Text;

namespace CSS.Connector.FileProcessing
{
	internal class FileMoveQueueItem
	{
		public FileMoveQueueItem(string fromFile, string toFile, int retryCount)
		{
			FromFile = fromFile;
			ToFile = toFile;
			RetryCount = retryCount;
		}
		public string FromFile { get; set; }
		public string ToFile { get; set; }
		public int RetryCount { get; set; }
	}
}
