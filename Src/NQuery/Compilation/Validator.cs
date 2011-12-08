using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class Validator : StandardVisitor
	{
		private IErrorReporter _errorReporter;
		private MetadataContext _metadataContext;
		private Stack<QueryScope> _queryScopeStack = new Stack<QueryScope>();
		private int _queryNestingLevel;
		private AggregateExpression _currentAggregateExpression;

		public Validator(IErrorReporter errorReporter, MetadataContext metadataContext)
		{
			_errorReporter = errorReporter;
			_metadataContext = metadataContext;
		}

		#region Query Scope Helpers
		
		public void PushQueryScope(QueryScope queryScope)
		{
			_queryScopeStack.Push(queryScope);
		}
		
		public void PopQueryScope()
		{
			_queryScopeStack.Pop();
		}
		
		public QueryScope CurrentQueryScope
		{
			get
			{
				if (_queryScopeStack.Count == 0)
					return null;
				
				return _queryScopeStack.Peek();
			}
		}
		
		#endregion
		
		#region Helpers

		private bool CheckIfTypeIsComparable(Type type)
		{
			if (type == typeof(DBNull))
				return true;
			
			// A type is sortable if
			//
			//   A comparer has been registered for it.
			//               -- or --
			//   It supports IComparable
			
			if (_metadataContext.Comparers.IsRegistered(type))
				return true;
			
			foreach (Type implementedInterface in type.GetInterfaces())
			{
				if (implementedInterface == typeof(IComparable))
					return true;
			}
			
			return false;
		}

		private void ValidateOrderByClause(IEnumerable<OrderByColumn> orderByColumns)
		{
			// Ensure that all ORDER BY datatypes are comparable.
			
			foreach (OrderByColumn orderByColumn in orderByColumns)
			{
				if (!CheckIfTypeIsComparable(orderByColumn.Expression.ExpressionType))
					_errorReporter.InvalidDataTypeInOrderBy(orderByColumn.Expression.ExpressionType);
			}
						
			// Ensure that no constant expression is in ORDER BY
			
			foreach (OrderByColumn orderByColumn in orderByColumns)
			{
				if (orderByColumn.Expression is ConstantExpression)
				{
					_errorReporter.ConstantExpressionInOrderBy();
					break;
				}
			}
		}

		private static bool GetAllColumnsAreInInput(IEnumerable<SelectColumn> inputSelectColumns, IEnumerable<OrderByColumn> orderByColumns)
		{
			bool allColumnsAreInSelect = true;

			foreach (OrderByColumn orderByColumn in orderByColumns)
			{
				bool inSelect = false;
					
				foreach (SelectColumn inputColumn in inputSelectColumns)
				{
					if (orderByColumn.Expression.IsStructuralEqualTo(inputColumn.Expression))
					{
						inSelect = true;
						break;
					}
				}
					
				if (!inSelect)
				{
					allColumnsAreInSelect = false;
					break;
				}
			}
			return allColumnsAreInSelect;
		}

		private void EnsureQueryHasOneColumnOnly(QueryNode query)
		{
			SelectColumn[] selectColumns = query.GetColumns();
			if (selectColumns.Length != 1)
				_errorReporter.TooManyExpressionsInSelectListOfSubquery();
		}

		private void EnsureQueryHasNoOrderByUnlessTopSpecified(QueryNode query)
		{
			SelectQuery selectQuery = query as SelectQuery;
			if (selectQuery == null)
				return;

			if (selectQuery.OrderByColumns != null && selectQuery.TopClause == null)
				_errorReporter.OrderByInvalidInSubqueryUnlessTopIsAlsoSpecified();
		}

		private void EnsureSubqueryNotNestedInAggregate()
		{
			if (_currentAggregateExpression != null)
				_errorReporter.AggregateCannotContainSubquery(_currentAggregateExpression);
		}

		#endregion

		public override ExpressionNode VisitAggregagateExpression(AggregateExpression expression)
		{
			// Check that all aggregated expressions appear only in queries.

			if (_queryNestingLevel == 0)
				_errorReporter.AggregateInvalidInCurrentContext(expression);

			// Check that all aggregated expressions do not contain other aggregated
			// expressions.

			if (_currentAggregateExpression != null)
			{
				// Oops, we found a nested aggregate.

				_errorReporter.AggregateCannotContainAggregate(_currentAggregateExpression, _currentAggregateExpression.Aggregate, expression.Aggregate);
				return base.VisitAggregagateExpression(expression);
			}

			// Validate all embedded expressions that check aggregation expressions.

			_currentAggregateExpression = expression;
			ExpressionNode result = base.VisitAggregagateExpression(expression);
			_currentAggregateExpression = null;

			return result;
		}

		public override ExpressionNode VisitExistsSubselect(ExistsSubselect expression)
		{
			EnsureSubqueryNotNestedInAggregate();
			base.VisitExistsSubselect(expression);
			EnsureQueryHasNoOrderByUnlessTopSpecified(expression.Query);
			return expression;
		}

		public override ExpressionNode VisitSingleRowSubselect(SingleRowSubselect expression)
		{
			EnsureSubqueryNotNestedInAggregate();
			base.VisitSingleRowSubselect(expression);
			EnsureQueryHasOneColumnOnly(expression.Query);
			EnsureQueryHasNoOrderByUnlessTopSpecified(expression.Query);
			return expression;
		}

		public override ExpressionNode VisitAllAnySubselect(AllAnySubselect expression)
		{
			EnsureSubqueryNotNestedInAggregate();
			base.VisitAllAnySubselect(expression);
			EnsureQueryHasOneColumnOnly(expression.Query);
			EnsureQueryHasNoOrderByUnlessTopSpecified(expression.Query);
			return expression;
		}

		public override TableReference VisitDerivedTableReference(DerivedTableReference node)
		{
			base.VisitDerivedTableReference(node);
			EnsureQueryHasNoOrderByUnlessTopSpecified(node.Query);
			return node;
		}

		public override QueryNode VisitSortedQuery(SortedQuery query)
		{
			_queryNestingLevel++;
			base.VisitSortedQuery(query);
			_queryNestingLevel--;

			ValidateOrderByClause(query.OrderByColumns);

			// Ensure that all ORDER BY expressions are present in the
			// input.
						
			if (query.Input is BinaryQuery)
			{
				bool allColumnsAreInInput = GetAllColumnsAreInInput(query.GetColumns(), query.OrderByColumns);
				if (!allColumnsAreInInput)
					_errorReporter.OrderByItemsMustBeInSelectListIfUnionSpecified();						
			}
			
			return query;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public override QueryNode VisitSelectQuery(SelectQuery query)
		{
			PushQueryScope(query.QueryScope);

			// Validate all embedded AST entries that need to check a query context.

			_queryNestingLevel++;
			QueryNode result = base.VisitSelectQuery(query);
			_queryNestingLevel--;

			// Validate DISTINCT

			if (query.IsDistinct)
			{
				// Ensure that all column sources are datatypes that are comparable.

				foreach (SelectColumn columnSource in query.SelectColumns)
				{
					if (!CheckIfTypeIsComparable(columnSource.Expression.ExpressionType))
						_errorReporter.InvalidDataTypeInSelectDistinct(columnSource.Expression.ExpressionType);
				}
			}

			// Validate TOP

			if (query.TopClause != null)
			{
				if (query.TopClause.WithTies && query.OrderByColumns == null)
					_errorReporter.TopWithTiesRequiresOrderBy();
			}

			// Ensure that all ORDER BY datatypes are comparable.

			if (query.OrderByColumns != null)
				ValidateOrderByClause(query.OrderByColumns);

			// Ensure that if both DISTINCT and ORDER BY are presents all expressions in ORDER BY are also part in SELECT.

			if (query.IsDistinct && query.OrderByColumns != null)
			{
				bool allColumnsAreInInput = GetAllColumnsAreInInput(query.SelectColumns, query.OrderByColumns);
				if (!allColumnsAreInInput)
					_errorReporter.OrderByItemsMustBeInSelectListIfDistinctSpecified();
			}

			// Validate GROUP BY and aggregation-only queries.

			if (query.GroupByColumns == null)
			{
				if (query.IsAggregated || query.HavingClause != null)
				{
					// No grouping applied but at least one aggregation function present. That
					// means we have an aggregation-only query.
					//
					// Check that all expressions in SELECT are either aggregated or do not
					// reference any column.

					foreach (SelectColumn columnSource in query.SelectColumns)
					{
						foreach (ColumnRefBinding referencedColumn in AstUtil.GetUngroupedAndUnaggregatedColumns(null, columnSource.Expression))
						{
							if (referencedColumn.Scope == query.QueryScope)
							{
								// The column is not an outer reference so this is an error.
								_errorReporter.SelectExpressionNotAggregatedAndNoGroupBy(referencedColumn);
							}
						}
					}

					// Check that all expressions in HAVING are either aggregated or do not
					// reference any column.

					if (query.HavingClause != null)
					{
						foreach (ColumnRefBinding referencedColumn in AstUtil.GetUngroupedAndUnaggregatedColumns(null, query.HavingClause))
						{
							if (referencedColumn.Scope == query.QueryScope)
							{
								// The column is not an outer reference so this is an error.
								_errorReporter.HavingExpressionNotAggregatedOrGrouped(referencedColumn);
							}
						}
					}

					// Check that all expressions in ORDER BY are either aggregated or do not
					// reference any column.

					if (query.OrderByColumns != null)
					{
						foreach (OrderByColumn orderByColumn in query.OrderByColumns)
						{
							foreach (ColumnRefBinding referencedColumn in AstUtil.GetUngroupedAndUnaggregatedColumns(null, orderByColumn.Expression))
							{
								if (referencedColumn.Scope == query.QueryScope)
								{
									// The column is not an outer reference so this is an error.
									_errorReporter.OrderByExpressionNotAggregatedAndNoGroupBy(referencedColumn);
								}
							}
						}
					}
				}
			}
			else
			{
				// Grouped query:
				//
				// 1. All expression in GROUP BY must have a datatype that is comparable.
				//				
				// 2. All expressions in GROUP BY must not be aggregated
				//
				// 3. All expressions in SELECT, ORDER BY, and HAVING must be aggregated, grouped or must not reference 
				//    columns.

				// Check that all GROUP BY expressions are not aggregated.

				foreach (ExpressionNode groupExpression in query.GroupByColumns)
				{
					if (!CheckIfTypeIsComparable(groupExpression.ExpressionType))
						_errorReporter.InvalidDataTypeInGroupBy(groupExpression.ExpressionType);

					MetaInfo metaInfo = AstUtil.GetMetaInfo(groupExpression);
					if (metaInfo.ColumnDependencies.Length == 0)
						_errorReporter.GroupByItemDoesNotReferenceAnyColumns();
				}

				// Check that all expressions in SELECT are either part of the GROUP BY or are referencing only those
				// columns that are part of the GROUP BY.

				foreach (SelectColumn columnSource in query.SelectColumns)
				{
					foreach (ColumnRefBinding referencedColumn in AstUtil.GetUngroupedAndUnaggregatedColumns(query.GroupByColumns, columnSource.Expression))
					{
						if (referencedColumn.Scope == query.QueryScope)
						{
							// The column is not an outer reference so this is an error.
							_errorReporter.SelectExpressionNotAggregatedOrGrouped(referencedColumn);
						}
					}
				}

				// Check that all expressions in HAVING are either part of the GROUP BY or are referencing only those
				// columns that are part of the GROUP BY.

				if (query.HavingClause != null)
				{
					foreach (ColumnRefBinding referencedColumn in AstUtil.GetUngroupedAndUnaggregatedColumns(query.GroupByColumns, query.HavingClause))
					{
						if (referencedColumn.Scope == query.QueryScope)
						{
							// The column is not an outer reference so this is an error.
							_errorReporter.HavingExpressionNotAggregatedOrGrouped(referencedColumn);
						}
					}
				}

				// Check that all expressions in the ORDER BY clause are either part of the GROUP BY or are 
				// referencing only those columns that are part of the GROUP BY.

				if (query.OrderByColumns != null)
				{
					foreach (OrderByColumn orderByColumn in query.OrderByColumns)
					{
						foreach (ColumnRefBinding referencedColumn in AstUtil.GetUngroupedAndUnaggregatedColumns(query.GroupByColumns, orderByColumn.Expression))
						{
							if (referencedColumn.Scope == query.QueryScope)
							{
								// The column is not an outer reference so this is an error.
								_errorReporter.OrderByExpressionNotAggregatedOrGrouped(referencedColumn);
							}
						}
					}
				}
			}
		
			PopQueryScope();
			return result;
		}

		public override QueryNode VisitBinaryQuery(BinaryQuery query)
		{
			base.VisitBinaryQuery (query);

			EnsureQueryHasNoOrderByUnlessTopSpecified(query.Left);
			EnsureQueryHasNoOrderByUnlessTopSpecified(query.Right);

			// Except for UNION ALL any binary operator requires that all
			// column data types are comparable.
			
			if (query.Op != BinaryQueryOperator.UnionAll)
			{
				foreach (SelectColumn columnSource in query.GetColumns())
				{
					if (!CheckIfTypeIsComparable(columnSource.Expression.ExpressionType))
						_errorReporter.InvalidDataTypeInUnion(columnSource.Expression.ExpressionType, query.Op);
				}
			}
			
			return query;
		}

		public override QueryNode VisitCommonTableExpressionQuery(CommonTableExpressionQuery query)
		{
			base.VisitCommonTableExpressionQuery(query);

			foreach (CommonTableExpression commonTableExpression in query.CommonTableExpressions)
			{
				EnsureQueryHasNoOrderByUnlessTopSpecified(commonTableExpression.QueryDeclaration);

				if (commonTableExpression.CommonTableBinding.IsRecursive)
				{
					foreach (QueryNode recursiveMember in commonTableExpression.CommonTableBinding.RecursiveMembers)
					{
						CommonTableExpressionRecursiveMemberChecker checker = new CommonTableExpressionRecursiveMemberChecker(commonTableExpression.TableName);
						checker.Visit(recursiveMember);

						if (checker.RecursiveReferenceInSubquery)
							_errorReporter.CteContainsRecursiveReferenceInSubquery(commonTableExpression.TableName);
						else if (checker.RecursiveReferences == 0)
							_errorReporter.CteContainsUnexpectedAnchorMember(commonTableExpression.TableName);
						else if (checker.RecursiveReferences > 1)
							_errorReporter.CteContainsMultipleRecursiveReferences(commonTableExpression.TableName);

						if (checker.ContainsUnion)
							_errorReporter.CteContainsUnion(commonTableExpression.TableName);

						if (checker.ContainsDisctinct)
							_errorReporter.CteContainsDistinct(commonTableExpression.TableName);

						if (checker.ContainsTop)
							_errorReporter.CteContainsTop(commonTableExpression.TableName);

						if (checker.ContainsOuterJoin)
							_errorReporter.CteContainsOuterJoin(commonTableExpression.TableName);

						if (checker.ContainsGroupByHavingOrAggregate)
							_errorReporter.CteContainsGroupByHavingOrAggregate(commonTableExpression.TableName);
					}
				}
			}

			return query;
		}
	}
}