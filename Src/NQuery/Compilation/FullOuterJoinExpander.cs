using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class FullOuterJoinExpander : StandardVisitor
	{
		public override AlgebraNode VisitJoinAlgebraNode(JoinAlgebraNode node)
		{
			base.VisitJoinAlgebraNode(node);

			if (node.Op == JoinAlgebraNode.JoinOperator.FullOuterJoin)
			{
				// TODO: Check if we could represent this join condition by an hash match operator

				JoinAlgebraNode leftOuterJoinNode = new JoinAlgebraNode();
				leftOuterJoinNode.Op = JoinAlgebraNode.JoinOperator.LeftOuterJoin;
				leftOuterJoinNode.Predicate = (ExpressionNode)node.Predicate.Clone();
				leftOuterJoinNode.Left = (AlgebraNode) node.Left.Clone();
				leftOuterJoinNode.Right = (AlgebraNode) node.Right.Clone();
				leftOuterJoinNode.OutputList = ArrayHelpers.Clone(node.OutputList);

				List<RowBufferEntry> swappedOutputList = new List<RowBufferEntry>();
				swappedOutputList.AddRange(node.Right.OutputList);
				swappedOutputList.AddRange(node.Left.OutputList);

				JoinAlgebraNode leftAntiSemiJoinNode = new JoinAlgebraNode();
				leftAntiSemiJoinNode.Op = JoinAlgebraNode.JoinOperator.LeftAntiSemiJoin;
				leftAntiSemiJoinNode.Predicate = (ExpressionNode) node.Predicate.Clone();
				leftAntiSemiJoinNode.Left = (AlgebraNode) node.Right.Clone();
				leftAntiSemiJoinNode.Right = (AlgebraNode) node.Left.Clone();
				leftAntiSemiJoinNode.OutputList = swappedOutputList.ToArray();

				List<ComputedValueDefinition> computeScalareDefinedValues = new List<ComputedValueDefinition>();
				foreach (RowBufferEntry rowBufferEntry in node.Left.OutputList)
				{
					ComputedValueDefinition definedValue = new ComputedValueDefinition();
					definedValue.Target = rowBufferEntry;
					definedValue.Expression = LiteralExpression.FromTypedNull(rowBufferEntry.DataType);
					computeScalareDefinedValues.Add(definedValue);
				}

				ComputeScalarAlgebraNode computeScalarNode = new ComputeScalarAlgebraNode();
				computeScalarNode.Input = leftAntiSemiJoinNode;
				computeScalarNode.DefinedValues = computeScalareDefinedValues.ToArray();
				computeScalarNode.OutputList = swappedOutputList.ToArray();

				List<UnitedValueDefinition> concatDefinedValues = new List<UnitedValueDefinition>();
				for (int i = 0; i < node.OutputList.Length; i++)
				{
					RowBufferEntry rowBufferEntry = node.OutputList[i];
					UnitedValueDefinition concatDefinedValue = new UnitedValueDefinition();
					concatDefinedValue.Target = rowBufferEntry;
					concatDefinedValue.DependendEntries = new RowBufferEntry[] { node.OutputList[i], node.OutputList[i] };
					concatDefinedValues.Add(concatDefinedValue);
				}

				ConcatAlgebraNode concatenationNode = new ConcatAlgebraNode();
				concatenationNode.Inputs = new AlgebraNode[] {leftOuterJoinNode, computeScalarNode};
				concatenationNode.DefinedValues = concatDefinedValues.ToArray();
				concatenationNode.OutputList = swappedOutputList.ToArray();

				return concatenationNode;
			}

			return node;
		}
	}
}