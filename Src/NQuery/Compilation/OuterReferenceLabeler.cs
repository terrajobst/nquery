using System;

namespace NQuery.Compilation
{
	internal sealed class OuterReferenceLabeler : StandardVisitor
	{
		public override AlgebraNode VisitJoinAlgebraNode(JoinAlgebraNode node)
		{
			Visit(node.Left);
			Visit(node.Right);

			node.OuterReferences = AstUtil.GetOuterReferences(node);
            
			return node;
		}
	}
}