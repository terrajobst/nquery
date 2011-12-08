using System;

namespace NQuery.Compilation
{
	internal sealed class ColumnAndAggregateExpressionReplacer : StandardVisitor
	{
		public override ExpressionNode VisitColumnExpression(ColumnExpression expression)
		{
			RowBufferEntryExpression rowBufferExpression = new RowBufferEntryExpression();
			rowBufferExpression.RowBufferEntry = expression.Column.ValueDefinition.Target;
			return rowBufferExpression;
		}

		public override ExpressionNode VisitAggregagateExpression(AggregateExpression expression)
		{
			expression.Argument = VisitExpression(expression.Argument);

			expression.ValueDefinition.Argument = expression.Argument;

			RowBufferEntryExpression rowBufferExpression = new RowBufferEntryExpression();
			rowBufferExpression.RowBufferEntry = expression.ValueDefinition.Target;
			return rowBufferExpression;
		}

		public override QueryNode VisitSelectQuery(SelectQuery query)
		{
			base.VisitSelectQuery(query);

			RowBufferCalculator rowBufferCalculator = new RowBufferCalculator(query);
			rowBufferCalculator.Calculate();
			query.RowBufferCalculator = rowBufferCalculator;

			return query;
		}

		public override QueryNode VisitSortedQuery(SortedQuery query)
		{
			query.Input = VisitQuery(query.Input);

			// NOTE: We don't visit the ORDER BY columns since they can only refer to existing expressions
			//       in the output of the inner most query. Later phases will ensure this is true.

			return query;
		}
	}
}