using System;

namespace NQuery.Compilation
{
	internal sealed class JoinLinearizer : StandardVisitor
	{
		public override AlgebraNode VisitJoinAlgebraNode(JoinAlgebraNode node)
		{
			// A IJ (B J C) --> (A IJ B) J C

			node.Left = VisitAlgebraNode(node.Left);

			if (node.Op != JoinAlgebraNode.JoinOperator.InnerJoin)
				return node;

			JoinAlgebraNode rightSide = node.Right as JoinAlgebraNode;

			if (rightSide == null)
				return node;

			node.Right = rightSide.Left;
			rightSide.Left = node;

			return VisitAlgebraNode(rightSide);
		}
	}
}