using System;
using System.Diagnostics.CodeAnalysis;

namespace NQuery.Compilation
{
	internal abstract class StandardVisitor
	{
		protected StandardVisitor()
		{
		}

		#region Dispatcher

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public virtual AstNode Visit(AstNode node)
		{
			switch (node.NodeType)
			{
				case AstNodeType.Literal:
					return VisitLiteralValue((LiteralExpression)node);

				case AstNodeType.UnaryExpression:
					return VisitUnaryExpression((UnaryExpression)node);
				case AstNodeType.BinaryExpression:
					return VisitBinaryExpression((BinaryExpression)node);
				case AstNodeType.BetweenExpression:
					return VisitBetweenExpression((BetweenExpression)node);
				case AstNodeType.IsNullExpression:
					return VisitIsNullExpression((IsNullExpression)node);
				case AstNodeType.CastExpression:
					return VisitCastExpression((CastExpression)node);
				case AstNodeType.CaseExpression:
					return VisitCaseExpression((CaseExpression)node);
				case AstNodeType.CoalesceExpression:
					return VisitCoalesceExpression((CoalesceExpression)node);
				case AstNodeType.NullIfExpression:
					return VisitNullIfExpression((NullIfExpression)node);
				case AstNodeType.InExpression:
					return VisitInExpression((InExpression)node);

				case AstNodeType.NamedConstantExpression:
					return VisitNamedConstantExpression((NamedConstantExpression)node);
				case AstNodeType.ParameterExpression:
					return VisitParameterExpression((ParameterExpression)node);
				case AstNodeType.NameExpression:
					return VisitNameExpression((NameExpression)node);
				case AstNodeType.PropertyAccessExpression:
					return VisitPropertyAccessExpression((PropertyAccessExpression)node);
				case AstNodeType.FunctionInvocationExpression:
					return VisitFunctionInvocationExpression((FunctionInvocationExpression)node);
				case AstNodeType.MethodInvocationExpression:
					return VisitMethodInvocationExpression((MethodInvocationExpression)node);

				case AstNodeType.ColumnExpression:
					return VisitColumnExpression((ColumnExpression)node);
				case AstNodeType.RowBufferEntryExpression:
					return VisitRowBufferEntryExpression((RowBufferEntryExpression)node);
				case AstNodeType.AggregateExpression:
					return VisitAggregagateExpression((AggregateExpression)node);
				case AstNodeType.SingleRowSubselect:
					return VisitSingleRowSubselect((SingleRowSubselect)node);
				case AstNodeType.ExistsSubselect:
					return VisitExistsSubselect((ExistsSubselect)node);
				case AstNodeType.AllAnySubselect:
					return VisitAllAnySubselect((AllAnySubselect)node);

				case AstNodeType.NamedTableReference:
					return VisitNamedTableReference((NamedTableReference)node);
				case AstNodeType.JoinedTableReference:
					return VisitJoinedTableReference((JoinedTableReference)node);
				case AstNodeType.DerivedTableReference:
					return VisitDerivedTableReference((DerivedTableReference)node);

				case AstNodeType.SelectQuery:
					return VisitSelectQuery((SelectQuery)node);
				case AstNodeType.BinaryQuery:
					return VisitBinaryQuery((BinaryQuery)node);
				case AstNodeType.SortedQuery:
					return VisitSortedQuery((SortedQuery)node);
				case AstNodeType.CommonTableExpressionQuery:
					return VisitCommonTableExpressionQuery((CommonTableExpressionQuery)node);

				case AstNodeType.TableAlgebraNode:
					return VisitTableAlgebraNode((TableAlgebraNode)node);
				case AstNodeType.JoinAlgebraNode:
					return VisitJoinAlgebraNode((JoinAlgebraNode)node);
				case AstNodeType.ConstantScanAlgebraNode:
					return VisitConstantScanAlgebraNode((ConstantScanAlgebraNode)node);
				case AstNodeType.NullScanAlgebraNode:
					return VisitNullScanAlgebraNode((NullScanAlgebraNode)node);
				case AstNodeType.ConcatAlgebraNode:
					return VisitConcatAlgebraNode((ConcatAlgebraNode)node);
				case AstNodeType.SortAlgebraNode:
					return VisitSortAlgebraNode((SortAlgebraNode)node);
				case AstNodeType.AggregateAlgebraNode:
					return VisitAggregateAlgebraNode((AggregateAlgebraNode)node);
				case AstNodeType.TopAlgebraNode:
					return VisitTopAlgebraNode((TopAlgebraNode)node);
				case AstNodeType.FilterAlgebraNode:
					return VisitFilterAlgebraNode((FilterAlgebraNode)node);
				case AstNodeType.ComputeScalarAlgebraNode:
					return VisitComputeScalarAlgebraNode((ComputeScalarAlgebraNode)node);
				case AstNodeType.ResultAlgebraNode:
					return VisitResultAlgebraNode((ResultAlgebraNode)node);
				case AstNodeType.AssertAlgebraNode:
					return VisitAssertAlgebraNode((AssertAlgebraNode)node);
				case AstNodeType.IndexSpoolAlgebraNode:
					return VisitIndexSpoolAlgebraNode((IndexSpoolAlgebraNode)node);
				case AstNodeType.StackedTableSpoolAlgebraNode:
					return VisitStackedTableSpoolAlgebraNode((StackedTableSpoolAlgebraNode)node);
				case AstNodeType.StackedTableSpoolRefAlgebraNode:
					return VisitTableSpoolRefAlgebraNode((StackedTableSpoolRefAlgebraNode)node);
				case AstNodeType.HashMatchAlgebraNode:
					return VisitHashMatchAlgebraNode((HashMatchAlgebraNode)node);

				default:
					throw ExceptionBuilder.UnhandledCaseLabel(node.NodeType);
			}
		}

