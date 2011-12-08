using System;
using System.Collections.Generic;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class AggregateBinder : StandardVisitor
	{
		private class AggregateList
		{
			private List<AggregateExpression> _aggregateExpressions = new List<AggregateExpression>();

			public void Add(AggregateExpression aggregateExpression)
			{
				AggregateExpression matchingAggregateExpression = null;
				foreach (AggregateExpression existingAggregateExpression in _aggregateExpressions)
				{
					if (existingAggregateExpression.IsStructuralEqualTo(aggregateExpression))
					{
						matchingAggregateExpression = existingAggregateExpression;
						break;
					}
				}

				if (matchingAggregateExpression != null)
				{
					aggregateExpression.ValueDefinition = matchingAggregateExpression.ValueDefinition;
				}
				else
				{
					RowBufferEntry rowBufferEntry = new RowBufferEntry(aggregateExpression.Aggregator.ReturnType);

					AggregatedValueDefinition aggregatedValueDefinition = new AggregatedValueDefinition();
					aggregatedValueDefinition.Target = rowBufferEntry;
					aggregatedValueDefinition.Aggregate = aggregateExpression.Aggregate;
					aggregatedValueDefinition.Aggregator = aggregateExpression.Aggregator;
					aggregatedValueDefinition.Argument = aggregateExpression.Argument;
					aggregateExpression.ValueDefinition = aggregatedValueDefinition;
					_aggregateExpressions.Add(aggregateExpression);
				}
			}

			public ICollection<AggregateExpression> Values
			{
				get { return _aggregateExpressions; }
			}
		}

		private IErrorReporter _errorReporter;
		private Dictionary<QueryScope, AggregateList> _aggregateDependencies = new Dictionary<QueryScope, AggregateList>();
		private Stack<AggregateList> _unscopedAggregateExpressionStack = new Stack<AggregateList>();

		public AggregateBinder(IErrorReporter errorReporter)
		{
			_errorReporter = errorReporter;
		}

		#region Helpers

		private void AddAggregateDependency(QueryScope associatedScope, AggregateExpression aggregateExpression)
		{
			if (associatedScope == null)
			{
				AggregateList currentUnscopedAggregateList = _unscopedAggregateExpressionStack.Peek();
				currentUnscopedAggregateList.Add(aggregateExpression);
			}
			else
			{
				AggregateList associatedAggregateExpressions;
				if (!_aggregateDependencies.TryGetValue(associatedScope, out associatedAggregateExpressions))
				{
					associatedAggregateExpressions = new AggregateList();
					_aggregateDependencies.Add(associatedScope, associatedAggregateExpressions);
				}

				associatedAggregateExpressions.Add(aggregateExpression);
			}
		}

		private ICollection<AggregateExpression> GetAggregateDependencies(QueryScope associatedScope)
		{
			AggregateList associatedAggregateExpressions;
			if (_aggregateDependencies.TryGetValue(associatedScope, out associatedAggregateExpressions))
				return associatedAggregateExpressions.Values;

			return null;
		}

		private bool QueryHasAggregates(QueryScope queryScope)
		{
			if (_unscopedAggregateExpressionStack.Peek().Values.Count > 0)
				return true;

			AggregateList queryAggregateList;
			if (_aggregateDependencies.TryGetValue(queryScope, out queryAggregateList) &&
				queryAggregateList.Values.Count > 0)
				return true;

			return false;
		}

		#endregion

		public override ExpressionNode VisitAggregagateExpression(AggregateExpression expression)
		{
			MetaInfo metaInfo = AstUtil.GetMetaInfo(expression.Argument);

			// Find associated query scope and ensure the aggregate's argument does not mix 
			// tables from different query scopes.

			QueryScope associatedScope = null;
			foreach (TableRefBinding tableDependency in metaInfo.TableDependencies)
			{
				if (associatedScope == null)
					associatedScope = tableDependency.Scope;
				else if (associatedScope != tableDependency.Scope)
					_errorReporter.AggregateContainsColumnsFromDifferentQueries(expression.Argument);
			}

			// Enter aggregate dependency.

			AddAggregateDependency(associatedScope, expression);

			return expression;
		}

		public override QueryNode VisitSelectQuery(SelectQuery query)
		{
			_unscopedAggregateExpressionStack.Push(new AggregateList());

			// Visit FROM table references

			if (query.TableReferences != null)
				query.TableReferences = VisitTableReference(query.TableReferences);

			if (QueryHasAggregates(query.QueryScope))
			{
				_errorReporter.AggregateInOn();
				return query;
			}

			// Visit WHERE expression

			if (query.WhereClause != null)
				query.WhereClause = VisitExpression(query.WhereClause);

			if (QueryHasAggregates(query.QueryScope))
			{
				_errorReporter.AggregateInWhere();
				return query;
			}

			// Visit GROUP BY clause

			if (query.GroupByColumns != null)
			{
				for (int i = 0; i < query.GroupByColumns.Length; i++)
					query.GroupByColumns[i] = VisitExpression(query.GroupByColumns[i]);
			}

			if (QueryHasAggregates(query.QueryScope))
			{
				_errorReporter.AggregateInGroupBy();
				return query;
			}

			// Visit select list.

			for (int i = 0; i < query.SelectColumns.Length; i++)
				query.SelectColumns[i].Expression = VisitExpression(query.SelectColumns[i].Expression);

			// Visit having clause.

			if (query.HavingClause != null)
				query.HavingClause = VisitExpression(query.HavingClause);

			// Visit ORDER BY.

			if (query.OrderByColumns != null)
			{
				for (int i = 0; i < query.OrderByColumns.Length; i++)
					query.OrderByColumns[i].Expression = VisitExpression(query.OrderByColumns[i].Expression);
			}

			AggregateList unscopedAggregateList = _unscopedAggregateExpressionStack.Pop();
			ICollection<AggregateExpression> directAggregateDependencies = GetAggregateDependencies(query.QueryScope);

			List<AggregateExpression> aggregateDependencies = new List<AggregateExpression>();
			aggregateDependencies.AddRange(unscopedAggregateList.Values);
			if (directAggregateDependencies != null)
				aggregateDependencies.AddRange(directAggregateDependencies);

			query.AggregateDependencies = aggregateDependencies.ToArray();

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