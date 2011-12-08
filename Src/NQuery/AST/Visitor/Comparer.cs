using System;
using System.Diagnostics.CodeAnalysis;

namespace NQuery.Compilation
{
	internal static class Comparer
	{
		#region Dispatcher

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public static bool Visit(AstNode node1, AstNode node2)
		{
			switch (node1.NodeType)
			{
				case AstNodeType.Literal:
					return VisitLiteralValue((LiteralExpression)node1, node2 as LiteralExpression);

				case AstNodeType.UnaryExpression:
					return VisitUnaryExpression((UnaryExpression)node1, node2 as UnaryExpression);
				case AstNodeType.BinaryExpression:
					return VisitBinaryExpression((BinaryExpression)node1, node2 as BinaryExpression);
				case AstNodeType.BetweenExpression:
					return VisitBetweenExpression((BetweenExpression)node1, node2 as BetweenExpression);
				case AstNodeType.IsNullExpression:
					return VisitIsNullExpression((IsNullExpression)node1, node2 as IsNullExpression);
				case AstNodeType.CastExpression:
					return VisitCastExpression((CastExpression)node1, node2 as CastExpression);
				case AstNodeType.CaseExpression:
					return VisitCaseExpression((CaseExpression)node1, node2 as CaseExpression);
				case AstNodeType.CoalesceExpression:
					return VisitCoalesceExpression((CoalesceExpression)node1, node2 as CoalesceExpression);
				case AstNodeType.NullIfExpression:
					return VisitNullIfExpression((NullIfExpression)node1, node2 as NullIfExpression);
				case AstNodeType.InExpression:
					return VisitInExpression((InExpression)node1, node2 as InExpression);

				case AstNodeType.NamedConstantExpression:
					return VisitNamedConstantExpression((NamedConstantExpression)node1, node2 as NamedConstantExpression);
				case AstNodeType.ParameterExpression:
					return VisitParameterExpression((ParameterExpression)node1, node2 as ParameterExpression);
				case AstNodeType.NameExpression:
					return VisitNameExpression((NameExpression)node1, node2 as NameExpression);
				case AstNodeType.PropertyAccessExpression:
					return VisitPropertyAccessExpression((PropertyAccessExpression)node1, node2 as PropertyAccessExpression);
				case AstNodeType.FunctionInvocationExpression:
					return VisitFunctionInvocationExpression((FunctionInvocationExpression)node1, node2 as FunctionInvocationExpression);
				case AstNodeType.MethodInvocationExpression:
					return VisitMethodInvocationExpression((MethodInvocationExpression)node1, node2 as MethodInvocationExpression);

				case AstNodeType.ColumnExpression:
					return VisitColumnExpression((ColumnExpression)node1, node2 as ColumnExpression);
				case AstNodeType.RowBufferEntryExpression:
					return VisitRowBufferExpression((RowBufferEntryExpression)node1, node2 as RowBufferEntryExpression);
				case AstNodeType.AggregateExpression:
					return VisitAggregagateExpression((AggregateExpression)node1, node2 as AggregateExpression);
				case AstNodeType.SingleRowSubselect:
					return VisitSingleRowSubselect((SingleRowSubselect)node1, node2 as SingleRowSubselect);
				case AstNodeType.ExistsSubselect:
					return VisitExistsSubselect((ExistsSubselect)node1, node2 as ExistsSubselect);
				case AstNodeType.AllAnySubselect:
					return VisitAllAnySubselect((AllAnySubselect)node1, node2 as AllAnySubselect);

				case AstNodeType.NamedTableReference:
					return VisitNamedTableReference((NamedTableReference)node1, node2 as NamedTableReference);
				case AstNodeType.JoinedTableReference:
					return VisitJoinedTableReference((JoinedTableReference)node1, node2 as JoinedTableReference);
				case AstNodeType.DerivedTableReference:
					return VisitDerivedTableReference((DerivedTableReference)node1, node2 as DerivedTableReference);

				case AstNodeType.SelectQuery:
					return VisitSelectQuery((SelectQuery)node1, node2 as SelectQuery);
				case AstNodeType.BinaryQuery:
					return VisitBinaryQuery((BinaryQuery)node1, node2 as BinaryQuery);
				case AstNodeType.SortedQuery:
					return VisitSortedQuery((SortedQuery)node1, node2 as SortedQuery);
				case AstNodeType.CommonTableExpressionQuery:
					return VisitCommonTableExpressionQuery((CommonTableExpressionQuery)node1, node2 as CommonTableExpressionQuery);

				default:
					throw ExceptionBuilder.UnhandledCaseLabel(node1.NodeType);
			}
		}