		#endregion

		#region Expressions

		public virtual ExpressionNode VisitExpression(ExpressionNode expression)
		{
			return (ExpressionNode)Visit(expression);
		}

		#region Literal Expression

		public virtual LiteralExpression VisitLiteralValue(LiteralExpression expression)
		{
			return expression;
		}

		#endregion

		#region Compound Expressions

		public virtual ExpressionNode VisitUnaryExpression(UnaryExpression expression)
		{
			expression.Operand = VisitExpression(expression.Operand);
			return expression;
		}

		public virtual ExpressionNode VisitBinaryExpression(BinaryExpression expression)
		{
			expression.Left = VisitExpression(expression.Left);
			expression.Right = VisitExpression(expression.Right);
			return expression;
		}

		public virtual ExpressionNode VisitBetweenExpression(BetweenExpression expression)
		{
			expression.Expression = VisitExpression(expression.Expression);
			expression.LowerBound = VisitExpression(expression.LowerBound);
			expression.UpperBound = VisitExpression(expression.UpperBound);
			return expression;
		}

		public virtual ExpressionNode VisitIsNullExpression(IsNullExpression expression)
		{
			expression.Expression = VisitExpression(expression.Expression);
			return expression;
		}

		public virtual ExpressionNode VisitCastExpression(CastExpression expression)
		{
			expression.Expression = VisitExpression(expression.Expression);
			return expression;
		}

		public virtual ExpressionNode VisitCaseExpression(CaseExpression expression)
		{
			if (expression.InputExpression != null)
				expression.InputExpression = VisitExpression(expression.InputExpression);

			for (int i = 0; i < expression.WhenExpressions.Length; i++)
				expression.WhenExpressions[i] = VisitExpression(expression.WhenExpressions[i]);

			for (int i = 0; i < expression.ThenExpressions.Length; i++)
				expression.ThenExpressions[i] = VisitExpression(expression.ThenExpressions[i]);

			if (expression.ElseExpression != null)
				expression.ElseExpression = VisitExpression(expression.ElseExpression);

			return expression;
		}

		public virtual ExpressionNode VisitCoalesceExpression(CoalesceExpression expression)
		{
			for (int i = 0; i < expression.Expressions.Length; i++)
				expression.Expressions[i] = VisitExpression(expression.Expressions[i]);

			return expression;
		}

		public virtual ExpressionNode VisitNullIfExpression(NullIfExpression expression)
		{
			expression.LeftExpression = VisitExpression(expression.LeftExpression);
			expression.RightExpression = VisitExpression(expression.RightExpression);
			return expression;
		}

		public virtual ExpressionNode VisitInExpression(InExpression expression)
		{
			for (int i = 0; i < expression.RightExpressions.Length; i++)
				expression.RightExpressions[i] = VisitExpression(expression.RightExpressions[i]);

			return expression;
		}


		#endregion

		#region Referencing Expression

		public virtual ExpressionNode VisitNamedConstantExpression(NamedConstantExpression expression)
		{
			return expression;
		}

		public virtual ExpressionNode VisitParameterExpression(ParameterExpression expression)
		{
			return expression;
		}

		public virtual ExpressionNode VisitNameExpression(NameExpression expression)
		{
			return expression;
		}

