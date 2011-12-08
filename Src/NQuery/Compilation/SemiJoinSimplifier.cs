using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class SemiJoinSimplifier : StandardVisitor
	{
		private Stack<bool> _semiJoinContextFlagStack = new Stack<bool>();
        
		public bool IsSemiJoinContext
		{
			get
			{
				if (_semiJoinContextFlagStack.Count == 0)
					return false;

				return _semiJoinContextFlagStack.Peek();
			}
		}

		public override AlgebraNode VisitResultAlgebraNode(ResultAlgebraNode node)
		{
			if (IsSemiJoinContext)
				return VisitAlgebraNode(node.Input);

			return base.VisitResultAlgebraNode(node);
		}

		public override AlgebraNode VisitJoinAlgebraNode(JoinAlgebraNode node)
		{
			// For easier handling we replace right operations by equivalent
			// left operations.
            
			if (node.Op == JoinAlgebraNode.JoinOperator.RightOuterJoin ||
			    node.Op == JoinAlgebraNode.JoinOperator.RightSemiJoin ||
			    node.Op == JoinAlgebraNode.JoinOperator.RightAntiSemiJoin)
			{
				node.SwapSides();
				return VisitAlgebraNode(node);
			}

			// Visit children
            
			bool semiJoinContext;

			semiJoinContext = (node.Op == JoinAlgebraNode.JoinOperator.RightSemiJoin ||
			                   node.Op == JoinAlgebraNode.JoinOperator.RightAntiSemiJoin);
			_semiJoinContextFlagStack.Push(semiJoinContext);
			node.Left = VisitAlgebraNode(node.Left);
			_semiJoinContextFlagStack.Pop();

			semiJoinContext = (node.Op == JoinAlgebraNode.JoinOperator.LeftSemiJoin ||
			                   node.Op == JoinAlgebraNode.JoinOperator.LeftAntiSemiJoin);
			_semiJoinContextFlagStack.Push(semiJoinContext);
			node.Right = VisitAlgebraNode(node.Right);
			_semiJoinContextFlagStack.Pop();

			return node;
		}
	}
}