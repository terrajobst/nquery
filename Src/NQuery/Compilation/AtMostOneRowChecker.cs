using System;

namespace NQuery.Compilation
{
	internal sealed class AtMostOneRowChecker : StandardVisitor
	{
		private bool _willProduceAtMostOneRow;

		public AtMostOneRowChecker()
		{
		}

		public bool WillProduceAtMostOneRow
		{
			get { return _willProduceAtMostOneRow; }
		}

		public override AstNode Visit(AstNode node)
		{
			switch (node.NodeType)
			{
				case AstNodeType.ConstantScanAlgebraNode:
				case AstNodeType.NullScanAlgebraNode:
					// For theses nodes it is clear that this subtree will produce 
					// at most one row.
					_willProduceAtMostOneRow = true;
					return node;

				case AstNodeType.AggregateAlgebraNode:
				case AstNodeType.TopAlgebraNode:
				case AstNodeType.AssertAlgebraNode:
					// For these nodes it depends on their actual config.
					return base.Visit(node);

				case AstNodeType.ResultAlgebraNode:
				case AstNodeType.ComputeScalarAlgebraNode:                    
				case AstNodeType.SortAlgebraNode:
				case AstNodeType.FilterAlgebraNode:
					// These nodes don't change the at "most one row" property at
					// all. So we visit them, but don't override their visitation.
					// Their "most one row" property depends on their input.
					return base.Visit(node);
                    
				default:
					// For all other rows we assume that they can produce more
					// then one row.
					return node;
			}
		}

		public override AlgebraNode VisitAggregateAlgebraNode(AggregateAlgebraNode node)
		{
			_willProduceAtMostOneRow = (node.Groups == null || node.Groups.Length == 0);
			return node;
		}

		public override AlgebraNode VisitTopAlgebraNode(TopAlgebraNode node)
		{
			_willProduceAtMostOneRow = node.Limit <= 1;
			return node;
		}

		public override AlgebraNode VisitAssertAlgebraNode(AssertAlgebraNode node)
		{
			_willProduceAtMostOneRow = node.AssertionType == AssertionType.MaxOneRow;
			return node;
		}
	}
}