		public virtual ExpressionNode VisitPropertyAccessExpression(PropertyAccessExpression expression)
		{
			expression.Target = VisitExpression(expression.Target);
			return expression;
		}

		public virtual ExpressionNode VisitFunctionInvocationExpression(FunctionInvocationExpression expression)
		{
			for (int i = 0; i < expression.Arguments.Length; i++)
				expression.Arguments[i] = VisitExpression(expression.Arguments[i]);
			return expression;
		}

		public virtual ExpressionNode VisitMethodInvocationExpression(MethodInvocationExpression expression)
		{
			expression.Target = VisitExpression(expression.Target);
			for (int i = 0; i < expression.Arguments.Length; i++)
				expression.Arguments[i] = VisitExpression(expression.Arguments[i]);
			return expression;
		}

		#endregion

		#region Query Expressions

		public virtual ExpressionNode VisitColumnExpression(ColumnExpression expression)
		{
			return expression;
		}

		public virtual ExpressionNode VisitRowBufferEntryExpression(RowBufferEntryExpression expression)
		{
			return expression;
		}

		public virtual ExpressionNode VisitAggregagateExpression(AggregateExpression expression)
		{
			expression.Argument = VisitExpression(expression.Argument);
			return expression;
		}

		public virtual ExpressionNode VisitSingleRowSubselect(SingleRowSubselect expression)
		{
			expression.Query = VisitQuery(expression.Query);
			return expression;
		}

		public virtual ExpressionNode VisitExistsSubselect(ExistsSubselect expression)
		{
			expression.Query = VisitQuery(expression.Query);
			return expression;
		}

		public virtual ExpressionNode VisitAllAnySubselect(AllAnySubselect expression)
		{
			expression.Left = VisitExpression(expression.Left);
			expression.Query = VisitQuery(expression.Query);
			return expression;
		}

		#endregion

		#endregion

		#region Query

		public virtual TableReference VisitTableReference(TableReference node)
		{
			return (TableReference)Visit(node);
		}

		public virtual TableReference VisitNamedTableReference(NamedTableReference node)
		{
			return node;
		}

		public virtual TableReference VisitJoinedTableReference(JoinedTableReference node)
		{
			node.Left = VisitTableReference(node.Left);
			node.Right = VisitTableReference(node.Right);

			if (node.Condition != null)
				node.Condition = VisitExpression(node.Condition);
			return node;
		}

		public virtual TableReference VisitDerivedTableReference(DerivedTableReference node)
		{
			node.Query = VisitQuery(node.Query);
			return node;
		}

		public virtual QueryNode VisitQuery(QueryNode query)
		{
			return (QueryNode)Visit(query);
		}

		public virtual QueryNode VisitSelectQuery(SelectQuery query)
		{
			// Visit all column expressions

			for (int i = 0; i < query.SelectColumns.Length; i++)
			{
				if (query.SelectColumns[i].Expression != null)
					query.SelectColumns[i].Expression = VisitExpression(query.SelectColumns[i].Expression);
			}

			// Visit FROM table references

			if (query.TableReferences != null)
				query.TableReferences = VisitTableReference(query.TableReferences);

			// Visit WHERE expression

			if (query.WhereClause != null)
				query.WhereClause = VisitExpression(query.WhereClause);

			// Visit GROUP BY clause

			if (query.GroupByColumns != null)
			{
				for (int i = 0; i < query.GroupByColumns.Length; i++)
					query.GroupByColumns[i] = VisitExpression(query.GroupByColumns[i]);
			}

			if (query.HavingClause != null)
				query.HavingClause = VisitExpression(query.HavingClause);

			// Visit ORDER BY expressions

			if (query.OrderByColumns != null)
			{
				for (int i = 0; i < query.OrderByColumns.Length; i++)
					query.OrderByColumns[i].Expression = VisitExpression(query.OrderByColumns[i].Expression);
			}

			return query;
		}

		public virtual QueryNode VisitBinaryQuery(BinaryQuery query)
		{
			query.Left = VisitQuery(query.Left);
			query.Right = VisitQuery(query.Right);
			return query;
		}

		public virtual QueryNode VisitSortedQuery(SortedQuery query)
		{
			// Visit input
			query.Input = VisitQuery(query.Input);

			// Visit ORDER BY expressions

			if (query.OrderByColumns != null)
			{
				for (int i = 0; i < query.OrderByColumns.Length; i++)
					query.OrderByColumns[i].Expression = VisitExpression(query.OrderByColumns[i].Expression);
			}

			return query;
		}

