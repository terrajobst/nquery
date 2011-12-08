using System;

using NQuery.Runtime.ExecutionPlan;

namespace NQuery.Compilation
{
	internal static class ExpressionCompiler
	{
		private sealed class InterpretedRuntimeExpression : RuntimeExpression
		{
			private string _source;
			private ExpressionNode _expressionNode;

			public InterpretedRuntimeExpression(ExpressionNode expressionNode)
			{
				_expressionNode = expressionNode;
			}

			public override Type ExpressionType
			{
				get { return _expressionNode.ExpressionType; }
			}

			public override object GetValue()
			{
				return _expressionNode.GetValue();
			}

			protected override string Source
			{
				get
				{
					if (_source == null)
						_source = _expressionNode.GenerateSource();

					return _source;
				}
			}
		}

		private sealed class CompiledRuntimeExpression : RuntimeExpression
		{
			private string _source;
			private Type _expressionType;
			private CompiledExpressionDelegate _emittedMethod;
			private object[] _args;

			public CompiledRuntimeExpression(string source, Type expressionType, CompiledExpressionDelegate emittedMethod, object[] args)
			{
				_source = source;
				_expressionType = expressionType;
				_emittedMethod = emittedMethod;
				_args = args;
			}

			public override Type ExpressionType
			{
				get { return _expressionType; }
			}

			public override object GetValue()
			{
				return _emittedMethod(_args);
			}

			protected override string Source
			{
				get { return _source; }
			}
		}

		public static RuntimeExpression CreateCompiled(ExpressionNode expressionNode)
		{
			string expressionSource = expressionNode.GenerateSource();
			ILEmitContext ilEmitContext = new ILEmitContext(expressionSource);
			
			ILParameterRegisterer ilParameterRegisterer = new ILParameterRegisterer(ilEmitContext);
			ilParameterRegisterer.Visit(expressionNode);

			ILTranslator ilTranslator = new ILTranslator(ilEmitContext);
			ilTranslator.Visit(expressionNode);

			CompiledExpressionDelegate compiledExpressionDelegate = ilEmitContext.CreateDelegate();
			object[] arguments = ilEmitContext.GetArguments();

			return new CompiledRuntimeExpression(expressionSource, expressionNode.ExpressionType, compiledExpressionDelegate, arguments);
		}
		
		public static RuntimeExpression CreateInterpreded(ExpressionNode expressionNode)
		{
			return new InterpretedRuntimeExpression(expressionNode);
		}
	}
}