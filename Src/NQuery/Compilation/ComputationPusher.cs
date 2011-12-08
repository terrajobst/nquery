using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class ComputationPusher : StandardVisitor
	{
		#region Helpers

		private class RowBufferEntryReplacer : StandardVisitor
		{
			private ComputedValueDefinition[] _definedValues;

			public RowBufferEntryReplacer(ComputedValueDefinition[] definedValues)
			{
				_definedValues = definedValues;
			}

			public override ExpressionNode VisitRowBufferEntryExpression(RowBufferEntryExpression expression)
			{
				foreach (ComputedValueDefinition valueDefinition in _definedValues)
				{
					if (valueDefinition.Target == expression.RowBufferEntry)
						return valueDefinition.Expression;
				}

				return base.VisitRowBufferEntryExpression(expression);
			}
		}

		private AlgebraNode PushOverUnary(ComputeScalarAlgebraNode node)
		{
			UnaryAlgebraNode inputNode = (UnaryAlgebraNode) node.Input;
			node.Input = inputNode.Input;
			inputNode.Input = VisitAlgebraNode(node);
			return inputNode;
		}

		private AlgebraNode PushOverFilter(ComputeScalarAlgebraNode node)
		{
			FilterAlgebraNode filterAlgebraNode = (FilterAlgebraNode) node.Input;

			if (filterAlgebraNode.Input is TableAlgebraNode)
			{
				// We don't push a compute scalar over the input if this is already a table scan.
				return node;
			}

			return PushOverUnary(node);
		}

		private AlgebraNode MergeWithComputeScalar(ComputeScalarAlgebraNode node)
		{
			ComputeScalarAlgebraNode inputNode = (ComputeScalarAlgebraNode) node.Input;
			node.Input = inputNode.Input;
			node.DefinedValues = ArrayHelpers.JoinArrays(node.DefinedValues, inputNode.DefinedValues);

			RowBufferEntryReplacer rowBufferEntryReplacer = new RowBufferEntryReplacer(inputNode.DefinedValues);
			foreach (ComputedValueDefinition definedValue in node.DefinedValues)
				definedValue.Expression = rowBufferEntryReplacer.VisitExpression(definedValue.Expression);

			return VisitAlgebraNode(node);
		}

		private AlgebraNode MegeWithConstantScan(ComputeScalarAlgebraNode node)
		{
			ConstantScanAlgebraNode constantScanAlgebraNode = (ConstantScanAlgebraNode) node.Input;
			constantScanAlgebraNode.DefinedValues = ArrayHelpers.JoinArrays(constantScanAlgebraNode.DefinedValues, node.DefinedValues);
			return VisitAlgebraNode(constantScanAlgebraNode);
		}

		private AlgebraNode PushOverJoin(ComputeScalarAlgebraNode node)
		{
			JoinAlgebraNode joinAlgebraNode = (JoinAlgebraNode) node.Input;
			RowBufferEntry[] leftDefinedValues = AstUtil.GetDefinedValueEntries(joinAlgebraNode.Left);
			RowBufferEntry[] rightDefinedValues = AstUtil.GetDefinedValueEntries(joinAlgebraNode.Right);

			List<ComputedValueDefinition> remainingValueDefinitions = new List<ComputedValueDefinition>();
			List<ComputedValueDefinition> leftPushedValueDefinitions = new List<ComputedValueDefinition>();
			List<ComputedValueDefinition> rightPushedValueDefinitions = new List<ComputedValueDefinition>();

			bool canPushToLeftSide = joinAlgebraNode.Op != JoinAlgebraNode.JoinOperator.RightOuterJoin &&
			                         joinAlgebraNode.Op != JoinAlgebraNode.JoinOperator.FullOuterJoin;

			bool canPushToRightSide = joinAlgebraNode.Op != JoinAlgebraNode.JoinOperator.LeftOuterJoin &&
			                          joinAlgebraNode.Op != JoinAlgebraNode.JoinOperator.FullOuterJoin;

			foreach (ComputedValueDefinition valueDefinition in node.DefinedValues)
			{
				RowBufferEntry[] referencedValues = AstUtil.GetRowBufferEntryReferences(valueDefinition.Expression);
				bool referencesProbeColumn = ArrayHelpers.Contains(referencedValues, joinAlgebraNode.ProbeBufferEntry);

				if (!referencesProbeColumn && canPushToLeftSide && AstUtil.DoesNotReference(referencedValues, rightDefinedValues))
				{
					leftPushedValueDefinitions.Add(valueDefinition);
				}
				else if (!referencesProbeColumn && canPushToRightSide && AstUtil.DoesNotReference(referencedValues, leftDefinedValues))
				{
					rightPushedValueDefinitions.Add(valueDefinition);
				}
				else
				{
					remainingValueDefinitions.Add(valueDefinition);
				}
			}

			if (leftPushedValueDefinitions.Count > 0)
			{
				ComputeScalarAlgebraNode computeScalarAlgebraNode = new ComputeScalarAlgebraNode();
				computeScalarAlgebraNode.DefinedValues = leftPushedValueDefinitions.ToArray();
				computeScalarAlgebraNode.Input = joinAlgebraNode.Left;
				joinAlgebraNode.Left = VisitAlgebraNode(computeScalarAlgebraNode);
			}

			if (rightPushedValueDefinitions.Count > 0)
			{
				ComputeScalarAlgebraNode computeScalarAlgebraNode = new ComputeScalarAlgebraNode();
				computeScalarAlgebraNode.DefinedValues = rightPushedValueDefinitions.ToArray();
				computeScalarAlgebraNode.Input = joinAlgebraNode.Right;
				joinAlgebraNode.Right = VisitAlgebraNode(computeScalarAlgebraNode);
			}

			if (remainingValueDefinitions.Count == 0)
				return joinAlgebraNode;

			node.DefinedValues = remainingValueDefinitions.ToArray();
			return node;
		}

		#endregion

		public override AlgebraNode VisitComputeScalarAlgebraNode(ComputeScalarAlgebraNode node)
		{
			switch (node.Input.NodeType)
			{
				// Push over top, filter, sort, max one row
				case AstNodeType.TopAlgebraNode:
			    case AstNodeType.SortAlgebraNode:
			    case AstNodeType.AssertAlgebraNode:
			    case AstNodeType.ResultAlgebraNode:
					return PushOverUnary(node);

				case AstNodeType.FilterAlgebraNode:
					return PushOverFilter(node);

				// Merge with computation
				case AstNodeType.ComputeScalarAlgebraNode:
					return MergeWithComputeScalar(node);

				// Merge with constant scan
				case AstNodeType.ConstantScanAlgebraNode:
					return MegeWithConstantScan(node);

				// Push over join
				case AstNodeType.JoinAlgebraNode:
					return PushOverJoin(node);

				// TODO: Push over concatenation
				case AstNodeType.ConcatAlgebraNode:
					return base.VisitComputeScalarAlgebraNode(node);

				// TODO: Push over aggregation
				case AstNodeType.AggregateAlgebraNode:
					return base.VisitComputeScalarAlgebraNode(node);

				default:
					return base.VisitComputeScalarAlgebraNode(node);
			}
		}
	}
}