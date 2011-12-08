using System;
using System.Collections.Generic;

namespace NQuery.Compilation 
{
	internal sealed class BinaryQuery : QueryNode
	{
		private QueryNode _left;
		private QueryNode _right;
		private BinaryQueryOperator _op;
		
		public override AstNodeType NodeType
		{
			get { return  AstNodeType.BinaryQuery; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			BinaryQuery result = new BinaryQuery();
			result.Left = (QueryNode)_left.Clone(alreadyClonedElements);
			result.Right = (QueryNode)_right.Clone(alreadyClonedElements);
			result.Op = _op;
			return result;
		}

		public override SelectColumn[] GetColumns()
		{
			return _left.GetColumns();
		}
		
		public QueryNode Left
		{
			get { return _left; }
			set { _left = value; }
		}

		public QueryNode Right
		{
			get { return _right; }
			set { _right = value; }
		}

		public BinaryQueryOperator Op
		{
			get { return _op; }
			set { _op = value; }
		}
	}
}