using System;
using System.Runtime.Serialization;

namespace NQuery
{
	/// <summary>
	/// Base class for all exceptions thrown by the query engine.
	/// </summary>
	[Serializable]
	public class NQueryException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NQueryException"/> class.
		/// </summary>
		public NQueryException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NQueryException"/> class with a specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public NQueryException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NQueryException"/> class with a specified error message and a 
		/// reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException"/>
		/// parameter is not <see langword="null"/>, the current exception is raised in a catch block that handles the inner exception.</param>
		public NQueryException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NQueryException"/> class with serialized data.  
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context"> The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
		protected NQueryException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}