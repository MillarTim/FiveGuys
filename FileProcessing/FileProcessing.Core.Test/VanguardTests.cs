using CSS.Connector.FileProcessors.Vanguard;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSS.Connector.FileProcessing.Core.Test
{
    [TestClass]
    public class VanguardTests
    {
        [TestMethod]
        public void TestMovementServiceHelper()
        {
			ExpectedActivity expectedActivity = new ExpectedActivity();
			expectedActivity.AccountNumber    = "321654987";
			expectedActivity.Amount           = 12;
			expectedActivity.TransactionType  = "trt";
			expectedActivity.Trailer          = "test";
			new MovementServiceHelper().PostExpectedActivity(expectedActivity);
        }
    }
}
