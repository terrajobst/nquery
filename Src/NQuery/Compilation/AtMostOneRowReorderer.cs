using System;

namespace NQuery.Compilation
{
	internal sealed class AtMostOneRowReorderer : StandardVisitor
	{
		private AlgebraNode ReturnInputIfItProducesJustOneRow(UnaryAlgebraNode node)
		{
			node.Input = VisitAlgebraNode(node.Input);
            
			if (AstUtil.WillProduceAtMostOneRow(node.Input))
				return node.Input;
            
			return node;
		}

		public override AlgebraNode VisitTopAlgebraNode(TopAlgebraNode node)
		{
			return ReturnInputIfItProducesJustOneRow(node);
		}

		public override AlgebraNode VisitSortAlgebraNode(SortAlgebraNode node)
		{
			return ReturnInputIfItProducesJustOneRow(node);
		}

		public override AlgebraNode VisitJoinAlgebraNode(JoinAlgebraNode node)
		{
			// Reorder
			//
			// A LSJ (B LOJ C)      --->    (A LOJ C) LSJ B     (LOJ has no join condition and C produces at most one row)

			if (node.Op == JoinAlgebraNode.JoinOperator.LeftSemiJoin ||
			    node.Op == JoinAlgebraNode.JoinOperator.LeftAntiSemiJoin)
			{
				JoinAlgebraNode rightChildJoin = node.Right as JoinAlgebraNode;
                
				if (rightChildJoin != null && 
				    rightChildJoin.Op == JoinAlgebraNode.JoinOperator.LeftOuterJoin &&
				    rightChildJoin.Predicate == null &&
				    AstUtil.WillProduceAtMostOneRow(rightChildJoin.Right))
				{
					node.Right = rightChildJoin.Left;
					rightChildJoin.Left = node.Left;
					node.Left = rightChildJoin;
					return VisitAlgebraNode(node);
				}
			}

			node.Left = VisitAlgebraNode(node.Left);
			node.Right = VisitAlgebraNode(node.Right);
			return node;
		}
	}
}