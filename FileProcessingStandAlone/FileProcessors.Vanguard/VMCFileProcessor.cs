/*
using CSS.Connector.FileProcessing;
using CSS.Connector.FileProcessing.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
 

namespace CSS.Connector.FileProcessors.Vanguard
{
    /// <summary>
    /// Class to process all the VMC files with the similar format.
    /// </summary>
    public class VMCFileProcessor : IFileProcessor
    {
        public FileInstance ProcessFile(string filePath, FileInstance fileInstance)
        {
            try
            {
                string recordId = string.Empty;
                bool readHeader = false;
                using (StreamReader sr = new StreamReader(filePath))
                {
                    
                    string line = "";
                    while ((line = sr.ReadLine()) != null)
                    {
                        recordId = line.Substring(0, 2);
                        switch (recordId)
                        {
                            case "01":
                                if (!readHeader)
                                {
                                    //2019-05-20-10.09.52.680000
                                    DateTime fileDate = new DateTime();
                                    DateTime.TryParseExact(line.Substring(14, 26), "yyyy-MM-dd-HH.mm.ss.FFFFFF", null, DateTimeStyles.None, out fileDate);
                                    fileInstance.FileDate = fileDate;
                                    int filesequence = 0;
                                    int.TryParse(line.Substring(12, 2), out filesequence);
                                    fileInstance.SequenceNumber = filesequence;
                                    readHeader = true;
                                }
                                break;
                            case "09":
                                string fileId = line.Substring(2, 10).Trim();
                                string debitTotal = CobolUtility.InsertImpliedDecimal(CobolUtility.ConvertFromCOBOLSignedNumberToDouble1(line.Substring(14, 18), 0), 2);

                                string creditTotal = CobolUtility.InsertImpliedDecimal(CobolUtility.ConvertFromCOBOLSignedNumberToDouble1(line.Substring(32, 18), 0),2);
                                //LoggingHelper.Log(string.Format("fileId {0} debitTotal {1} credit total {2}", fileId, debitTotal, creditTotal));
                                break;
                            case "99":
                                string totalRecords = CobolUtility.InsertImpliedDecimal(CobolUtility.ConvertFromCOBOLSignedNumberToDouble1(line.Substring(15, 18), 0), 2);
                                break;
                            default:
                                break;
                        }
                        
                         
                    }
                }
                
                fileInstance.Successful = true;
                return fileInstance;
            }
            catch (Exception ex)
            {
                fileInstance.EndTime = DateTime.Now.ToLocalTime();
                fileInstance.Successful = false;
                fileInstance.Message = ex.ToString();
                return fileInstance;
            }
            
            
            
        }
        
    }
}
*/