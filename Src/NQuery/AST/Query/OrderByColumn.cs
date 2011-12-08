using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class OrderByColumn : AstElement
	{
		private ExpressionNode _expression;
		private SortOrder _sortOrder;
		private int _columnIndex;

		public OrderByColumn()
		{
		}

		public OrderByColumn(ExpressionNode expression, SortOrder sortOrder)
			: this()
		{
			_expression = expression;
			_sortOrder = sortOrder;
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			OrderByColumn result = new OrderByColumn();
			result.Expression = (ExpressionNode)_expression.Clone(alreadyClonedElements);
			result.SortOrder = _sortOrder;
			return result;
		}

		public ExpressionNode Expression
		{
			get { return _expression; }
			set { _expression = value; }
		}

		public SortOrder SortOrder
		{
			get { return _sortOrder; }
			set { _sortOrder = value; }
		}

		public int ColumnIndex
		{
			get { return _columnIndex; }
			set { _columnIndex = value; }
		}
	}
}