using CSS.Connector.FileProcessing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace CSS.Connector.FileProcessing.Core
{
    /// <summary>
    /// Service for File Processing
    /// </summary>
    public class FileService : FileSvcTemplate
    {
        /// <summary>
        /// Computes a 32 character hex string that is the MD5 file hash of the bytes of the file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public override string CreateFileHash(string filename)
        {
            if (System.IO.File.Exists(filename))
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = System.IO.File.OpenRead(filename))
                    {
                        var hash = md5.ComputeHash(stream);
                        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    }
                }
            }
            else
            {
                return "";
            }
        }
        /// <summary>
        /// Find the FileDefinitions in the store based on the filename.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public override List<FileDefinition> FindFilesByFileName(string fileName)
        {
            List<FileDefinition> results = new List<FileDefinition>();
            FileContext dbContext = new FileContext();
            var files = dbContext.GetFiles().Where(x => Regex.IsMatch(fileName, x.RegexNameExpression)).ToList();
            return files.ConvertAll(x =>
            new CSS.Connector.FileProcessing.Models.FileDefinition
            {
                Id = x.Id,
                Direction = x.Direction,
                Name = x.Name,
                Processor = x.Processor,
                RegexNameExpression = x.RegexNameExpression,
                UseHashCodeDuplicateDetection = x.UseHashCodeDuplicateDetection,
				UseFileParsing = x.UseFileParsing
            }
            );
        }
        /// <summary>
        /// Returns the File Watcher Folder names from the store.
        /// </summary>
        /// <returns></returns>
        public override List<FileWatcherFolder> GetFileWatcherFolders()
        {
            FileContext dbContext = new FileContext();
            return dbContext.GetFileWatcherFolders().ConvertAll(x =>
                new FileWatcherFolder
                {
                    WatchingPath = x.WatchingPath,
					InProcessPath = x.InProcessPath,
                    ProcessedPath = x.ProcessedPath
                }
            );
        }

        /// <summary>
        /// Saves the FileInstance in the store.
        /// </summary>
        /// <param name="fileInstance"></param>
        /// <returns></returns>
        public override FileInstance SaveFileInstance(FileInstance fileInstance)
        {
            FileContext dbContext = new FileContext();
           
            var results = ModelConverter.ConvertFileInstanceToServiceModel((dbContext.SaveFileInstance(ModelConverter.ConvertFileInstanceToEFModule(fileInstance))));
            return results;
        }

        /// <summary>
        /// Writes a new FileEventLog to the store.
        /// </summary>
        /// <param name="log"></param>
        public override void WriteFileEventLog(FileEventLog log)
        {
            FileContext dbContext = new FileContext();
            CSS.Connector.FileProcessing.Core.Models.FileEventLog fileEventlog = new Models.FileEventLog();
            fileEventlog.TimeStamp = log.TimeStamp;
            fileEventlog.Message = log.Message;
			fileEventlog.InstanceId = log.InstanceId;

            dbContext.WriteFileEventLog(fileEventlog);
        }
        /// <summary>
        /// Returns true if a duplicate run was detected based on the hashcode and fileId.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="hashCode"></param>
        /// <returns></returns>
        public override bool IsDuplicateRun(int id, string hashCode)
        {
            FileContext dbContext = new FileContext();
            return dbContext.CheckForDuplicate(id, hashCode);
        }
        /// <summary>
        /// Returns the FileInstance history for display purposes
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public override List<FileInstanceDisplay> GetFileInstances(DateTime startDate, DateTime endDate)
        {
            FileContext dbContext = new FileContext();
            return ModelConverter.ConvertFileInstanceListToServiceModel(dbContext.GetFileInstances(startDate, endDate));
        }

		public override string GetFileProcessorConfig(string processor)
		{
			FileContext dbContext = new FileContext();
			return dbContext.GetFileProcessorConfig(processor);
		}

		public override string GetTypeMapping(string fileId, string keyType)
		{
			FileContext dbContext = new FileContext();
			return dbContext.GetTypeMapping(fileId, keyType);
		}
	}
}
