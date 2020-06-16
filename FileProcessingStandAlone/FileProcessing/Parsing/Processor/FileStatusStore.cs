// TODO: Remove this class
/*
using System;
using CSS.Framework.DataAccess;
using CSS.Connector.FileProcessing.Parsing.Parser;

namespace CSS.Connector.FileProcessing.Parsing.Processor
{
    /// <summary>
    /// Class used to read and write file processing details to the inbound file processing log (trd_rec).
    /// </summary>
    public sealed class FileStatusStore
    {
        #region Private Methods
        private FileStatusStore()
        {
        }
        static private InboundFileStatusKey CreateKeyFromFileInfo(FileInfo info)
        {
            InboundFileStatusKey key = new InboundFileStatusKey();
            key.Date = info.Date;
            // TODO: New File processing should start renaming the three digits of
            // the type id to the auto route number (.FileProcessing.FileTypeMap.config, def_cmt change as well)
            // We found that DTCC was using the same type id but different AutoRoute numbers.
            // Example: ACATS Reversal File for CBRS (02091008)
            //          CNS Miscellaneous Activity File for Balance Order Processing (02042008)
            key.TypeId = (!string.IsNullOrWhiteSpace(info.AutoRouteNumber)) ? info.AutoRouteNumber : info.TypeId;
            key.SequenceNumber = info.SequenceNumber;
            key.Source = info.Source;
            return key;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Reads the file processing details for the specified file instance.
        /// </summary>
        /// <param name="key">The key that uniquely indentifies the file status to retrieve.</param>
        /// <returns>The file status.</returns>
        static public InboundFileStatus ReadFileStatus(InboundFileStatusKey key)
        {
            InboundFileStatusKeyPayload keyPayload = new InboundFileStatusKeyPayload();
            keyPayload.InboundFileStatusKeys = new InboundFileStatusKeyCollection();
            keyPayload.InboundFileStatusKeys.Add(key);
            InboundFileStatusPayload statusPayload = (InboundFileStatusPayload)DataService.Read(keyPayload);
            if (statusPayload.InboundFileStatuses.Count > 1)
            {
                throw new NotSupportedException(Errors.MoreThanOneRowReturnedFromFileStatus);
            }
            if (statusPayload.InboundFileStatuses.Count == 0)
            {
                return null;
            }
            return statusPayload.InboundFileStatuses[0];
        }

        /// <summary>
        /// Writes the specified file details to the inbound file process processing log
        /// </summary>
        /// <param name="status">The file details to write</param>
        /// <returns>The updated file details written to the inbound file processing log</returns>
        static public InboundFileStatus WriteFileStatus(InboundFileStatus status)
        {
            InboundFileStatusPayload payload = new InboundFileStatusPayload();
            payload.InboundFileStatuses = new InboundFileStatusCollection();
            payload.InboundFileStatuses.Add(status);
            payload = (InboundFileStatusPayload)DataService.Write(payload);
            if (payload.InboundFileStatuses.Count != 1)
            {
                throw new NotSupportedException(Errors.InvalidRecordCountInFileStatus);
            }
            return payload.InboundFileStatuses[0];
        }

        /// <summary>
        /// Writes the file details with a status of <b>In Progress</b> to the inbound file processing log.
        /// </summary>
        /// <param name="info">The file details to write.</param>
        /// <param name="fileCanBeReprocessed">Flag that indicates if the file can be reprocessed.</param>
        /// <returns>The updated file details written to the inbound file processing log.</returns>
        static public InboundFileStatus WriteFileInProgressStatus(FileInfo info, bool fileCanBeReprocessed)
        {
            InboundFileStatusKey key = CreateKeyFromFileInfo(info);
            InboundFileStatus status = ReadFileStatus(key);
            if (status != null)
            {
                if (status.FileProcessingStatus == InboundFileProcessingStatus.InProgress)
                {
                    throw new FileProcessingException(Errors.FileAlreadyInProgress);
                }
                if (!fileCanBeReprocessed)
                {
                    throw new FileProcessingException(Errors.FileConfigDoesNotAllowReprocessing);
                }
                status.PassNumber += 1;
                status.RowState = RowState.Modified;
            }
            else
            {
                status = new InboundFileStatus();
                status.RowState = RowState.Added;
                status.Date = info.Date;
                // TODO: New File processing should start renaming the three digits of
                // the type id to the auto route number (.FileProcessing.FileTypeMap.config, def_cmt change as well)
                // We found that DTCC was using the same type id but different AutoRoute numbers.
                // Example: ACATS Reversal File for CBRS (02091008)
                //          CNS Miscellaneous Activity File for Balance Order Processing (02042008)
                status.TypeId = (!string.IsNullOrWhiteSpace(info.AutoRouteNumber)) ? info.AutoRouteNumber : info.TypeId;
                status.SequenceNumber = info.SequenceNumber;
                status.Source = info.Source;
                status.PassNumber = 1;
            }
            status.FileProcessingStatus = InboundFileProcessingStatus.InProgress;
            status.FullFileName = info.ShortName;
            status.ProcessStartTime = DateTime.Now;
            return WriteFileStatus(status);
        }

        /// <summary>
        /// Writes an end row using the specified file details to the file processing log.
        /// </summary>
        /// <param name="status">The file details to write.</param>
        /// <param name="completionInfo">The completed file details to write.</param>
        /// <param name="processingStatus">The completed file status to write.</param>
        static public void WriteFileProcessingFinishedStatus(InboundFileStatus status, FileCompletionInfo completionInfo, InboundFileProcessingStatus processingStatus)
        {
            status.RowState = RowState.Modified;
            status.FileProcessingStatus = processingStatus;
            status.ProcessEndTime = DateTime.Now;
            status.TotalRecords = completionInfo.TotalRecords;
            status.RecordsProcessed = completionInfo.RecordsProcessed;
            //status.RecordsFailed
            WriteFileStatus(status);
        }
        #endregion
    }
}
*/