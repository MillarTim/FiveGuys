using CSS.Connector.FileProcessing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace CSS.Connector.FileProcessing
{
    /// <summary>
    /// Interface used to hook into the file watcher.  
    /// </summary>
    public interface IFileProcessor
    {
        /// <summary>
        /// Method to process a file.
        /// </summary>
        /// <param name="filePath">The full name and path of the file to process.</param>
        /// <param name="fileInstance">The FileInstance of the file run.  This is assumed to be already
        /// initialized.  It is expected that the file processor fill in the details, such as the sequence number,
        /// successful, and message properties.</param>
        /// <returns>The FileInstance that will be logged to the system. </returns>
        FileInstance ProcessFile(string filePath, FileInstance fileInstance);
    }
}