		#endregion

		#region Expressions

		#region Literals

		private static bool VisitLiteralValue(LiteralExpression node1, LiteralExpression node2)
		{
			return node2 != null &&
			       Equals(node1.GetValue(), node2.GetValue());
		}

		#endregion

		#region Compound Expressions

		private static bool VisitUnaryExpression(UnaryExpression node1, UnaryExpression node2)
		{
			return node2 != null &&
			       node1.Op == node2.Op &&
			       Visit(node1.Operand, node2.Operand);
		}

		private static bool VisitBinaryExpression(BinaryExpression node1, BinaryExpression node2)
		{
			return node2 != null &&
			       node1.Op == node2.Op &&
			       Visit(node1.Left, node2.Left) && Visit(node1.Right, node2.Right);
		}

		private static bool VisitBetweenExpression(BetweenExpression node1, BetweenExpression node2)
		{
			return node2 != null &&
			       Visit(node1.Expression, node2.Expression) &&
			       Visit(node1.LowerBound, node2.LowerBound) &&
			       Visit(node1.UpperBound, node2.UpperBound);
		}

		private static bool VisitIsNullExpression(IsNullExpression node1, IsNullExpression node2)
		{
			return node2 != null &&
				   node1.Negated == node2.Negated &&
				   Visit(node1.Expression, node2.Expression);
		}

		private static bool VisitCastExpression(CastExpression node1, CastExpression node2)
		{
			return node2 != null &&
				   node1.TypeReference.TypeName == node2.TypeReference.TypeName &&
				   Visit(node1.Expression, node2.Expression);
		}

		private static bool VisitCaseExpression(CaseExpression node1, CaseExpression node2)
		{
			if (node2 == null)
				return false;

			if (node1.InputExpression != null)
				if (!Visit(node1.InputExpression, node2.InputExpression))
					return false;

			if (node1.WhenExpressions.Length != node2.WhenExpressions.Length ||
				node1.ThenExpressions.Length != node2.ThenExpressions.Length)
				return false;

			for (int i = 0; i < node1.WhenExpressions.Length; i++)
				if (!Visit(node1.WhenExpressions[i], node2.WhenExpressions[i]))
					return false;

			for (int i = 0; i < node1.ThenExpressions.Length; i++)
				if (!Visit(node1.ThenExpressions[i], node2.ThenExpressions[i]))
					return false;

			if (node1.ElseExpression != null)
				if (!Visit(node1.ElseExpression, node2.ElseExpression))
					return false;

			return true;
		}

		private static bool VisitCoalesceExpression(CoalesceExpression node1, CoalesceExpression node2)
		{
			if (node2 == null)
				return false;

			if (node1.Expressions.Length != node2.Expressions.Length)
				return false;

			for (int i = 0; i < node1.Expressions.Length; i++)
				if (!Visit(node1.Expressions[i], node2.Expressions[i]))
					return false;

			return true;
		}

		private static bool VisitNullIfExpression(NullIfExpression node1, NullIfExpression node2)
		{
			return node2 != null &&
				   Visit(node1.LeftExpression, node2.LeftExpression) &&
				   Visit(node1.RightExpression, node2.RightExpression);
		}

		private static bool VisitInExpression(InExpression node1, InExpression node2)
		{
			if (node2 == null)
				return false;

			if (!Visit(node1.Left, node2.Left))
				return false;

			if (node1.RightExpressions.Length != node2.RightExpressions.Length)
				return false;

			for (int i = 0; i < node1.RightExpressions.Length; i++)
				if (!Visit(node1.RightExpressions[i], node2.RightExpressions[i]))
					return false;
		
			return true;
		}

		private static bool VisitSingleRowSubselect(SingleRowSubselect node1, SingleRowSubselect node2)
		{
			return node2 != null &&
			       Visit(node1.Query, node2.Query);
		}

		private static bool VisitExistsSubselect(ExistsSubselect node1, ExistsSubselect node2)
		{
			return node2 != null &&
				   node1.Negated == node2.Negated &&
				   Visit(node1.Query, node2.Query);
		}

