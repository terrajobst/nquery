using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class RowBufferCalculator
	{
		private SelectQuery _selectQuery;

		private ComputedValueDefinition[] _computedGroupColumns;
		private RowBufferEntry[] _groupColumns;

		private ComputedValueDefinition[] _computedSelectAndOrderColumns;
		private RowBufferEntry[] _selectColumns;
		private RowBufferEntry[] _orderColumns;

		private class ComputedSubsequenceReplacer : StandardVisitor
		{
			private IEnumerable<ComputedValueDefinition> _computedValues;

			public ComputedSubsequenceReplacer(IEnumerable<ComputedValueDefinition> computedBufferedValues)
			{
				_computedValues = computedBufferedValues;
			}

			public override AstNode Visit(AstNode node)
			{
				ExpressionNode expression = node as ExpressionNode;
				if (expression != null)
				{
					foreach (ComputedValueDefinition computedBufferedValue in _computedValues)
					{
						if (expression.IsStructuralEqualTo(computedBufferedValue.Expression))
						{
							RowBufferEntryExpression rowBufferExpression = new RowBufferEntryExpression();
							rowBufferExpression.RowBufferEntry = computedBufferedValue.Target;
							return rowBufferExpression;
						}
					}
				}

				return base.Visit(node);
			}
		}

		public RowBufferCalculator(SelectQuery selectQuery)
		{
			_selectQuery = selectQuery;
		}

		public ComputedValueDefinition[] ComputedGroupColumns
		{
			get { return _computedGroupColumns; }
		}

		public RowBufferEntry[] GroupColumns
		{
			get { return _groupColumns; }
		}

		public ComputedValueDefinition[] ComputedSelectAndOrderColumns
		{
			get { return _computedSelectAndOrderColumns; }
		}

		public RowBufferEntry[] SelectColumns
		{
			get { return _selectColumns; }
		}

		public RowBufferEntry[] OrderColumns
		{
			get { return _orderColumns; }
		}

		public void Calculate()
		{
			if (_selectQuery.GroupByColumns != null)
			{
				List<ComputedValueDefinition> computedGroupColumnList = new List<ComputedValueDefinition>();
				List<RowBufferEntry> groupColumnList = new List<RowBufferEntry>();

				foreach (ExpressionNode groupByColumn in _selectQuery.GroupByColumns)
					CreateBufferedValue(groupByColumn, computedGroupColumnList, groupColumnList, null);

				_computedGroupColumns = computedGroupColumnList.ToArray();
				_groupColumns = groupColumnList.ToArray();
			}

			List<ComputedValueDefinition> computedSelectAndOrderColumnList = new List<ComputedValueDefinition>();
			List<RowBufferEntry> selectColumnList = new List<RowBufferEntry>();

			foreach (SelectColumn columnSource in _selectQuery.SelectColumns)
				CreateBufferedValue(columnSource.Expression, computedSelectAndOrderColumnList, selectColumnList, _computedGroupColumns);

			if (_selectQuery.OrderByColumns != null)
			{
				List<RowBufferEntry> orderColumnList = new List<RowBufferEntry>();

				foreach (OrderByColumn orderByColumn in _selectQuery.OrderByColumns)
					CreateBufferedValue(orderByColumn.Expression, computedSelectAndOrderColumnList, orderColumnList, _computedGroupColumns);

				_orderColumns = orderColumnList.ToArray();
			}

			_computedSelectAndOrderColumns = computedSelectAndOrderColumnList.ToArray();
			_selectColumns = selectColumnList.ToArray();
		}

		private static void CreateBufferedValue(ExpressionNode expression, ICollection<ComputedValueDefinition> computedColumnList, ICollection<RowBufferEntry> columnList, IEnumerable<ComputedValueDefinition> alreadyComputedBufferedValues)
		{
			if (alreadyComputedBufferedValues != null)
				expression = ReplaceAlreadyComputedSubsequences(expression, alreadyComputedBufferedValues);

			foreach (ComputedValueDefinition computedBufferedValue in computedColumnList)
			{
				if (expression.IsStructuralEqualTo(computedBufferedValue.Expression))
				{
					columnList.Add(computedBufferedValue.Target);
					return;
				}
			}

			RowBufferEntryExpression rowBufferExpression = expression as RowBufferEntryExpression;
			if (rowBufferExpression != null)
			{
				columnList.Add(rowBufferExpression.RowBufferEntry);
			}
			else
			{
				RowBufferEntry rowBufferEntry = new RowBufferEntry(expression.ExpressionType);
				columnList.Add(rowBufferEntry);

				ComputedValueDefinition computedValue = new ComputedValueDefinition();
				computedValue.Target = rowBufferEntry;
				computedValue.Expression = expression;
				computedColumnList.Add(computedValue);
			}
		}

		private static ExpressionNode ReplaceAlreadyComputedSubsequences(ExpressionNode expression, IEnumerable<ComputedValueDefinition> alreadyComputedBufferedValues)
		{
			ComputedSubsequenceReplacer replacer = new ComputedSubsequenceReplacer(alreadyComputedBufferedValues);
			return replacer.VisitExpression(expression);
		}
	}
}