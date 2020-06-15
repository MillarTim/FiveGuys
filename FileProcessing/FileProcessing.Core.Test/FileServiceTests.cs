using CSS.Connector.FileProcessing.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace CSS.Connector.FileProcessing.Core.Test
{
    [TestClass]
    public class FileServiceTests
    {
        [TestMethod]
        public void GetFolderNames()
        {
            FileService svc = new FileService();
            var folders = svc.GetFileWatcherFolders();
            Assert.IsTrue(folders.Count > 0);
        }

        [TestMethod]
        public void FindFileByInputName()
        {
            FileService svc = new FileService();
            var files = svc.FindFilesByFileName("VMC.MACH.txt");

            Assert.IsTrue(files.Count ==1);
        }

        [TestMethod]
        public void SaveFileInstanceTest()
        {
            FileService svc = new FileService();
            FileInstance fileins = new FileInstance();
            fileins.BeginTime = DateTime.Now;
            fileins.FileId = 1;
            fileins = svc.SaveFileInstance(fileins);
            fileins.EndTime = DateTime.Now;
            fileins = svc.SaveFileInstance(fileins);
        }
        
    }
}
