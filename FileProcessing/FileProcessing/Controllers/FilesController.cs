using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSS.Cloud.Common;
using CSS.Cloud.Framework;
using CSS.Connector.FileProcessing.Core;
using CSS.Connector.FileProcessing.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CSS.Connector.FileProcessing.Controllers
{
	
	[ApiController]
    [Authorize]
    public class FilesController : BaseController
	{
		[HttpGet]
        [Route("api/v1/Files/FileInstances")]
        [Authorize]
        public List<FileInstanceDisplay> FileInstances(DateTime startDate, DateTime endDate)
		{
            FileService svc = new FileService();
            return svc.GetFileInstances(startDate, endDate);
        }

		[HttpPost]
		[Route("api/v1/Files/Watcher")]
		[Authorize]
		public void RestartFileWatcher()
		{
			Program.InboundFileWatcher.RestartWatcher();
		}
	}
}
