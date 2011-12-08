using System;
using System.Collections.Generic;

using NQuery.CodeAssistance;
using NQuery.Compilation;
using NQuery.Runtime.ExecutionPlan;

namespace NQuery
{
	/// <summary>
	/// Represents an evaluatable expresssion. The result is of type <typeparamref name="T"/> and can be obtained by <see cref="Evaluate"/>.
	/// </summary>
	/// <typeparam name="T">The type the expression should evaluate to. If the type cannot be determined at compile-time you can use 
	/// <see cref="Object"/> and set <see cref="TargetType"/> to an appropriate value.</typeparam>
	/// <example>
	/// <p>For an example see <a href="/Quick Start/ExpressionQuickStart.html">Expression Quick Start</a>.</p>
	/// </example>
	public sealed class Expression<T> : Evaluatable
	{
		private ErrorCollector _errorCollector = new ErrorCollector();
		private RuntimeExpression _runtimeExpression;
		private Type _targetType;
		private T _nullValue;

		/// <summary>
		/// Creates a new instance of <see cref="Expression{T}"/> with no <see cref="Evaluatable.Text"/> and an
		/// empty <see cref="DataContext"/>.
		/// </summary>
		public Expression()
		{
			_targetType = typeof(T);
		}
		
		/// <summary>
		/// Creates a new instance of <see cref="Expression{T}"/> with the given <see cref="DataContext"/> and
		/// no <see cref="Evaluatable.Text"/>.
		/// </summary>
		/// <param name="dataContext">The data context to set <see cref="DataContext"/> to.</param>
		public Expression(DataContext dataContext)
			: this(String.Empty, dataContext)
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="Expression{T}"/> with the given <see cref="Evaluatable.Text"/> and
		/// an empty <see cref="DataContext"/>.
		/// </summary>
		/// <param name="text">The text to set <see cref="Evaluatable.Text"/> to.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is <see langword="null"/>.</exception>
		public Expression(string text) 
			: base(text)
		{
			_targetType = typeof(T);
		}

		/// <summary>
		/// Creates a new instance of <see cref="Expression{T}"/> with the given <see cref="Evaluatable.Text"/>.
		/// and <see cref="DataContext"/>.
		/// </summary>
		/// <param name="text">The text to set <see cref="Evaluatable.Text"/> to.</param>
		/// <param name="dataContext">The data context to set <see cref="DataContext"/> to.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is <see langword="null"/>.</exception>
		public Expression(string text, DataContext dataContext) 
			: base(text, dataContext)
		{
			_targetType = typeof(T);
		}

		protected override void ClearCompiledState()
		{
			_errorCollector.Reset();
			_runtimeExpression = null;
		}

		private void EnsureCompiled()
		{
			if (_runtimeExpression == null)
				Compile();
		}

		private void Compile()
		{
			if (DataContext == null)
				throw ExceptionBuilder.PropertyNotInitialized("DataContext");
			
			if (Text == null || Text.Length == 0)
				throw ExceptionBuilder.PropertyNotInitialized("Text");
			
			// Compile expression
			
			ClearCompiledState();
			Compiler compiler = new Compiler(_errorCollector);
			ExpressionNode expressionNode = compiler.CompileExpression(Text, _targetType, Scope);
	
			if (_errorCollector.ErrorsSeen)
			{
				IList<CompilationError> errors = _errorCollector.GetErrors();
				OnCompilationFailed(new CompilationFailedEventArgs(errors));
				throw ExceptionBuilder.ExpressionCompilationFailed(errors);
			}

			OnCompilationSucceeded(EventArgs.Empty);
			_runtimeExpression = ExpressionCompiler.CreateCompiled(expressionNode);
			ILEmitContext.CompleteILCompilation();
		}

		/// <summary>
		/// Gets or sets the value returned when the expression evaluates to <c>NULL</c>.
		/// </summary>
		public T NullValue
		{
			get { return _nullValue;  }
			set { _nullValue = value; }
		}

		/// <summary>
		/// Gets or sets the target type of this expression.
		/// </summary>
		/// <remarks>
		/// <p>Normally, <see cref="TargetType"/> is the runtime type information of <typeparamref name="T"/>. If the type cannot be 
		/// determined at compile time one can use <c>Expression&lt;object&gt;</c> in conjunction with an appropiate setting
		/// for <see cref="TargetType"/>.</p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown when <typeparamref name="T"/> is not assignable from <paramref name="value"/>.</exception>
		public Type TargetType
		{
			get { return _targetType; }
			set
			{
				if (value == null)
					throw ExceptionBuilder.ArgumentNull("value");

				if (value != _targetType)
				{
					if (!typeof (T).IsAssignableFrom(value))
						throw ExceptionBuilder.TargetTypeMismatch("value", typeof(T), value);

					_targetType = value;
					InvalidateCompiledState();
				}
			}
		}

		/// <summary>
		/// Resolves the expression to get the <see cref="Type"/> it will evaluate to.
		/// </summary>
		/// <remarks>
		/// <p>Although the expression object is typed it is possible that the expression is known to return a more specific object. For 
		/// instance imagine this:</p>
		/// <code>
		/// // class Base : { }
		/// // class Derived : Base { }
		/// 
		/// Expression&lt;Base&gt; expr = new Expression&lt;Base&gt;();
		/// expr.Parameters.Add("MyDerived", typeof(Derived), new Derived());
		/// expr.Text = "MyDerived";
		/// </code>
		/// <p>In this case <c>expr.Resolve()</c> will return <c>typeof(Derived)</c>.</p>
		/// </remarks>
		/// <exception cref="CompilationException">Thrown when <see cref="Evaluatable.Text"/> could not be compiled as expression.</exception>
		public Type Resolve()
		{
			EnsureCompiled();
			return _runtimeExpression.ExpressionType;
		}

		/// <summary>
		/// Evaluates the expression and returns a value of type <typeparamref name="T"/> representing its value.
		/// </summary>
		/// <exception cref="CompilationException">Thrown when <see cref="Evaluatable.Text"/> could not be compiled as expression.</exception>
		/// <exception cref="RuntimeException">Thrown when an error during evaluation occured.</exception>
		public T Evaluate()
		{
			EnsureCompiled();

			object result = _runtimeExpression.GetValue();
			if (result == null)
				return _nullValue;

			return (T) result;
		}

		/// <summary>
		/// Gets a code assistance context provider for this expression.
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