		private static bool VisitAllAnySubselect(AllAnySubselect node1, AllAnySubselect node2)
		{
			return node2 != null &&
			       node1.Type == node2.Type &&
			       node1.Op == node2.Op &&
			       Visit(node1.Left, node2.Left) &&
			       Visit(node1.Query, node2.Query);
		}

		#endregion

		#region Referencing Expressions

		private static bool VisitNamedConstantExpression(NamedConstantExpression node1, NamedConstantExpression node2)
		{
			return node2 != null &&
			       node1.Constant == node2.Constant;
		}

		private static bool VisitParameterExpression(ParameterExpression node1, ParameterExpression node2)
		{
			return node2 != null &&
			       node1.Name == node2.Name;
		}

		private static bool VisitNameExpression(NameExpression node1, NameExpression node2)
		{
			return node2 != null &&
			       node1.Name == node2.Name;
		}

		private static bool VisitPropertyAccessExpression(PropertyAccessExpression node1, PropertyAccessExpression node2)
		{
			return node2 != null &&
			       node1.Name == node2.Name &&
			       Visit(node1.Target, node2.Target);
		}

		private static bool VisitFunctionInvocationExpression(FunctionInvocationExpression node1, FunctionInvocationExpression node2)
		{
			if (node2 == null)
				return false;

			if (node1.HasAsteriskModifier != node2.HasAsteriskModifier ||
				node1.Arguments.Length != node2.Arguments.Length ||
				node1.Name != node2.Name)
				return false;

			for (int i = 0; i < node1.Arguments.Length; i++)
				if (!Visit(node1.Arguments[i], node2.Arguments[i]))
					return false;

			return true;
		}

		private static bool VisitMethodInvocationExpression(MethodInvocationExpression node1, MethodInvocationExpression node2)
		{
			if (node2 == null)
				return false;
			
			if (node1.Arguments.Length != node2.Arguments.Length ||
				node1.Name != node2.Name)
				return false;

			if (!Visit(node1.Target, node2.Target))
				return false;

			for (int i = 0; i < node1.Arguments.Length; i++)
				if (!Visit(node1.Arguments[i], node2.Arguments[i]))
					return false;

			return true;
		}

		#endregion

		#region Query Expressions

		private static bool VisitNamedTableReference(NamedTableReference node1, NamedTableReference node2)
		{
			return node2 != null &&
			       node1.TableName == node2.TableName &&
			       node1.CorrelationName == node2.CorrelationName &&
			       node1.TableRefBinding == node2.TableRefBinding;
		}

		private static bool VisitJoinedTableReference(JoinedTableReference node1, JoinedTableReference node2)
		{
			if (node2 == null)
				return false;

			if (node1.JoinType != node2.JoinType ||
			    (node1.Condition == null) != (node2.Condition == null))
				return false;

			if (node1.Condition != null)
				if (!Visit(node1.Condition, node2.Condition))
					return false;

			if (!Visit(node1.Left, node2.Left))
				return false;

			if (!Visit(node1.Right, node2.Right))
				return false;

			return true;
		}

		private static bool VisitDerivedTableReference(DerivedTableReference node1, DerivedTableReference node2)
		{
			return node2 != null &&
			       node1.CorrelationName == node2.CorrelationName &&
			       node1.DerivedTableBinding == node2.DerivedTableBinding &&
			       Visit(node1.Query, node2.Query);
		}

		private static bool VisitColumnExpression(ColumnExpression node1, ColumnExpression node2)
		{
			return node2 != null &&
			       node1.Column == node2.Column;
		}

		private static bool VisitRowBufferExpression(RowBufferEntryExpression node1, RowBufferEntryExpression node2)
	    {
			return node2 != null &&
			       node1.RowBufferEntry == node2.RowBufferEntry;
        }

		private static bool VisitAggregagateExpression(AggregateExpression node1, AggregateExpression node2)
		{
			return node2 != null &&
			       node1.HasAsteriskModifier == node2.HasAsteriskModifier &&
			       node1.Aggregate == node2.Aggregate &&
			       Visit(node1.Argument, node2.Argument);
		}

		#endregion

		#endregion

