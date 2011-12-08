using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class NullScanOptimizer : StandardVisitor
	{
		#region Helpers

		private static AlgebraNode NullScanIfInputIsNullScan(UnaryAlgebraNode algebraNode)
		{
			if (algebraNode.Input is NullScanAlgebraNode)
				return CreateNullScan(algebraNode.OutputList);

			return algebraNode;
		}

		private static NullScanAlgebraNode CreateNullScan(RowBufferEntry[] outputList)
		{
			NullScanAlgebraNode result = new NullScanAlgebraNode();
			result.OutputList = outputList;
			return result;
		}

		private enum RowBufferCreationMode
		{
			KeepExisting,
			CreateNew
		}

		private static AlgebraNode PadRightWithNullsOnLeftSide(JoinAlgebraNode node, RowBufferCreationMode rowBufferCreationMode)
		{
			ComputeScalarAlgebraNode computeScalarAlgebraNode = new ComputeScalarAlgebraNode();
			computeScalarAlgebraNode.Input = node.Right;

			List<RowBufferEntry> outputList = new List<RowBufferEntry>();
			outputList.AddRange(node.Right.OutputList);

			List<ComputedValueDefinition> definedValues = new List<ComputedValueDefinition>();
			foreach (RowBufferEntry rowBufferEntry in node.Left.OutputList)
			{
				ComputedValueDefinition definedValue = new ComputedValueDefinition();

				if (rowBufferCreationMode == RowBufferCreationMode.KeepExisting)
				{
					definedValue.Target = rowBufferEntry;
				}
				else
				{
					definedValue.Target = new RowBufferEntry(rowBufferEntry.DataType);
				}

				definedValue.Expression = LiteralExpression.FromTypedNull(rowBufferEntry.DataType);
				definedValues.Add(definedValue);
				outputList.Add(definedValue.Target);
			}

			computeScalarAlgebraNode.DefinedValues = definedValues.ToArray();
			computeScalarAlgebraNode.OutputList = outputList.ToArray();

			return computeScalarAlgebraNode;
		}

		private static AlgebraNode PadLeftWithNullsOnRightSide(JoinAlgebraNode node, RowBufferCreationMode rowBufferCreationMode)
		{
			ComputeScalarAlgebraNode computeScalarAlgebraNode = new ComputeScalarAlgebraNode();
			computeScalarAlgebraNode.Input = node.Left;

			List<RowBufferEntry> outputList = new List<RowBufferEntry>();
			outputList.AddRange(node.Left.OutputList);

			List<ComputedValueDefinition> definedValues = new List<ComputedValueDefinition>();
			foreach (RowBufferEntry rowBufferEntry in node.Right.OutputList)
			{
				ComputedValueDefinition definedValue = new ComputedValueDefinition();

				if (rowBufferCreationMode == RowBufferCreationMode.KeepExisting)
				{
					definedValue.Target = rowBufferEntry;
				}
				else
				{
					definedValue.Target = new RowBufferEntry(rowBufferEntry.DataType);
				}

				definedValue.Expression = LiteralExpression.FromTypedNull(rowBufferEntry.DataType);
				definedValues.Add(definedValue);
				outputList.Add(definedValue.Target);
			}

			computeScalarAlgebraNode.DefinedValues = definedValues.ToArray();
			computeScalarAlgebraNode.OutputList = outputList.ToArray();

			return computeScalarAlgebraNode;
		}

		#endregion

		public override AlgebraNode VisitSortAlgebraNode(SortAlgebraNode node)
		{
			node.Input = VisitAlgebraNode(node.Input);
			return NullScanIfInputIsNullScan(node);
		}

		public override AlgebraNode VisitTopAlgebraNode(TopAlgebraNode node)
		{
			if (node.Limit == 0)
				return CreateNullScan(node.OutputList);
            
			node.Input = VisitAlgebraNode(node.Input);
			return NullScanIfInputIsNullScan(node);
		}

		public override AlgebraNode VisitAssertAlgebraNode(AssertAlgebraNode node)
		{
			node.Input = VisitAlgebraNode(node.Input);
			return NullScanIfInputIsNullScan(node);
		}

		public override AlgebraNode VisitComputeScalarAlgebraNode(ComputeScalarAlgebraNode node)
		{
			node.Input = VisitAlgebraNode(node.Input);
			return NullScanIfInputIsNullScan(node);
		}

		public override AlgebraNode VisitFilterAlgebraNode(FilterAlgebraNode node)
		{
			node.Input = VisitAlgebraNode(node.Input);

			ConstantExpression predicateAsConstant = node.Predicate as ConstantExpression;
			if (predicateAsConstant != null)
			{
				if (predicateAsConstant.AsBoolean)
					return node.Input;
				else
					return CreateNullScan(node.OutputList);
			}

			return NullScanIfInputIsNullScan(node);
		}

		public override AlgebraNode VisitConcatAlgebraNode(ConcatAlgebraNode node)
		{
			base.VisitConcatAlgebraNode(node);
            
			List<AlgebraNode> nonEmptyInputs = new List<AlgebraNode>();

			foreach (AlgebraNode input in node.Inputs)
			{
				if (input is NullScanAlgebraNode)
				{
					// removed by not adding to list.
				}
				else
				{
					nonEmptyInputs.Add(input);
				}
			}

			if (nonEmptyInputs.Count == 0)
				return CreateNullScan(node.OutputList);

			if (nonEmptyInputs.Count == 1)
			{
				int inputIndex = Array.IndexOf(node.Inputs, nonEmptyInputs[0]);

				List<ComputedValueDefinition> definedValues = new List<ComputedValueDefinition>();
				ComputeScalarAlgebraNode computeScalarAlgebraNode = new ComputeScalarAlgebraNode();
				computeScalarAlgebraNode.Input = nonEmptyInputs[0];
				foreach (UnitedValueDefinition unitedValueDefinition in node.DefinedValues)
				{
					ComputedValueDefinition computedValueDefinition = new ComputedValueDefinition();
					computedValueDefinition.Target = unitedValueDefinition.Target;
					computedValueDefinition.Expression = new RowBufferEntryExpression(unitedValueDefinition.DependendEntries[inputIndex]);
					definedValues.Add(computedValueDefinition);
				}
				computeScalarAlgebraNode.DefinedValues = definedValues.ToArray();
				computeScalarAlgebraNode.OutputList = node.OutputList;
				return computeScalarAlgebraNode;
			}

			node.Inputs = nonEmptyInputs.ToArray();

			// Update dependend entries

			for (int i = 0; i < node.DefinedValues.Length; i++)
			{
				UnitedValueDefinition definition = node.DefinedValues[i];
				List<RowBufferEntry> dependendEntries = new List<RowBufferEntry>();

				foreach (RowBufferEntry dependendEntry in definition.DependendEntries)
				{
					bool entryInAnyInput = false;

					foreach (AlgebraNode input in node.Inputs)
					{
						if (ArrayHelpers.Contains(input.OutputList, dependendEntry))
						{
							entryInAnyInput = true;
							break;
						}
					}

					if (entryInAnyInput)
						dependendEntries.Add(dependendEntry);
				}

				definition.DependendEntries = dependendEntries.ToArray();
			}
			return node;
		}

		public override AlgebraNode VisitAggregateAlgebraNode(AggregateAlgebraNode node)
		{
			node.Input = VisitAlgebraNode(node.Input);

			if (node.Input is NullScanAlgebraNode)
			{
				if (node.Groups != null && node.Groups.Length > 0)
				{
					// Grouped queries return nothing on empty input.
					return CreateNullScan(node.OutputList);
				}
				else
				{
					// Non-grouped aggregation. We return the result value of the aggregates
					// executed against an empty input as a single row.

					List<RowBufferEntry> outputList = new List<RowBufferEntry>();
					List<ComputedValueDefinition> emptyValueList = new List<ComputedValueDefinition>();
					foreach (AggregatedValueDefinition definedValue in node.DefinedValues)
					{
						IAggregator aggregator = definedValue.Aggregator;
						aggregator.Init();
						object emptyValue = aggregator.Terminate();

						ComputedValueDefinition computedBufferedValue = new ComputedValueDefinition();
						computedBufferedValue.Target = definedValue.Target;
						computedBufferedValue.Expression = LiteralExpression.FromTypedValue(emptyValue, aggregator.ReturnType);
						emptyValueList.Add(computedBufferedValue);
						outputList.Add(computedBufferedValue.Target);
					}

					ConstantScanAlgebraNode constantScanAlgebraNode = new ConstantScanAlgebraNode();
					constantScanAlgebraNode.DefinedValues = emptyValueList.ToArray();
					constantScanAlgebraNode.OutputList = outputList.ToArray();
					return constantScanAlgebraNode;
				}
			}
            
			return node;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public override AlgebraNode VisitJoinAlgebraNode(JoinAlgebraNode node)
		{
			node.Left = VisitAlgebraNode(node.Left);
			node.Right = VisitAlgebraNode(node.Right);

			bool leftIsNull = node.Left is NullScanAlgebraNode;
			bool rightIsNull = node.Right is NullScanAlgebraNode;
			bool joinConditionAlwaysFalse = false;

			if (node.Predicate != null)
			{
				ExpressionNode predicate = node.Predicate;
				ConstantExpression predicateAsConstant = predicate as ConstantExpression;
				if (predicateAsConstant != null)
				{
					if (predicateAsConstant.AsBoolean)
						node.Predicate = null;
					else
						joinConditionAlwaysFalse = true;
				}
			}

			if (node.Op == JoinAlgebraNode.JoinOperator.RightOuterJoin || 
			    node.Op == JoinAlgebraNode.JoinOperator.RightSemiJoin || 
			    node.Op == JoinAlgebraNode.JoinOperator.RightAntiSemiJoin)
			{
				node.SwapSides();
			}

			switch (node.Op)
			{
				case JoinAlgebraNode.JoinOperator.InnerJoin:
					if (joinConditionAlwaysFalse || leftIsNull || rightIsNull)
						return CreateNullScan(node.OutputList);
					break;
				case JoinAlgebraNode.JoinOperator.FullOuterJoin:
					if (leftIsNull && rightIsNull)
						return CreateNullScan(node.OutputList);
					if (leftIsNull)
						return PadRightWithNullsOnLeftSide(node, RowBufferCreationMode.KeepExisting);
					if (rightIsNull)
						return PadLeftWithNullsOnRightSide(node, RowBufferCreationMode.KeepExisting);
					if (joinConditionAlwaysFalse)
					{
						AlgebraNode left = PadLeftWithNullsOnRightSide(node, RowBufferCreationMode.CreateNew);
						AlgebraNode right = PadRightWithNullsOnLeftSide(node, RowBufferCreationMode.CreateNew);

						ConcatAlgebraNode concatAlgebraNode = new ConcatAlgebraNode();
						concatAlgebraNode.Inputs = new AlgebraNode[] { left, right };

						List<RowBufferEntry> outputList = new List<RowBufferEntry>();
						List<UnitedValueDefinition> definedValues = new List<UnitedValueDefinition>();
						for (int i = 0; i < node.Left.OutputList.Length; i++)
						{
							int leftIndex = i;
							int rightIndex = node.Right.OutputList.Length + i;

							UnitedValueDefinition definedValue = new UnitedValueDefinition();
							definedValue.Target = node.Left.OutputList[leftIndex];
							definedValue.DependendEntries = new RowBufferEntry[] { left.OutputList[leftIndex], right.OutputList[rightIndex] };
							definedValues.Add(definedValue);
							outputList.Add(definedValue.Target);
						}
						for (int i = 0; i < node.Right.OutputList.Length; i++)
						{
							int leftIndex = node.Left.OutputList.Length + i;
							int rightIndex = i;

							UnitedValueDefinition definedValue = new UnitedValueDefinition();
							definedValue.Target = node.Right.OutputList[rightIndex];
							definedValue.DependendEntries = new RowBufferEntry[] { left.OutputList[leftIndex], right.OutputList[rightIndex] };
							definedValues.Add(definedValue);
							outputList.Add(definedValue.Target);
						}

						concatAlgebraNode.DefinedValues = definedValues.ToArray();
						concatAlgebraNode.OutputList = outputList.ToArray();
						return concatAlgebraNode;
					}
					break;
				case JoinAlgebraNode.JoinOperator.LeftOuterJoin:
					if (leftIsNull)
						return CreateNullScan(node.OutputList);
					if (rightIsNull || joinConditionAlwaysFalse)
						return PadLeftWithNullsOnRightSide(node, RowBufferCreationMode.KeepExisting);
					break;
				case JoinAlgebraNode.JoinOperator.LeftSemiJoin:
					if (leftIsNull || rightIsNull || joinConditionAlwaysFalse)
						return CreateNullScan(node.OutputList);
					if (node.Predicate == null && AstUtil.WillProduceAtLeastOneRow(node.Right))
						return node.Left;
					break;
				case JoinAlgebraNode.JoinOperator.LeftAntiSemiJoin:
					if (leftIsNull)
						return CreateNullScan(node.OutputList);
					if (rightIsNull || joinConditionAlwaysFalse)
						return node.Left;
					if (node.Predicate == null && AstUtil.WillProduceAtLeastOneRow(node.Right))
						return CreateNullScan(node.OutputList);
					break;
			}
            
			return node;
		}
	}
}