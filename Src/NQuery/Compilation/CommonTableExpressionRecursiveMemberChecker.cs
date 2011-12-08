using System;

namespace NQuery.Compilation
{
	internal sealed class CommonTableExpressionRecursiveMemberChecker : StandardVisitor
	{
		private Identifier _commonTableName;
		private int _subqueryContextCounter;
		private int _recursiveReferences;
		private bool _recursiveReferenceInSubquery;
		private bool _containsDisctinct;
		private bool _containsTop;
		private bool _containsUnion;
		private bool _containsGroupByHavingOrAggregate;
		private bool _containsOuterJoin;

		public CommonTableExpressionRecursiveMemberChecker(Identifier commonTableName)
		{
			_commonTableName = commonTableName;
		}

		public int RecursiveReferences
		{
			get { return _recursiveReferences; }
		}

		public bool RecursiveReferenceInSubquery
		{
			get { return _recursiveReferenceInSubquery; }
		}

		public bool ContainsDisctinct
		{
			get { return _containsDisctinct; }
		}

		public bool ContainsTop
		{
			get { return _containsTop; }
		}

		public bool ContainsUnion
		{
			get { return _containsUnion; }
		}

		public bool ContainsGroupByHavingOrAggregate
		{
			get { return _containsGroupByHavingOrAggregate; }
		}

		public bool ContainsOuterJoin
		{
			get { return _containsOuterJoin; }
		}

		public override ExpressionNode VisitSingleRowSubselect(SingleRowSubselect expression)
		{
			_subqueryContextCounter++;
			try
			{
				return base.VisitSingleRowSubselect(expression);
			}
			finally
			{
				_subqueryContextCounter--;
			}
		}

		public override ExpressionNode VisitExistsSubselect(ExistsSubselect expression)
		{
			_subqueryContextCounter++;
			try
			{
				return base.VisitExistsSubselect(expression);
			}
			finally
			{
				_subqueryContextCounter--;
			}
		}

		public override ExpressionNode VisitAllAnySubselect(AllAnySubselect expression)
		{
			_subqueryContextCounter++;
			try
			{
				return base.VisitAllAnySubselect(expression);
			}
			finally
			{
				_subqueryContextCounter--;
			}
		}

		public override ExpressionNode VisitAggregagateExpression(AggregateExpression expression)
		{
			_containsGroupByHavingOrAggregate = true;
			return base.VisitAggregagateExpression(expression);
		}

		public override TableReference VisitNamedTableReference(NamedTableReference node)
		{
			if (node.TableName.Matches(_commonTableName))
			{
				if (_subqueryContextCounter > 0)
					_recursiveReferenceInSubquery = true;
				else
					_recursiveReferences++;
			}

			return node;
		}

		public override TableReference VisitJoinedTableReference(JoinedTableReference node)
		{
			if (node.JoinType != JoinType.Inner)
				_containsOuterJoin = true;

			return base.VisitJoinedTableReference(node);
		}

		public override QueryNode VisitBinaryQuery(BinaryQuery query)
		{
			if (query.Op == BinaryQueryOperator.Union)
				_containsUnion = true;

			return base.VisitBinaryQuery(query);
		}

		public override QueryNode VisitSelectQuery(SelectQuery query)
		{
			if (query.IsDistinct)
				_containsDisctinct = true;
			if (query.TopClause != null)
				_containsTop = true;
			if (query.GroupByColumns != null)
				_containsGroupByHavingOrAggregate = true;

			return base.VisitSelectQuery(query);
		}
	}
}