		#region Query

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		private static bool VisitSelectQuery(SelectQuery node1, SelectQuery node2)
		{
			if (node2 == null)
				return false;

			if (node1.IsAggregated != node2.IsAggregated ||
				node1.IsDistinct != node2.IsDistinct ||
				node1.SelectColumns.Length != node2.SelectColumns.Length ||
				(node1.TableReferences == null) != (node2.TableReferences == null) ||
				(node1.TopClause == null) != (node2.TopClause == null) ||
				(node1.WhereClause == null) != (node2.WhereClause == null) ||
				(node1.GroupByColumns == null) != (node2.GroupByColumns == null) ||
				(node1.OrderByColumns == null) != (node2.OrderByColumns == null))
				return false;

			if (node1.TopClause != null)
				if (node1.TopClause.Value != node2.TopClause.Value)
					return false;

			for (int i = 0; i < node1.SelectColumns.Length; i++)
			{
				if (node1.SelectColumns[i].IsAsterisk != node2.SelectColumns[i].IsAsterisk ||
					node1.SelectColumns[i].Alias != node2.SelectColumns[i].Alias ||
					(node1.SelectColumns[i].Expression == null) != (node2.SelectColumns[i].Expression == null))
					return false;

				if (node1.SelectColumns[i].Expression != null)
					if (!Visit(node1.SelectColumns[i].Expression, node2.SelectColumns[i].Expression))
						return false;
			}

			if (node1.TableReferences != null)
				if (!Visit(node1.TableReferences, node2.TableReferences))
					return false;

			if (node1.WhereClause != null)
				if (!Visit(node1.WhereClause, node2.WhereClause))
					return false;

			if (node1.HavingClause != null)
				if (!Visit(node1.HavingClause, node2.HavingClause))
					return false;

			if (node1.GroupByColumns != null)
			{
				if (node1.GroupByColumns.Length != node2.GroupByColumns.Length)
					return false;

				for (int i = 0; i < node1.GroupByColumns.Length; i++)
					if (!Visit(node1.GroupByColumns[i], node2.GroupByColumns[i]))
						return false;
			}

			if (node1.OrderByColumns != null)
			{
				for (int i = 0; i < node1.OrderByColumns.Length; i++)
				{
					if (node1.OrderByColumns[i].SortOrder != node2.OrderByColumns[i].SortOrder)
						return false;

					if (!Visit(node1.OrderByColumns[i].Expression, node2.OrderByColumns[i].Expression))
						return false;
				}
			}

			return true;
		}

		private static bool VisitBinaryQuery(BinaryQuery node1, BinaryQuery node2)
		{
			return node2 != null &&
			       node1.Op == node2.Op &&
			       Visit(node1.Left, node2.Left) &&
			       Visit(node1.Right, node2.Right);
		}

		private static bool VisitSortedQuery(SortedQuery node1, SortedQuery node2)
		{
			if (node2 == null)
				return false;

			if ((node1.Input == null) != (node2.Input == null) ||
				(node1.OrderByColumns == null) != (node2.OrderByColumns == null) ||
				node1.OrderByColumns.Length != node2.OrderByColumns.Length)
				return false;

			if (node1.Input != null)
				if (!Visit(node1.Input, node2.Input))
					return false;

			if (node1.OrderByColumns != null)
			{
				for (int i = 0; i < node1.OrderByColumns.Length; i++)
				{
					if (node1.OrderByColumns[i].SortOrder != node2.OrderByColumns[i].SortOrder)
						return false;

					if (!Visit(node1.OrderByColumns[i].Expression, node2.OrderByColumns[i].Expression))
						return false;
				}
			}

			return true;
		}

		private static bool VisitCommonTableExpressionQuery(CommonTableExpressionQuery node1, CommonTableExpressionQuery node2)
		{
			if (node2 == null)
				return false;

			if ((node1.Input == null) != (node2.Input == null) ||
				(node1.CommonTableExpressions == null) != (node2.CommonTableExpressions == null) ||
				node1.CommonTableExpressions.Length != node2.CommonTableExpressions.Length)
				return false;

			if (node1.Input != null)
				if (!Visit(node1.Input, node2.Input))
					return false;

			if (node1.CommonTableExpressions != null)
			{
				for (int i = 0; i < node1.CommonTableExpressions.Length; i++)
				{
					if (node1.CommonTableExpressions[i].TableName != node2.CommonTableExpressions[i].TableName)
						return false;

					if (!Visit(node1.CommonTableExpressions[i].QueryDeclaration, node2.CommonTableExpressions[i].QueryDeclaration))
						return false;
				}
			}

			return true;
		}

		#endregion
	}
}