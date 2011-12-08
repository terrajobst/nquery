using System;

namespace NQuery.Compilation
{
	internal sealed class OuterJoinReorderer : StandardVisitor
	{
		public OuterJoinReorderer()
		{
		}

		public override AlgebraNode VisitJoinAlgebraNode(JoinAlgebraNode node)
		{
			node.Left = VisitAlgebraNode(node.Left);
			node.Right = VisitAlgebraNode(node.Right);

			// Reorder (pull LOJ out of IJ)
			//
			//  (A LOJ B) IJ C     --->    (A IJ C) LOJ B    (IJ does not depend on B)
			//  (A ROJ B) IJ C     --->    A ROJ (B IJ C)    (IJ does not depend on A)
            
			if (node.Op == JoinAlgebraNode.JoinOperator.InnerJoin)
			{
				JoinAlgebraNode childJoin = node.Left as JoinAlgebraNode;

				if (childJoin != null && 
				    (childJoin.Op == JoinAlgebraNode.JoinOperator.LeftOuterJoin || 
				     childJoin.Op == JoinAlgebraNode.JoinOperator.RightOuterJoin))
				{
					if (childJoin.Op == JoinAlgebraNode.JoinOperator.LeftOuterJoin)
					{
						if (AstUtil.JoinDoesNotDependOn(node, childJoin.Right))
						{
							node.Left = childJoin.Left;
							childJoin.Left = node;
							return VisitAlgebraNode(childJoin);
						}
					}
					else if (childJoin.Op == JoinAlgebraNode.JoinOperator.RightOuterJoin)
					{
						if (AstUtil.JoinDoesNotDependOn(node, childJoin.Left))
						{
							node.Left = childJoin.Right;
							childJoin.Right = node;
							return VisitAlgebraNode(childJoin);
						}
					}
				}
			}

			//// Reorder (push LSJ)
			////
			////  (A J B) LSJ C      --->    (A LSJ C) J B     (LSJ does not depend on B)
			////  (A J B) LSJ C      --->     A J (B LSJ C)    (LSJ does not depend on A)

			//if (node.Op == JoinAlgebraNode.JoinOperator.LeftSemiJoin ||
			//    node.Op == JoinAlgebraNode.JoinOperator.LeftAntiSemiJoin)
			//{
			//    JoinAlgebraNode childJoin = node.Left as JoinAlgebraNode;
			//    if (childJoin != null)
			//    {
			//        if (AstUtil.JoinDoesNotDependOn(node, childJoin.Right))
			//        {
			//            node.Left = childJoin.Left;
			//            childJoin.Left = node;
			//            return VisitAlgebraNode(childJoin);
			//        }

			//        if (AstUtil.JoinDoesNotDependOn(node, childJoin.Left))
			//        {
			//            node.Left = childJoin.Right;
			//            childJoin.Right = node;
			//            return VisitAlgebraNode(childJoin);
			//        }
			//    }
			//}

			return node;
		}
	}
}