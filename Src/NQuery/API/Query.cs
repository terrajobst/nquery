using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;

using NQuery.CodeAssistance;
using NQuery.Compilation;
using NQuery.Runtime.ExecutionPlan;

namespace NQuery
{
	/// <summary>
	/// Represents a <c>SELECT</c> query against a set of tables, functions, and parameters. The result is a <see cref="DataTable"/>
	/// that can be obtained by <see cref="ExecuteDataTable"/>.
	/// </summary>
	/// <example>
	/// <p>For an example see <a href="/Quick Start/QueryQuickStart.html">Query Quick Start</a>.</p>
	/// </example>
	public sealed class Query : Evaluatable
	{
		private ErrorCollector _errorCollector = new ErrorCollector();
		private ResultAlgebraNode _resultAlgebraNode;
		private ResultIterator _resultIterator;

		/// <summary>
		/// Creates a new instance of <see cref="Query"/> with no <see cref="Evaluatable.Text"/> and an
		/// empty <see cref="DataContext"/>.
		/// </summary>
		public Query() 
		{
		}
		
		/// <summary>
		/// Creates a new instance of <see cref="Query"/> with the given <see cref="DataContext"/> and
		/// no <see cref="Evaluatable.Text"/>.
		/// </summary>
		/// <param name="dataContext">The data context to set <see cref="DataContext"/> to.</param>
		public Query(DataContext dataContext)
			: this(String.Empty, dataContext)
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="Query"/> with the given <see cref="Evaluatable.Text"/> and
		/// an empty <see cref="DataContext"/>.
		/// </summary>
		/// <param name="text">The text to set <see cref="Evaluatable.Text"/> to.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is <see langword="null"/>.</exception>
		public Query(string text) 
			: base(text)
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="Query"/> with the given <see cref="Evaluatable.Text"/>.
		/// and <see cref="DataContext"/>.
		/// </summary>
		/// <param name="text">The text to set <see cref="Evaluatable.Text"/> to.</param>
		/// <param name="dataContext">The data context to set <see cref="DataContext"/> to.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is <see langword="null"/>.</exception>
		public Query(string text, DataContext dataContext) 
			: base(text, dataContext)
		{
		}

		protected override void ClearCompiledState()
		{
			_resultAlgebraNode = null;
			_resultIterator = null;
			_errorCollector.Reset();
		}

		private void EnsureCompiled()
		{
			if (_resultAlgebraNode == null)
				Compile();
		}

		private void Compile()
		{
			if (DataContext == null)
				throw ExceptionBuilder.PropertyNotInitialized("DataContext");
			
			if (Text == null || Text.Length == 0)
				throw ExceptionBuilder.PropertyNotInitialized("Text");
			
			// Compile query

			ClearCompiledState();
			Compiler compiler = new Compiler(_errorCollector);
			_resultAlgebraNode = compiler.CompileQuery(Text, Scope);
	
			if (_errorCollector.ErrorsSeen)
			{
				IList<CompilationError> errors = _errorCollector.GetErrors();
				OnCompilationFailed(new CompilationFailedEventArgs(errors));
				throw ExceptionBuilder.QueryCompilationFailed(errors);
			}

			OnCompilationSucceeded(EventArgs.Empty);
			_resultIterator = IteratorCreator.Convert(DataContext.MetadataContext, true, _resultAlgebraNode);
		}

		/// <summary>
		/// Returns the execution plan of the compiled query that shows which optimizations and joins have been created by the 
		/// query engine.
		/// </summary>
		/// <remarks>
		/// <p>The execution plan is a tree-like data structure. Each node represents a physical operation used to generate the
		/// result. To supply additional details each node also has properties indicating what columns are defined and returned 
		/// by the operation.</p>
		/// <p>It is provided for informational purposes only and is read-only.</p>
		/// </remarks>
		/// <exception cref="CompilationException">Thrown when <see cref="Evaluatable.Text"/> could not be compiled as query.</exception>
		// Since construction of a show plan might be an expensive operation we
		// prefer using a method instead of a property.
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public ShowPlan GetShowPlan()
		{
			EnsureCompiled();
			return ShowPlan.Build(_resultAlgebraNode);
		}
	    
        /// <summary>
		/// Executes the <c>SELECT</c> query specified in <see cref="Evaluatable.Text"/>.
		/// </summary>
		/// <exception cref="CompilationException">Thrown when <see cref="Evaluatable.Text"/> could not be compiled as query.</exception>
		/// <exception cref="RuntimeException">Thrown when an error during execution occured.</exception>
		public DataTable ExecuteDataTable()
		{
			EnsureCompiled();

			DataTable dataTable = _resultIterator.CreateSchemaTable();

			_resultIterator.Initialize();
			_resultIterator.Open();
			while (_resultIterator.Read())
				dataTable.Rows.Add(_resultIterator.RowBuffer);

			return dataTable;
		}

		/// <summary>
		/// Resolves the <c>SELECT</c> query specified in <see cref="Evaluatable.Text"/> and returns a <see cref="DataTable"/>.
		/// Since the query is not actually executed the returned <see cref="DataTable"/> can only be used to retreive schema 
		/// information.
		/// </summary>
		/// <exception cref="CompilationException">Thrown when <see cref="Evaluatable.Text"/> could not be compiled as query.</exception>
		public DataTable ExecuteSchemaDataTable()
		{
			EnsureCompiled();
			return _resultIterator.CreateSchemaTable();
		}

		/// <summary>
		/// Executes the <c>SELECT</c> query specified in <see cref="Evaluatable.Text"/> and returns the first value of
		/// the first row only.
		/// </summary>
		/// <exception cref="CompilationException">Thrown when <see cref="Evaluatable.Text"/> could not be compiled as query.</exception>
		/// <exception cref="RuntimeException">Thrown when an error during execution occured.</exception>
		public object ExecuteScalar()
		{
			EnsureCompiled();

			_resultIterator.Initialize();
			_resultIterator.Open();
			if (_resultIterator.Read() && _resultIterator.RowBuffer.Length > 0)
				return _resultIterator.RowBuffer[0];

			return null;
		}

		/// <summary>
		/// Executes the <c>SELECT</c> query specified in <see cref="Evaluatable.Text"/> and returns a <see cref="QueryDataReader"/>.
		/// </summary>
		/// <exception cref="CompilationException">Thrown when <see cref="Evaluatable.Text"/> could not be compiled as query.</exception>
		/// <exception cref="RuntimeException">Thrown when an error during execution occured.</exception>
		public QueryDataReader ExecuteDataReader()
		{
			EnsureCompiled();
			return new QueryDataReader(_resultIterator, false);
		}
				
		/// <summary>
		/// Resolves the <c>SELECT</c> query specified in <see cref="Evaluatable.Text"/> and returns a <see cref="QueryDataReader"/>.
		/// Since the query is not actually executed the returned <see cref="QueryDataReader"/> can only be used to retreive schema 
		/// information.
		/// </summary>
		/// <exception cref="CompilationException">Thrown when <see cref="Evaluatable.Text"/> could not be compiled as query.</exception>
		public QueryDataReader ExecuteSchemaReader()
		{
			EnsureCompiled();
			return new QueryDataReader(_resultIterator, true);
		}

		/// <summary>
		/// Gets a code assistance context provider for this query.
		/// </summary>
		/// <remarks>
		/// For a conceptual overview of code assistance see <a href="/Overview/CodeAssistance.html">Code Assistance</a>.
		/// </remarks>
		public override ICodeAssistanceContextProvider GetCodeAssistanceContextProvider()
		{
			return (new CodeAssistanceContextProvider(Scope, Text));
		}
	}
}