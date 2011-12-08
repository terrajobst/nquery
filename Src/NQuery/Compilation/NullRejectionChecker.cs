using System;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	/// <summary>
	/// This visitor checks whether a given expression will always yield null or false if
	/// all columns of the given <see cref="TableRefBinding" /> are null.
	/// </summary>
	internal sealed class NullRejectionChecker : StandardVisitor
	{
		private TableRefBinding _nullableTableRefBinding;
		private RowBufferEntry _rowBufferEntry;
		private bool _lastExpressionsYieldsNullOrFalse;
		
		public NullRejectionChecker(TableRefBinding nullableTableRefBinding)
		{
			_nullableTableRefBinding = nullableTableRefBinding;
		}

		public NullRejectionChecker(RowBufferEntry rowBufferEntry)
		{
			_rowBufferEntry = rowBufferEntry;
		}

		public bool ExpressionRejectsNull
		{
			get { return _lastExpressionsYieldsNullOrFalse; }
		}

		public override AstNode Visit(AstNode node)
		{
			// Since we are going to visit the next node we must reset this flag.
			_lastExpressionsYieldsNullOrFalse = false;
			
			// Only for node types listed below we can predict if the expression will yield
			// null/false. For all other node types this is unknown. To avoid that children
			// of those nodes can set the _lastExpressionsYieldsNullOrFalse we don't visit them.
			switch (node.NodeType)
			{
				case AstNodeType.IsNullExpression:
				case AstNodeType.UnaryExpression:
				case AstNodeType.BinaryExpression:
				case AstNodeType.ColumnExpression:
				case AstNodeType.RowBufferEntryExpression:
				case AstNodeType.PropertyAccessExpression:
				case AstNodeType.MethodInvocationExpression:
					return base.Visit(node);									
			}
						
			return node;
		}

		public override ExpressionNode VisitIsNullExpression(IsNullExpression expression)
		{
			Visit(expression.Expression);
			
			if (_lastExpressionsYieldsNullOrFalse && !expression.Negated)
				_lastExpressionsYieldsNullOrFalse = false;
				
			return expression;
		}

		public override ExpressionNode VisitBinaryExpression(BinaryExpression expression)
		{
			if (expression.Op == BinaryOperator.LogicalOr)
			{
				// Special handling for logical OR:
				// For logical OR both arguments must be NULL to yield false/null.
				
				Visit(expression.Left);
				bool leftIsNullOrFalse = _lastExpressionsYieldsNullOrFalse;
				Visit(expression.Right);
				
				_lastExpressionsYieldsNullOrFalse = leftIsNullOrFalse && _lastExpressionsYieldsNullOrFalse;
			}
			else
			{
				// In all other cases we know the result will be false/null if
				// any operand is null.

				Visit(expression.Left);
				bool leftIsNullOrFalse = _lastExpressionsYieldsNullOrFalse;
				Visit(expression.Right);
				
				_lastExpressionsYieldsNullOrFalse = leftIsNullOrFalse || _lastExpressionsYieldsNullOrFalse;
			}
			
			return expression;
		}

		public override ExpressionNode VisitColumnExpression(ColumnExpression expression)
		{
			if (expression.Column.TableRefBinding == _nullableTableRefBinding)
				_lastExpressionsYieldsNullOrFalse = true;
			
			return expression;
		}

		public override ExpressionNode VisitRowBufferEntryExpression(RowBufferEntryExpression expression)
		{
			if (expression.RowBufferEntry == _rowBufferEntry)
				_lastExpressionsYieldsNullOrFalse = true;

			return expression;
		}

		public override ExpressionNode VisitMethodInvocationExpression(MethodInvocationExpression expression)
		{
			Visit(expression.Target);
			
			// Since arguments with null values don't answer the question whether the whole method invocation
			// is null we must not visit the arguments.			
			return expression;
		}
	}
}