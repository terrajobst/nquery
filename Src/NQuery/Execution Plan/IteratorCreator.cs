#define COMPILE_IL

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using NQuery.Compilation;

using Comparer = System.Collections.Comparer;

namespace NQuery.Runtime.ExecutionPlan
{
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	internal sealed class IteratorCreator : StandardVisitor
	{
		private MetadataContext _metadataContext;
		private bool _includeStatistics;
		private Iterator _lastIterator;
		private Stack<BoundRowBufferEntrySet> _outerReferenceStack = new Stack<BoundRowBufferEntrySet>();

		private IteratorCreator(MetadataContext metadataContext, bool includeStatistics)
		{
			_metadataContext = metadataContext;
			_includeStatistics = includeStatistics;
		}

		public static ResultIterator Convert(MetadataContext metadataContext, bool includeStatistics, ResultAlgebraNode resultAlgebraNode)
		{
			IteratorCreator iteratorCreator = new IteratorCreator(metadataContext, includeStatistics);
			Iterator iterator = iteratorCreator.ConvertAlgebraNode(resultAlgebraNode);

			ILEmitContext.CompleteILCompilation();

			if (includeStatistics)
				return (ResultIterator) ((StatisticsIterator) iterator).Input;

			return (ResultIterator) iterator;
		}

		#region Helpers

		private sealed class BoundRowBufferEntrySet
		{
			public BoundRowBufferEntrySet(object[] rowBuffer, RowBufferEntry[] entries)
			{
				RowBuffer = rowBuffer;
				Entries = entries;
			}

			public object[] RowBuffer;
			public RowBufferEntry[] Entries;
		}

		private sealed class RowBufferEntryExpressionUpdater : StandardVisitor
		{
			private BoundRowBufferEntrySet[] _bufferEntrySets;

			public RowBufferEntryExpressionUpdater(BoundRowBufferEntrySet[] bufferEntrySets)
			{
				_bufferEntrySets = bufferEntrySets;
			}

			public override ExpressionNode VisitRowBufferEntryExpression(RowBufferEntryExpression expression)
			{
				foreach (BoundRowBufferEntrySet boundRowBufferEntrySet in _bufferEntrySets)
				{
					int rowBufferIndex = Array.IndexOf(boundRowBufferEntrySet.Entries, expression.RowBufferEntry);
					if (rowBufferIndex >= 0)
					{
						expression.RowBuffer = boundRowBufferEntrySet.RowBuffer;
						expression.RowBufferIndex = rowBufferIndex;
						break;
					}
				}

				return expression;
			}
		}

		private void SetLastIterator(AlgebraNode owner, Iterator iterator)
		{
			if (_includeStatistics && owner.StatisticsIterator == null)
			{
				owner.StatisticsIterator = new StatisticsIterator();
				owner.StatisticsIterator.RowBuffer = iterator.RowBuffer;
				owner.StatisticsIterator.Input = iterator;
				iterator = owner.StatisticsIterator;
			}

			_lastIterator = iterator;
		}

		private Iterator GetLastIterator()
		{
			Iterator result = _lastIterator;
			_lastIterator = null;
			return result;
		}

		private Iterator ConvertAlgebraNode(AlgebraNode algebraNode)
		{
			Visit(algebraNode);
			Iterator iterator = GetLastIterator();
			return iterator;
		}

		private static IteratorInput[] GetIteratorInput(RowBufferEntry[] allEntries, RowBufferEntry[] neededEntries)
		{
			IteratorInput[] result = new IteratorInput[neededEntries.Length];

			for (int i = 0; i < neededEntries.Length; i++)
			{
				result[i] = new IteratorInput();
				result[i].SourceIndex = Array.IndexOf(allEntries, neededEntries[i]);
			}

			return result;
		}

		private static IteratorOutput[] GetIteratorOutput(int offset, RowBufferEntry[] allEntries, RowBufferEntry[] neededEntries)
		{
			List<IteratorOutput> result = new List<IteratorOutput>(neededEntries.Length);

			for (int i = 0; i < allEntries.Length; i++)
			{
				if (ArrayHelpers.Contains(neededEntries, allEntries[i]))
				{
					IteratorOutput iteratorOutput = new IteratorOutput();
					iteratorOutput.SourceIndex = i;
					iteratorOutput.TargetIndex = offset + result.Count;
					result.Add(iteratorOutput);
				}
			}

			return result.ToArray();
		}

		private RuntimeComputedValueOutput[] GetDefinedValues(RowBufferEntry[] outputList, ComputedValueDefinition[] definedValues, params BoundRowBufferEntrySet[] boundRowBufferEntrySets)
		{
			RuntimeComputedValueOutput[] result = new RuntimeComputedValueOutput[definedValues.Length];

			for (int i = 0; i < definedValues.Length; i++)
			{
				RuntimeComputedValueOutput definedValue = new RuntimeComputedValueOutput();
				definedValue.Expression = CreateRuntimeExpression(definedValues[i].Expression, boundRowBufferEntrySets);
				definedValue.TargetIndex = Array.IndexOf(outputList, definedValues[i].Target);
				result[i] = definedValue;
			}

			return result;
		}

		private static RuntimeValueOutput GetDefinedValue(RowBufferEntry[] outputList, RowBufferEntry rowBufferEntry)
		{
			RuntimeValueOutput result = new RuntimeValueOutput();
			result.TargetIndex = Array.IndexOf(outputList, rowBufferEntry);
			return result;
		}

		private RuntimeAggregateValueOutput[] GetDefinedValues(RowBufferEntry[] outputList, AggregatedValueDefinition[] definedValues, params BoundRowBufferEntrySet[] boundRowBufferEntrySets)
		{
			RuntimeAggregateValueOutput[] result = new RuntimeAggregateValueOutput[definedValues.Length];

			for (int i = 0; i < definedValues.Length; i++)
			{
				RuntimeAggregateValueOutput definedValue = new RuntimeAggregateValueOutput();
				definedValue.Aggregator = definedValues[i].Aggregator;
				definedValue.Argument = CreateRuntimeExpression(definedValues[i].Argument, boundRowBufferEntrySets);
				definedValue.TargetIndex = Array.IndexOf(outputList, definedValues[i].Target);
				result[i] = definedValue;
			}

			return result;
		}

		private RuntimeExpression CreateRuntimeExpression(ExpressionNode expression, params BoundRowBufferEntrySet[] boundRowBufferEntrySets)
		{
			// Update row buffer references

			List<BoundRowBufferEntrySet> boundRowBufferEntrySetList = new List<BoundRowBufferEntrySet>();
			boundRowBufferEntrySetList.AddRange(boundRowBufferEntrySets);
			foreach (BoundRowBufferEntrySet outerreferenceBoundRowBufferEntrySet in _outerReferenceStack)
				boundRowBufferEntrySetList.Add(outerreferenceBoundRowBufferEntrySet);
			boundRowBufferEntrySets = boundRowBufferEntrySetList.ToArray();

			RowBufferEntryExpressionUpdater rowBufferEntryExpressionUpdater = new RowBufferEntryExpressionUpdater(boundRowBufferEntrySets);
			expression = rowBufferEntryExpressionUpdater.VisitExpression(expression);

#if COMPILE_IL
			return ExpressionCompiler.CreateCompiled(expression);
#else
			return ExpressionCompiler.CreateInterpreded(expression);
#endif
		}

		private IComparer[] GetComparersFromExpressionTypes(RowBufferEntry[] rowBufferEntries)
		{
			IComparer[] result = new IComparer[rowBufferEntries.Length];

			for (int i = 0; i < result.Length; i++)
			{
				IComparer customComparer = _metadataContext.Comparers[rowBufferEntries[i].DataType];

				if (customComparer != null)
					result[i] = customComparer;
				else
					result[i] = Comparer.Default;
			}

			return result;
		}

		private void PushOuterReferences(object[] rowBuffer, JoinAlgebraNode node)
		{
			if (node.OuterReferences != null && node.OuterReferences.Length > 0)
			{
				// Important: We cannot use node.OuterReferences as argument for BoundRowBufferEntrySet().
				// The replacment strategy below will replace occurences to the entries by their index
				// within the array. Therefore we need an array with the same layout as the row buffer.

				RowBufferEntry[] outerReferences = new RowBufferEntry[node.Left.OutputList.Length];
				for (int i = 0; i < outerReferences.Length; i++)
				{
					if (ArrayHelpers.Contains(node.OuterReferences, node.Left.OutputList[i]))
						outerReferences[i] = node.Left.OutputList[i];
				}

				BoundRowBufferEntrySet bufferEntrySet = new BoundRowBufferEntrySet(rowBuffer, outerReferences);
				_outerReferenceStack.Push(bufferEntrySet);
			}
		}

		private void UpdatePredicateRowBufferReferences(NestedLoopsIterator nestedLoopsIterator, JoinAlgebraNode node)
		{
			BoundRowBufferEntrySet leftBoundRowBufferEntrySet = new BoundRowBufferEntrySet(nestedLoopsIterator.Left.RowBuffer, node.Left.OutputList);
			BoundRowBufferEntrySet rightBoundRowBufferEntrySet = new BoundRowBufferEntrySet(nestedLoopsIterator.Right.RowBuffer, node.Right.OutputList);

			if (node.Predicate != null)
				nestedLoopsIterator.Predicate = CreateRuntimeExpression(node.Predicate, leftBoundRowBufferEntrySet, rightBoundRowBufferEntrySet);

			if (node.PassthruPredicate != null)
				nestedLoopsIterator.PassthruPredicate = CreateRuntimeExpression(node.PassthruPredicate, leftBoundRowBufferEntrySet, rightBoundRowBufferEntrySet);
		}

		private void UpdatePredicateRowBufferReferences(HashMatchIterator hashMatchIterator, HashMatchAlgebraNode node)
		{
			if (node.ProbeResidual != null)
			{
				BoundRowBufferEntrySet leftBoundRowBufferEntrySet = new BoundRowBufferEntrySet(hashMatchIterator.Left.RowBuffer, node.Left.OutputList);
				BoundRowBufferEntrySet rightBoundRowBufferEntrySet = new BoundRowBufferEntrySet(hashMatchIterator.Right.RowBuffer, node.Right.OutputList);
				hashMatchIterator.ProbeResidual = CreateRuntimeExpression(node.ProbeResidual, leftBoundRowBufferEntrySet, rightBoundRowBufferEntrySet);
			}
		}

		#endregion

		public override AlgebraNode VisitTableAlgebraNode(TableAlgebraNode node)
		{
			List<RuntimeColumnValueOutput> definedValues = new List<RuntimeColumnValueOutput>();
			foreach (ColumnValueDefinition columnValueDefinition in node.DefinedValues)
			{
				RuntimeColumnValueOutput definedValue = new RuntimeColumnValueOutput();
				definedValue.TargetIndex = definedValues.Count;
				definedValue.ColumnBinding = columnValueDefinition.ColumnRefBinding.ColumnBinding;
				definedValues.Add(definedValue);
			}

			TableIterator tableIterator = new TableIterator();
			tableIterator.RowBuffer = new object[node.OutputList.Length];
			tableIterator.DefinedValues = definedValues.ToArray();
			tableIterator.Table = node.TableRefBinding.TableBinding;
			SetLastIterator(node, tableIterator);

			return node;
		}

		public override AlgebraNode VisitJoinAlgebraNode(JoinAlgebraNode node)
		{
			if (node.Op == JoinAlgebraNode.JoinOperator.RightOuterJoin ||
				node.Op == JoinAlgebraNode.JoinOperator.RightAntiSemiJoin ||
				node.Op == JoinAlgebraNode.JoinOperator.RightSemiJoin)
			{
				if (node.OuterReferences != null && node.OuterReferences.Length > 0)
					throw ExceptionBuilder.InternalError("Outer references should not be possible for {0} and are not supported.", node.Op);

				node.SwapSides();
			}

			NestedLoopsIterator nestedLoopsIterator;

			switch (node.Op)
			{
				case JoinAlgebraNode.JoinOperator.InnerJoin:
					nestedLoopsIterator = new InnerNestedLoopsIterator();
					break;
				case JoinAlgebraNode.JoinOperator.LeftOuterJoin:
					nestedLoopsIterator = new LeftOuterNestedLoopsIterator();
					break;
				case JoinAlgebraNode.JoinOperator.LeftSemiJoin:
					if (node.ProbeBufferEntry == null)
						nestedLoopsIterator = new LeftSemiNestedLoopsIterator();
					else
					{
						ProbedSemiJoinNestedLoopsIterator probedSemiJoinNestedLoopsIterator = new ProbedSemiJoinNestedLoopsIterator();
						probedSemiJoinNestedLoopsIterator.ProbeOutput = GetDefinedValue(node.OutputList, node.ProbeBufferEntry);
						nestedLoopsIterator = probedSemiJoinNestedLoopsIterator;
					}
					break;
				case JoinAlgebraNode.JoinOperator.LeftAntiSemiJoin:
					if (node.ProbeBufferEntry == null)
						nestedLoopsIterator = new LeftAntiSemiNestedLoopsIterator();
					else
					{
						ProbedAntiSemiJoinNestedLoopsIterator probedSemiJoinNestedLoopsIterator = new ProbedAntiSemiJoinNestedLoopsIterator();
						probedSemiJoinNestedLoopsIterator.ProbeOutput = GetDefinedValue(node.OutputList, node.ProbeBufferEntry);
						nestedLoopsIterator = probedSemiJoinNestedLoopsIterator;
					}
					break;
				default:
					throw ExceptionBuilder.UnhandledCaseLabel(node.Op);
			}

			nestedLoopsIterator.RowBuffer = new object[node.OutputList.Length];
			nestedLoopsIterator.Left = ConvertAlgebraNode(node.Left);
			nestedLoopsIterator.LeftOutput = GetIteratorOutput(0, node.Left.OutputList, node.OutputList);

			PushOuterReferences(nestedLoopsIterator.Left.RowBuffer, node);

			nestedLoopsIterator.Right = ConvertAlgebraNode(node.Right);
			nestedLoopsIterator.RightOutput = GetIteratorOutput(nestedLoopsIterator.LeftOutput.Length, node.Right.OutputList, node.OutputList);

			UpdatePredicateRowBufferReferences(nestedLoopsIterator, node);

			if (node.OuterReferences != null && node.OuterReferences.Length > 0)
				_outerReferenceStack.Pop();

			SetLastIterator(node, nestedLoopsIterator);

			return node;
		}

		public override AlgebraNode VisitConstantScanAlgebraNode(ConstantScanAlgebraNode node)
		{
			ConstantIterator constantIterator = new ConstantIterator();
			constantIterator.RowBuffer = new object[node.OutputList.Length];
			constantIterator.DefinedValues = GetDefinedValues(node.OutputList, node.DefinedValues);
			SetLastIterator(node, constantIterator);

			return node;
		}

		public override AlgebraNode VisitNullScanAlgebraNode(NullScanAlgebraNode node)
		{
			NullIterator nullIterator = new NullIterator();
			nullIterator.RowBuffer = new object[node.OutputList.Length];
			SetLastIterator(node, nullIterator);

			return node;
		}

		public override AlgebraNode VisitConcatAlgebraNode(ConcatAlgebraNode node)
		{
			ConcatenationIterator concatenationIterator = new ConcatenationIterator();
			concatenationIterator.RowBuffer = new object[node.OutputList.Length];
			concatenationIterator.Inputs = new Iterator[node.Inputs.Length];
			concatenationIterator.InputOutput = new IteratorOutput[node.Inputs.Length][];
			for (int i = 0; i < concatenationIterator.Inputs.Length; i++)
			{
				concatenationIterator.Inputs[i] = ConvertAlgebraNode(node.Inputs[i]);

				List<IteratorOutput> inputOutputList = new List<IteratorOutput>();
				foreach (UnitedValueDefinition unitedValueDefinition in node.DefinedValues)
				{
					IteratorOutput iteratorOutput = new IteratorOutput();
					iteratorOutput.SourceIndex = Array.IndexOf(node.Inputs[i].OutputList, unitedValueDefinition.DependendEntries[i]);
					iteratorOutput.TargetIndex = Array.IndexOf(node.OutputList, unitedValueDefinition.Target);
					inputOutputList.Add(iteratorOutput);
				}
				concatenationIterator.InputOutput[i] = inputOutputList.ToArray();
			}
			SetLastIterator(node, concatenationIterator);

			return node;
		}

		public override AlgebraNode VisitSortAlgebraNode(SortAlgebraNode node)
		{
			SortIterator sortIterator;
			if (node.Distinct)
				sortIterator = new DistinctSortIterator();
			else 
				sortIterator = new SortIterator();

			sortIterator.RowBuffer = new object[node.OutputList.Length];
			sortIterator.SortOrders = node.SortOrders;
			sortIterator.SortEntries = GetIteratorInput(node.Input.OutputList, node.SortEntries);
			sortIterator.Comparers = GetComparersFromExpressionTypes(node.SortEntries);
			sortIterator.Input = ConvertAlgebraNode(node.Input);
			sortIterator.InputOutput = GetIteratorOutput(0, node.Input.OutputList, node.OutputList);
			SetLastIterator(node, sortIterator);

			return node;
		}

		public override AlgebraNode VisitAggregateAlgebraNode(AggregateAlgebraNode node)
		{
			StreamAggregateIterator streamAggregateIterator = new StreamAggregateIterator();
			streamAggregateIterator.RowBuffer = new object[node.OutputList.Length];
			streamAggregateIterator.Input = ConvertAlgebraNode(node.Input);
			streamAggregateIterator.InputOutput = GetIteratorOutput(0, node.Input.OutputList, node.OutputList);

			BoundRowBufferEntrySet boundRowBufferEntrySet = new BoundRowBufferEntrySet(streamAggregateIterator.Input.RowBuffer, node.Input.OutputList);

			if (node.Groups == null)
			{
				streamAggregateIterator.GroupByEntries = new IteratorInput[0];
				streamAggregateIterator.DefinedValues = GetDefinedValues(node.OutputList, node.DefinedValues, boundRowBufferEntrySet);
			}
			else
			{
				streamAggregateIterator.GroupByEntries = GetIteratorInput(node.Input.OutputList, node.Groups);
				streamAggregateIterator.DefinedValues = GetDefinedValues(node.OutputList, node.DefinedValues, boundRowBufferEntrySet);
			}

			SetLastIterator(node, streamAggregateIterator);

			return node;
		}

		public override AlgebraNode VisitTopAlgebraNode(TopAlgebraNode node)
		{
			TopIterator topIterator;

			if (node.TieEntries == null)
			{
				topIterator = new TopIterator();
			}
			else
			{
				TopWithTiesIterator topWithTiesIterator = new TopWithTiesIterator();
				topWithTiesIterator.TieEntries = GetIteratorInput(node.Input.OutputList, node.TieEntries);
				topIterator = topWithTiesIterator;
			}

			topIterator.RowBuffer = new object[node.OutputList.Length];
			topIterator.Limit = node.Limit;
			topIterator.Input = ConvertAlgebraNode(node.Input);
			topIterator.InputOutput = GetIteratorOutput(0, node.Input.OutputList, node.OutputList);
			SetLastIterator(node, topIterator);

			return node;
		}

		public override AlgebraNode VisitFilterAlgebraNode(FilterAlgebraNode node)
		{
			FilterIterator filterIterator = new FilterIterator();
			filterIterator.RowBuffer = new object[node.OutputList.Length];
			filterIterator.Input = ConvertAlgebraNode(node.Input);
			filterIterator.InputOutput = GetIteratorOutput(0, node.Input.OutputList, node.OutputList);
			BoundRowBufferEntrySet boundRowBufferEntrySet = new BoundRowBufferEntrySet(filterIterator.Input.RowBuffer, node.Input.OutputList);
			filterIterator.Predicate = CreateRuntimeExpression(node.Predicate, boundRowBufferEntrySet);
			SetLastIterator(node, filterIterator);

			return node;
		}

		public override AlgebraNode VisitComputeScalarAlgebraNode(ComputeScalarAlgebraNode node)
		{
			ComputeScalarIterator computeScalarIterator = new ComputeScalarIterator();
			computeScalarIterator.RowBuffer = new object[node.OutputList.Length];
			computeScalarIterator.Input = ConvertAlgebraNode(node.Input);
			computeScalarIterator.InputOutput = GetIteratorOutput(0, node.Input.OutputList, node.OutputList);

			BoundRowBufferEntrySet boundRowBufferEntrySet = new BoundRowBufferEntrySet(computeScalarIterator.Input.RowBuffer, node.Input.OutputList);
			computeScalarIterator.DefinedValues = GetDefinedValues(node.OutputList, node.DefinedValues, boundRowBufferEntrySet);

			SetLastIterator(node, computeScalarIterator);

			return node;
		}

		public override AlgebraNode VisitResultAlgebraNode(ResultAlgebraNode node)
		{
			ResultIterator resultIterator = new ResultIterator();
			resultIterator.RowBuffer = new object[node.OutputList.Length];
			resultIterator.Input = ConvertAlgebraNode(node.Input);
			resultIterator.InputOutput = new IteratorOutput[node.OutputList.Length];
			resultIterator.ColumnNames = new string[node.OutputList.Length];
			resultIterator.ColumnTypes = new Type[node.OutputList.Length];
			for (int i = 0; i < node.OutputList.Length; i++)
			{
				IteratorOutput iteratorOutput = new IteratorOutput();
				iteratorOutput.SourceIndex = Array.IndexOf(node.Input.OutputList, node.OutputList[i]);
				iteratorOutput.TargetIndex = i;
				resultIterator.InputOutput[i] = iteratorOutput;
				resultIterator.ColumnNames[i] = node.ColumnNames[i];
				resultIterator.ColumnTypes[i] = node.OutputList[i].DataType;
			}
			SetLastIterator(node, resultIterator);

			return node;
		}

		public override AlgebraNode VisitAssertAlgebraNode(AssertAlgebraNode node)
		{
			string message;
			switch(node.AssertionType)
			{
				case AssertionType.MaxOneRow:
					message = Resources.SubqueryReturnedMoreThanRow;
					break;
				case AssertionType.BelowRecursionLimit:
					message = Resources.MaximumRecursionLevelExceeded;
					break;
				default:
					throw ExceptionBuilder.UnhandledCaseLabel(node.AssertionType);
			}

			AssertIterator assertIterator = new AssertIterator();
			assertIterator.RowBuffer = new object[node.OutputList.Length];
			assertIterator.Input = ConvertAlgebraNode(node.Input);
			assertIterator.InputOutput = GetIteratorOutput(0, node.Input.OutputList, node.OutputList);
			assertIterator.Message = message;

			BoundRowBufferEntrySet boundRowBufferEntrySet = new BoundRowBufferEntrySet(assertIterator.Input.RowBuffer, node.Input.OutputList);
			assertIterator.Predicate = CreateRuntimeExpression(node.Predicate, boundRowBufferEntrySet);
			SetLastIterator(node, assertIterator);

			return node;
		}

		public override AlgebraNode VisitIndexSpoolAlgebraNode(IndexSpoolAlgebraNode node)
		{
			IndexSpoolIterator indexSpoolIterator = new IndexSpoolIterator();
			indexSpoolIterator.RowBuffer = new object[node.OutputList.Length];
			indexSpoolIterator.Input = ConvertAlgebraNode(node.Input);
			indexSpoolIterator.InputOutput = GetIteratorOutput(0, node.Input.OutputList, node.OutputList);
			indexSpoolIterator.IndexEntry = GetIteratorInput(node.Input.OutputList, new RowBufferEntry[] { node.IndexEntry })[0];

			BoundRowBufferEntrySet boundRowBufferEntrySet = new BoundRowBufferEntrySet(indexSpoolIterator.Input.RowBuffer, node.Input.OutputList);
			indexSpoolIterator.ProbeExpression = CreateRuntimeExpression(node.ProbeExpression, boundRowBufferEntrySet);

			SetLastIterator(node, indexSpoolIterator);

			return node;
		}

		private Dictionary<StackedTableSpoolAlgebraNode, TableSpoolIterator> tableSpoolIterators = new Dictionary<StackedTableSpoolAlgebraNode, TableSpoolIterator>();

		public override AstNode VisitStackedTableSpoolAlgebraNode(StackedTableSpoolAlgebraNode node)
		{
			TableSpoolIterator tableSpoolIterator = new TableSpoolIterator();
			tableSpoolIterators.Add(node, tableSpoolIterator);

			tableSpoolIterator.RowBuffer = new object[node.OutputList.Length];
			tableSpoolIterator.Input = ConvertAlgebraNode(node.Input);
			tableSpoolIterator.InputOutput = GetIteratorOutput(0, node.Input.OutputList, node.OutputList);
			SetLastIterator(node, tableSpoolIterator);

			return node;
		}

		public override AstNode VisitTableSpoolRefAlgebraNode(StackedTableSpoolRefAlgebraNode node)
		{
			TableSpoolRefIterator tableSpoolRefIterator = new TableSpoolRefIterator();
			tableSpoolRefIterator.RowBuffer = new object[node.OutputList.Length];
			tableSpoolRefIterator.PrimarySpool = tableSpoolIterators[node.PrimarySpool];
			SetLastIterator(node, tableSpoolRefIterator);

			return node;
		}

		public override AlgebraNode VisitHashMatchAlgebraNode(HashMatchAlgebraNode node)
		{
			HashMatchIterator hashMatchIterator = new HashMatchIterator();
			hashMatchIterator.RowBuffer = new object[node.OutputList.Length];
			hashMatchIterator.Left = ConvertAlgebraNode(node.Left);
			hashMatchIterator.LeftOutput = GetIteratorOutput(0, node.Left.OutputList, node.OutputList);
			hashMatchIterator.Right = ConvertAlgebraNode(node.Right);
			hashMatchIterator.RightOutput = GetIteratorOutput(hashMatchIterator.LeftOutput.Length, node.Right.OutputList, node.OutputList);
			hashMatchIterator.BuildKeyEntry = GetIteratorInput(node.Left.OutputList, new RowBufferEntry[] { node.BuildKeyEntry })[0];
			hashMatchIterator.ProbeEntry = GetIteratorInput(node.Right.OutputList, new RowBufferEntry[] { node.ProbeEntry })[0];

			switch (node.Op)
			{
				case JoinAlgebraNode.JoinOperator.InnerJoin:
					hashMatchIterator.LogicalOp = JoinType.Inner;
					break;
				case JoinAlgebraNode.JoinOperator.RightOuterJoin:
					hashMatchIterator.LogicalOp = JoinType.RightOuter;
					break;
				case JoinAlgebraNode.JoinOperator.FullOuterJoin:
					hashMatchIterator.LogicalOp = JoinType.FullOuter;
					break;
				default:
					throw ExceptionBuilder.UnhandledCaseLabel(node.Op);
			}

			UpdatePredicateRowBufferReferences(hashMatchIterator, node);
			SetLastIterator(node, hashMatchIterator);

			return node;
		}
	}
}