		public virtual QueryNode VisitCommonTableExpressionQuery(CommonTableExpressionQuery query)
		{
			foreach (CommonTableExpression commonTableExpression in query.CommonTableExpressions)
				commonTableExpression.QueryDeclaration = VisitQuery(commonTableExpression.QueryDeclaration);

			query.Input = VisitQuery(query.Input);
			return query;
		}

		#endregion

		#region Algebra Nodes

		public virtual AlgebraNode VisitAlgebraNode(AlgebraNode node)
		{
			return (AlgebraNode)Visit(node);
		}

		public virtual AlgebraNode VisitTableAlgebraNode(TableAlgebraNode node)
		{
			return node;
		}

		public virtual AlgebraNode VisitJoinAlgebraNode(JoinAlgebraNode node)
		{
			node.Left = VisitAlgebraNode(node.Left);
			node.Right = VisitAlgebraNode(node.Right);

			if (node.Predicate != null)
				node.Predicate = VisitExpression(node.Predicate);

			if (node.PassthruPredicate != null)
				node.PassthruPredicate = VisitExpression(node.PassthruPredicate);

			return node;
		}

		public virtual AlgebraNode VisitConstantScanAlgebraNode(ConstantScanAlgebraNode node)
		{
			if (node.DefinedValues != null)
			{
				foreach (ComputedValueDefinition definedValue in node.DefinedValues)
					definedValue.Expression = VisitExpression(definedValue.Expression);
			}

			return node;
		}

		public virtual AlgebraNode VisitNullScanAlgebraNode(NullScanAlgebraNode node)
		{
			return node;
		}

		public virtual AlgebraNode VisitConcatAlgebraNode(ConcatAlgebraNode node)
		{
			for (int i = 0; i < node.Inputs.Length; i++)
				node.Inputs[i] = VisitAlgebraNode(node.Inputs[i]);

			return node;
		}

		public virtual AlgebraNode VisitSortAlgebraNode(SortAlgebraNode node)
		{
			node.Input = VisitAlgebraNode(node.Input);
			return node;
		}

		public virtual AlgebraNode VisitAggregateAlgebraNode(AggregateAlgebraNode node)
		{
			node.Input = VisitAlgebraNode(node.Input);

			if (node.DefinedValues != null)
			{
				foreach (AggregatedValueDefinition definedValue in node.DefinedValues)
					definedValue.Argument = VisitExpression(definedValue.Argument);
			}

			return node;
		}

		public virtual AlgebraNode VisitTopAlgebraNode(TopAlgebraNode node)
		{
			node.Input = VisitAlgebraNode(node.Input);
			return node;
		}

		public virtual AlgebraNode VisitFilterAlgebraNode(FilterAlgebraNode node)
		{
			node.Input = VisitAlgebraNode(node.Input);
			node.Predicate = VisitExpression(node.Predicate);

			return node;
		}

		public virtual AlgebraNode VisitComputeScalarAlgebraNode(ComputeScalarAlgebraNode node)
		{
			node.Input = VisitAlgebraNode(node.Input);

			if (node.DefinedValues != null)
			{
				foreach (ComputedValueDefinition definedValue in node.DefinedValues)
					definedValue.Expression = VisitExpression(definedValue.Expression);
			}

			return node;
		}

		public virtual AlgebraNode VisitResultAlgebraNode(ResultAlgebraNode node)
		{
			node.Input = VisitAlgebraNode(node.Input);
			return node;
		}

		public virtual AlgebraNode VisitAssertAlgebraNode(AssertAlgebraNode node)
		{
			node.Input = VisitAlgebraNode(node.Input);
			node.Predicate = VisitExpression(node.Predicate);
			return node;
		}

		public virtual AlgebraNode VisitIndexSpoolAlgebraNode(IndexSpoolAlgebraNode node)
		{
			node.Input = VisitAlgebraNode(node.Input);
			return node;
		}

		public virtual AstNode VisitStackedTableSpoolAlgebraNode(StackedTableSpoolAlgebraNode node)
		{
			node.Input = VisitAlgebraNode(node.Input);
			return node;
		}

		public virtual AstNode VisitTableSpoolRefAlgebraNode(StackedTableSpoolRefAlgebraNode node)
		{
			return node;
		}

		public virtual AlgebraNode VisitHashMatchAlgebraNode(HashMatchAlgebraNode node)
		{
			node.Left = VisitAlgebraNode(node.Left);
			node.Right = VisitAlgebraNode(node.Right);

			if (node.ProbeResidual != null)
				node.ProbeResidual = VisitExpression(node.ProbeResidual);

			return node;
		}

		#endregion
	}
}