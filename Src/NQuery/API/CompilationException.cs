using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace NQuery
{
	/// <summary>
	/// Exception thrown by <see cref="Query"/> and <see cref="Expression{T}"/> to indicate the query
	/// or expression could not be compiled. Details are contained in the <see cref="CompilationErrors"/> property.
	/// </summary>
	[Serializable]
	public sealed class CompilationException : NQueryException
	{
		private CompilationErrorCollection _compilationErrors;

		/// <summary>
		/// Initializes a new instance of the <see cref="CompilationException"/> class.
		/// </summary>
		public CompilationException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CompilationException"/> class with a specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public CompilationException(string message)
			: base(message)
		{
		}

		internal CompilationException(string message, IList<CompilationError> errors)
			: base(message)
		{
			if (errors == null)
				throw ExceptionBuilder.ArgumentNull("errors");

			_compilationErrors = new CompilationErrorCollection(errors);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CompilationException"/> class with a specified error message and a 
		/// reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException"/>
		/// parameter is not <see langword="null"/>, the current exception is raised in a catch block that handles the inner exception.</param>
		public CompilationException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CompilationException"/> class with serialized data.  
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context"> The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
		/// <exception name="ArgumentNullException">The <paramref name="info"/> parameter is <see langword="null"/>.</exception>
		/// <exception name="SerializationException">The class name is <see langword="null"/> or <see cref="Exception.HResult"/> is zero (<c>0</c>).</exception>
		private CompilationException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_compilationErrors = (CompilationErrorCollection) info.GetValue("CompilationErrors", typeof(CompilationErrorCollection));
		}

		/// <summary>
		/// Gets a read-only collection with the compile-time errors.
		/// </summary>
		public CompilationErrorCollection CompilationErrors
		{
			get { return _compilationErrors; }
		}

		/// <summary>
		/// Sets the <see cref="SerializationInfo"/> with information about the exception. 
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context"> The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="info"/> parameter is <see langword="null"/>.</exception>
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw ExceptionBuilder.ArgumentNull("info");

			info.AddValue("CompilationErrors", _compilationErrors, typeof(CompilationErrorCollection));
			base.GetObjectData(info, context);
		}
	}
}