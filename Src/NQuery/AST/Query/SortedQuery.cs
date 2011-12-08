using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class SortedQuery : QueryNode
	{
		private QueryNode _input; 	
		private OrderByColumn[] _orderByColumns;

		public override SelectColumn[] GetColumns()
		{
			return _input.GetColumns();
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.SortedQuery; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			SortedQuery result = new SortedQuery();
			result.Input = (QueryNode)_input.Clone(alreadyClonedElements);
			result.OrderByColumns = ArrayHelpers.CreateDeepCopyOfAstElementArray(_orderByColumns, alreadyClonedElements);
			return result;
		}

		public QueryNode Input
		{
			get { return _input; }
			set { _input = value; }
		}

		public OrderByColumn[] OrderByColumns
		{
			get { return _orderByColumns; }
			set { _orderByColumns = value; }
		}
	}
}