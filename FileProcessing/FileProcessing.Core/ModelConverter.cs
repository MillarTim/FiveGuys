using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSS.Connector.FileProcessing.Core.Models;
using CSS.Connector.FileProcessing.Models;

namespace CSS.Connector.FileProcessing.Core
{
    /// <summary>
    /// Helper method to convert the models used by Entity Framework to those exposed to the outside world.
    /// </summary>
    public static class ModelConverter
    {
        /// <summary>
        /// Converts a FileInstance to an EF module of FileInstance.
        /// </summary>
        /// <param name="fileInstance"></param>
        /// <returns></returns>
        public static CSS.Connector.FileProcessing.Core.Models.FileInstance ConvertFileInstanceToEFModule(CSS.Connector.FileProcessing.Models.FileInstance fileInstance)
        {
            return new CSS.Connector.FileProcessing.Core.Models.FileInstance
            {
                BeginTime = fileInstance.BeginTime,
                EndTime = fileInstance.EndTime,
                HashCode = fileInstance.HashCode,
                InstanceId = fileInstance.InstanceId,
                FileId = fileInstance.FileId,
                Message = fileInstance.Message,
                Successful = fileInstance.Successful,
                SequenceNumber = fileInstance.SequenceNumber,
                FileDate = fileInstance.FileDate
            };
        }
        /// <summary>
        /// Converts an EF module of FileInstance to a  FileInstance.
        /// </summary>
        /// <param name="fileInstance"></param>
        /// <returns></returns>
        public static CSS.Connector.FileProcessing.Models.FileInstance ConvertFileInstanceToServiceModel(CSS.Connector.FileProcessing.Core.Models.FileInstance fileInstance)
        {
            return new CSS.Connector.FileProcessing.Models.FileInstance
            {
                BeginTime = fileInstance.BeginTime,
                EndTime = fileInstance.EndTime,
                HashCode = fileInstance.HashCode,
                InstanceId = fileInstance.InstanceId,
                FileId = fileInstance.FileId,
                Message = fileInstance.Message,
                Successful = fileInstance.Successful,
                SequenceNumber = fileInstance.SequenceNumber,
                FileDate = fileInstance.FileDate
            };
        }

        internal static List<FileProcessing.Models.FileInstanceDisplay> ConvertFileInstanceListToServiceModel(List<Models.FileInstance> list)
        {
            FileContext fileContext = new FileContext();
            var files = fileContext.GetFiles();

            return list.ConvertAll(r =>
           new FileProcessing.Models.FileInstanceDisplay
           {
               BeginTime = r.BeginTime,
               EndTime = r.EndTime,
               HashCode = r.HashCode,
               InstanceId = r.InstanceId,
               FileId = r.FileId,
               Message = r.Message,
               Successful = r.Successful,
			   IsInProcess = (r.EndTime == null && r.Message == null),
               SequenceNumber = r.SequenceNumber,
               FileDate = r.FileDate,
               Name = GetFileName(files, r.FileId),
               Direction = files.Where(f => f.Id == r.FileId).FirstOrDefault()?.Direction,
               RegexNameExpression = files.Where(f => f.Id == r.FileId).FirstOrDefault()?.RegexNameExpression
		   });
        }

		private static string GetFileName(List<Models.FileDefinition> files, int fileId)
		{
			var item = files.Where(f => f.Id == fileId).FirstOrDefault()?.Name;
			if (item == null) item = string.Format("File ID {0}", fileId);
			return item;
		}
	}
}
