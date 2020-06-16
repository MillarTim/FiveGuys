using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CSS.Connector.FileProcessing.Core.Models;

namespace CSS.Connector.FileProcessing
{
	internal class InboundFile
	{
		public InboundFile(bool processComplete, DateTime dateTimeDetected, string filePath, string filePathAndName, FileDefinition fileDefinition)
		{
			ProcessComplete  = processComplete;
			DateTimeDetected = dateTimeDetected;
			FilePath         = filePath;
			FilePathAndName  = filePathAndName;
			FileDefinition   = fileDefinition;
		}

		/*
		public enum Status
		{
			Detected,
			InProcess,
			Complete
		}
		public Status CurrentStatus { get; set; }
		*/
		public bool ProcessComplete { get; set; }
		public DateTime DateTimeDetected { get; set; }
		public string FilePath { get; set; }
		public string FilePathAndName { get; set; }
		public FileDefinition FileDefinition { get; set; }
	}
}
