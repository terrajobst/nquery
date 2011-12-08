using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace NQuery
{
	/// <summary>
	/// Base class for an evaluatable element, such as <see cref="Query"/> or <see cref="Expression{T}"/>.
	/// </summary>
	public abstract class Evaluatable : Component
	{
		private string _text;
		private Scope _scope;
		private MetadataContext _metadataContext;

		/// <summary>
		/// Creates a new instance of <see cref="Evaluatable"/> with no <see cref="Text"/> and an
		/// empty <see cref="DataContext"/>.
		/// </summary>
		protected Evaluatable()
			: this(String.Empty)
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="Evaluatable"/> with the given <see cref="DataContext"/> and
		/// no <see cref="Text"/>.
		/// </summary>
		/// <param name="dataContext">The data context to set <see cref="DataContext"/> to.</param>
		protected Evaluatable(DataContext dataContext)
			: this(String.Empty, dataContext)
		{
		}
		
		/// <summary>
		/// Creates a new instance of <see cref="Evaluatable"/> with the given <see cref="Text"/> and
		/// an empty <see cref="DataContext"/>.
		/// </summary>
		/// <param name="text">The text to set <see cref="Text"/> to.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is <see langword="null"/>.</exception>
		protected Evaluatable(string text)
			: this(text, new DataContext())
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="Evaluatable"/> with the given <see cref="Text"/>.
		/// and <see cref="DataContext"/>.
		/// </summary>
		/// <param name="text">The text to set <see cref="Text"/> to.</param>
		/// <param name="dataContext">The data context to set <see cref="DataContext"/> to.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is <see langword="null"/>.</exception>
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected Evaluatable(string text, DataContext dataContext)
		{
			if (text == null)
				throw ExceptionBuilder.ArgumentNull("text");
			
			_scope = new Scope();
			_scope.Parameters.Changed += parameters_Changed;
			
			_text = text;
			DataContext = dataContext;
		}

		protected virtual void ClearCompiledState()
		{
		}

		protected void InvalidateCompiledState()
		{
			OnCompiledStateInvalidated(EventArgs.Empty);
			ClearCompiledState();
		}

		protected virtual void OnDataContextChanged(EventArgs args)
		{
			EventHandler<EventArgs> handler = DataContextChanged;
			if (handler != null)
				handler.Invoke(this, args);

			InvalidateCompiledState();
		}

		protected virtual void OnTextChanged(EventArgs e)
		{
			EventHandler<EventArgs> handler = TextChanged;
			if (handler != null)
				handler.Invoke(this, e);

			InvalidateCompiledState();
		}

		protected virtual void OnCompilationFailed(CompilationFailedEventArgs args)
		{
			EventHandler<CompilationFailedEventArgs> handler = CompilationFailed;
			if (handler != null)
				handler.Invoke(this, args);
		}

		protected virtual void OnCompilationSucceeded(EventArgs args)
		{
			EventHandler<EventArgs> handler = CompilationSucceeded;
			if (handler != null)
				handler.Invoke(this, args);
		}

		protected virtual void OnCompiledStateInvalidated(EventArgs args)
		{
			EventHandler<EventArgs> handler = CompiledStateInvalidated;
			if (handler != null)
				handler.Invoke(this, args);
		}

		private void metadataContext_Changed(object sender, EventArgs e)
		{
			InvalidateCompiledState();
		}

		private void dataContext_OnChanged(object sender, EventArgs e)
		{
			InvalidateCompiledState();
		}

		private void parameters_Changed(object sender, EventArgs e)
		{
			InvalidateCompiledState();
		}

		private void dataContext_metadataContext_Changed(object sender, EventArgs e)
		{
			if (_metadataContext != null)
				_metadataContext.Changed -= metadataContext_Changed;

			InvalidateCompiledState();

			if (_scope.DataContext.MetadataContext != null)
			{
				_metadataContext = _scope.DataContext.MetadataContext;
				_metadataContext.Changed += metadataContext_Changed;
			}
		}

		internal Scope Scope
		{
			get { return _scope; }
		}

		/// <summary>
		/// Gets or sets the data context of this evaluatable.
		/// </summary>
		public DataContext DataContext
		{
			get { return _scope.DataContext; }
			set
			{
				if (_scope.DataContext != null)
				{
					_scope.DataContext.Changed -= dataContext_OnChanged;
					_scope.DataContext.MetadataContextChanged -= dataContext_metadataContext_Changed;
					_scope.DataContext.MetadataContext.Changed -= metadataContext_Changed;
					_metadataContext = null;
				}

				_scope.DataContext = value;

				if (_scope.DataContext != null)
				{
					_scope.DataContext.Changed += dataContext_OnChanged;
					_scope.DataContext.MetadataContextChanged += dataContext_metadataContext_Changed;
					_scope.DataContext.MetadataContext.Changed += metadataContext_Changed;

					_metadataContext = _scope.DataContext.MetadataContext;
				}

				OnDataContextChanged(EventArgs.Empty);
			}
		}

		/// <summary>
		/// Gets the parameters of this evaluatable.
		/// </summary>
		public ParameterCollection Parameters
		{
			get { return _scope.Parameters; }
		}

		/// <summary>
		/// Gets ot sets the text of this evaluatable.
		/// </summary>
		public string Text
		{
			get { return _text; } 
			set
			{
				if (_text != value)
				{
					_text = value;
					OnTextChanged(EventArgs.Empty);
				}
			}
		}

		/// <summary>
		/// Gets a code assistance context provider for this evaluatble.
		/// </summary>
		/// <remarks>
		/// For a conceptual overview of code assistance see <a href="/Overview/CodeAssistance.html">Code Assistance</a>.
		/// </remarks>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public abstract ICodeAssistanceContextProvider GetCodeAssistanceContextProvider();
		
		/// <summary>
		/// Raised when either the value of the property <see cref="Text"/> has been changed.
		/// </summary>
		public event EventHandler<EventArgs> TextChanged;

		/// <summary>
		/// Raised when either the value of the property <see cref="DataContext"/> has been changed.
		/// </summary>
		public event EventHandler<EventArgs> DataContextChanged;

		/// <summary>
		/// Raised when the compilation of this evaluatable failed.
		/// </summary>
		public event EventHandler<CompilationFailedEventArgs> CompilationFailed;

		/// <summary>
		/// Raised when the compilation of this evaluatable completed successfully.
		/// </summary>
		public event EventHandler<EventArgs> CompilationSucceeded;

		/// <summary>
		/// Raised when the compiled state of this evaluatable becomes invalid.
		/// </summary>
		public event EventHandler<EventArgs> CompiledStateInvalidated;
	}
}