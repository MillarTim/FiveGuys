using CSS.Connector.FileProcessing.Models;
using CSS.Connector.FileProcessors.Vanguard;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSS.Connector.FileProcessing.Core.Test
{
    [TestClass]
    public class FileProcessingTests
    {
        [TestMethod]
        public void VMCFileTest()
        {
            VMCFileProcessor proc = new VMCFileProcessor();
            FileInstance instance = new FileInstance();
            instance = proc.ProcessFile(@"C:\data\VMC.MACH.txt", instance);
            Assert.IsTrue(instance.SequenceNumber == 1);

        }
    }
}
