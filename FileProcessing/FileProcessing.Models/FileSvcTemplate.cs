using System;
using System.Collections.Generic;
using System.Text;

namespace CSS.Connector.FileProcessing.Models
{
    public abstract class FileSvcTemplate
    {
        public abstract List<FileWatcherFolder> GetFileWatcherFolders();

        public abstract void WriteFileEventLog(FileEventLog log);

        public abstract List<FileDefinition> FindFilesByFileName(string fileName);

        public abstract FileInstance SaveFileInstance(FileInstance fileInstance);

        public abstract string CreateFileHash(string filename);

        public abstract bool IsDuplicateRun(int id, string hashCode);

        public abstract List<FileInstanceDisplay> GetFileInstances(DateTime startDate, DateTime endDate);

		public abstract string GetFileProcessorConfig(string processor);

		public abstract string GetTypeMapping(string fileId, string keyType);
	}

}
