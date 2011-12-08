using System;

namespace NQuery.Compilation
{
	internal sealed class AtLeastOneRowChecker : StandardVisitor
	{
		private bool _willProduceAtLeastOneRow;

		public AtLeastOneRowChecker()
		{
		}

		public bool WillProduceAtLeastOneRow
		{
			get { return _willProduceAtLeastOneRow; }
		}

		public override AstNode Visit(AstNode node)
		{
			switch (node.NodeType)
			{
				case AstNodeType.ConstantScanAlgebraNode:
					// For theses nodes it is clear that this subtree will produce 
					// at least one row.
					_willProduceAtLeastOneRow = true;
					return node;

				case AstNodeType.AggregateAlgebraNode:
					// For these nodes it depends on their actual config.
					return base.Visit(node);

				case AstNodeType.ResultAlgebraNode:
				case AstNodeType.TopAlgebraNode:
				case AstNodeType.AssertAlgebraNode:
				case AstNodeType.ComputeScalarAlgebraNode:                    
				case AstNodeType.SortAlgebraNode:
				case AstNodeType.ConcatAlgebraNode:
					// These nodes don't change the "at least one row" property at
					// all. So we visit them, but don't override their visitation.
					// Their "at least one row" property depends on their input.
					return base.Visit(node);
                    
				default:
					// For all other rows we assume that they can produce more
					// then one row.
					return node;
			}
		}

		public override AlgebraNode VisitAggregateAlgebraNode(AggregateAlgebraNode node)
		{
			_willProduceAtLeastOneRow = (node.Groups == null || node.Groups.Length == 0);
			return node;
		}

		public override AlgebraNode VisitTopAlgebraNode(TopAlgebraNode node)
		{
			if (node.Limit == 0)
			{
				_willProduceAtLeastOneRow = false;
				return node;
			}

			return base.VisitTopAlgebraNode(node);
		}
	}
}