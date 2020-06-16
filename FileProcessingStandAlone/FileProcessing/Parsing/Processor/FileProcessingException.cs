using System;
using System.Runtime.Serialization;

namespace CSS.Connector.FileProcessing.Parsing.Processor
{
    /// <summary>
    /// The exception that is thrown when processing a file fails.
    /// </summary>
    [Serializable]
    public class FileProcessingException : Exception
    {
        /// <summary>
        /// Initializes a new new instance of the FileProcessingException class
        /// </summary>
        public FileProcessingException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new new instance of the FileProcessingException class with its errorMessage
        /// string set to errorMessage
        /// </summary>
        /// <param name="message">A description of the error. The content of errorMessage is intended
        /// to be understood by humans. The caller of this constructor is required to ensure that
        /// this string has been localized for the current system culture.</param>
        public FileProcessingException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new new instance of the FileProcessingException class with a specified
        /// error errorMessage and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">A description of the error. The content of errorMessage is intended
        /// to be understood by humans. The caller of this constructor is required to ensure that
        /// this string has been localized for the current system culture.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.
        /// If the innerException parameter is not null, the current exception is raised in a catch
        /// block that handles the inner exception.</param>
        public FileProcessingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new new instance of the FileProcessingException class with the specified
        /// serialization and context information.
        /// </summary>
        /// <param name="info">An object that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">An object that contains contextual information about the source or destination.</param>
        protected FileProcessingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}