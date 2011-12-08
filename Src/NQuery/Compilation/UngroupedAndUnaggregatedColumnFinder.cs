using System;
using System.Collections.Generic;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class UngroupedAndUnaggregatedColumnFinder : StandardVisitor
	{
        private ExpressionNode[] _groupByExpressions;
		private List<ColumnRefBinding> _ungroupedColumnList = new List<ColumnRefBinding>();
		private TableRefBinding[] _groupedTableRefs;

		public UngroupedAndUnaggregatedColumnFinder(ExpressionNode[] groupByExpressions)
		{
			_groupByExpressions = groupByExpressions;
			
			// Special handling for the case the user entered something like this:
			//
			// SELECT	*
			// FROM		Employees e
			// GROUP	BY e
			//
			// In this case, all columns from e are implicitly grouped.

			if (_groupByExpressions == null)
			{
				_groupedTableRefs = new TableRefBinding[0];
			}
			else
			{
                List<TableRefBinding> groupedTableRefList = new List<TableRefBinding>();

                foreach (ExpressionNode groupByExpression in _groupByExpressions)
				{
					ColumnExpression exprAsColumn = groupByExpression as ColumnExpression;

					if (exprAsColumn != null && exprAsColumn.Column.ColumnBinding is RowColumnBinding)
					{
						TableRefBinding groupedTableRef = exprAsColumn.Column.TableRefBinding;
						if (!groupedTableRefList.Contains(groupedTableRef))
							groupedTableRefList.Add(groupedTableRef);
					}
				}
				
				_groupedTableRefs = groupedTableRefList.ToArray();
			}
		}

        public ColumnRefBinding[] GetColumns()
        {
            return _ungroupedColumnList.ToArray();
        }

		public override AstNode Visit(AstNode node)
		{
			if (_groupByExpressions != null)
			{
				// First we check if the expression is directly contained the in GROUP BY.
				// In this case we don't traverse the children.
				//
				// This way we get only those columns which are not contained in GROUP BY.

				ExpressionNode nodeAsExpr = node as ExpressionNode;

				if (nodeAsExpr != null)
				{
					foreach (ExpressionNode groupyByExpression in _groupByExpressions)
					{
						if (groupyByExpression.IsStructuralEqualTo(nodeAsExpr))
							return node;
					}
				}
			}

			return base.Visit (node);
		}

		public override ExpressionNode VisitAggregagateExpression(AggregateExpression expression)
		{
			// We don't visit the arguments of aggregation nodes. This way
			// we get only columns that are not contained in an aggregate.

			return expression;
		}
	
		public override ExpressionNode VisitColumnExpression(ColumnExpression expression)
		{
			if (!ArrayHelpers.Contains(_groupedTableRefs, expression.Column.TableRefBinding))
			{
				// The column's table does not belong to the grouped table list.
				// Therfore it is an ungrouped column.
				_ungroupedColumnList.Add(expression.Column);
			}
			
			return base.VisitColumnExpression (expression);
		}

        // NOTE: Since we are only interetested in table references of the root query
        //       we don't visit nested queries.

        public override TableReference VisitDerivedTableReference(DerivedTableReference node)
        {
            return node;
        }

        public override ExpressionNode VisitSingleRowSubselect(SingleRowSubselect expression)
        {
            return expression;
        }

        public override ExpressionNode VisitExistsSubselect(ExistsSubselect expression)
        {
            return expression;
        }

        public override ExpressionNode VisitAllAnySubselect(AllAnySubselect expression)
        {
            return expression;
        }
	}
}