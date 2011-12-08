using System;
using System.Collections.Generic;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal static class AstUtil
	{
		public static bool IsNull(ExpressionNode expression)
		{
			ConstantExpression constantExpression = expression as ConstantExpression;
			return (constantExpression != null && constantExpression.IsNullValue);
		}

		public static BinaryOperator NegateBinaryOp(BinaryOperator op)
		{
			if (op == BinaryOperator.LogicalAnd)
				return BinaryOperator.LogicalOr;

			if (op == BinaryOperator.LogicalOr)
				return BinaryOperator.LogicalAnd;

			if (op == BinaryOperator.Equal)
				return BinaryOperator.NotEqual;

			if (op == BinaryOperator.NotEqual)
				return BinaryOperator.Equal;

			if (op == BinaryOperator.Greater)
				return BinaryOperator.LessOrEqual;

			if (op == BinaryOperator.GreaterOrEqual)
				return BinaryOperator.Less;

			if (op == BinaryOperator.Less)
				return BinaryOperator.GreaterOrEqual;

			if (op == BinaryOperator.LessOrEqual)
				return BinaryOperator.Greater;

			return null;
		}

		public static int FindStructuralEquivalentExpression(ExpressionNode[] expressions, ExpressionNode expr)
		{
			for (int i = 0; i < expressions.Length; i++)
			{
				ExpressionNode node = expressions[i];

				if (node.IsStructuralEqualTo(expr))
					return i;
			}

			return -1;
		}

		public static ExpressionNode CombineConditions(LogicalOperator logicalOperator, params ExpressionNode[] conditions)
		{
			return CombineConditions(logicalOperator, (IList<ExpressionNode>)conditions);
		}

		public static ExpressionNode CombineConditions(LogicalOperator logicalOperator, IList<ExpressionNode> conditions)
		{
			if (conditions == null || conditions.Count == 0)
				return null;

			ExpressionBuilder expressionBuilder = new ExpressionBuilder();
			foreach (ExpressionNode condition in conditions)
			{
				if (condition != null)
					expressionBuilder.Push(condition);
			}

			if (expressionBuilder.Count == 0)
				return null;

			expressionBuilder.PushNAry(logicalOperator);
			return expressionBuilder.Pop();
		}

		public static ExpressionNode[] SplitCondition(LogicalOperator logicalOperator, ExpressionNode condition)
		{
			PartExtractor partExtractor = new PartExtractor(logicalOperator);
			partExtractor.Visit(condition);
			return partExtractor.GetParts();
		}

		public static RowBufferEntry[] GetDefinedValueEntries(AlgebraNode node)
		{
			DefinedValuesFinder definedValuesFinder = new DefinedValuesFinder();
			definedValuesFinder.Visit(node);
			return definedValuesFinder.GetDefinedValueEntries();
		}

		public static bool JoinDoesNotDependOn(JoinAlgebraNode joinNode, AlgebraNode joinSide)
		{
			RowBufferEntry[] joinSideDefinedValues = GetDefinedValueEntries(joinSide);

			if (joinNode.Predicate != null)
			{
				RowBufferEntry[] referencedRowBufferEntries = GetRowBufferEntryReferences(joinNode.Predicate);

				foreach (RowBufferEntry joinSideDefinedValue in joinSideDefinedValues)
				{
					if (ArrayHelpers.Contains(referencedRowBufferEntries, joinSideDefinedValue))
						return false;
				}
			}

			return true;
		}

		public static bool WillProduceAtLeastOneRow(AlgebraNode algebraNode)
		{
			AtLeastOneRowChecker atLeastOneRowChecker = new AtLeastOneRowChecker();
			atLeastOneRowChecker.Visit(algebraNode);
			return atLeastOneRowChecker.WillProduceAtLeastOneRow;
		}

		public static bool WillProduceAtMostOneRow(AlgebraNode algebraNode)
		{
			AtMostOneRowChecker atMostOneRowChecker = new AtMostOneRowChecker();
			atMostOneRowChecker.Visit(algebraNode);
			return atMostOneRowChecker.WillProduceAtMostOneRow;
		}

		public static RowBufferEntry[] GetOuterReferences(JoinAlgebraNode node)
		{
			RowBufferEntry[] leftDefinedValues = GetDefinedValueEntries(node.Left);

			RowBufferEntry[] rowBufferEntries = GetRowBufferEntryReferences(node.Right);
			List<RowBufferEntry> outerReferenceList = new List<RowBufferEntry>();

			foreach (RowBufferEntry rightColumnDependency in rowBufferEntries)
			{
				if (ArrayHelpers.Contains(leftDefinedValues, rightColumnDependency))
					outerReferenceList.Add(rightColumnDependency);
			}

			return outerReferenceList.ToArray();
		}

		private sealed class RowBufferEntryReferenceFinder : StandardVisitor
		{
			private List<RowBufferEntry> _rowBufferEntries = new List<RowBufferEntry>();

			public override ExpressionNode VisitRowBufferEntryExpression(RowBufferEntryExpression expression)
			{
				if (!_rowBufferEntries.Contains(expression.RowBufferEntry))
					_rowBufferEntries.Add(expression.RowBufferEntry);

				return base.VisitRowBufferEntryExpression(expression);
			}

			public RowBufferEntry[] GetReferences()
			{
				return _rowBufferEntries.ToArray();
			}
		}

		public static RowBufferEntry[] GetRowBufferEntryReferences(AstNode astNode)
		{
			RowBufferEntryReferenceFinder rowBufferEntryReferenceFinder = new RowBufferEntryReferenceFinder();
			rowBufferEntryReferenceFinder.Visit(astNode);
			return rowBufferEntryReferenceFinder.GetReferences();
		}

		public static bool ContainsSubselect(AstNode astNode)
		{
			MetaInfo metaInfo = GetMetaInfo(astNode);
			return metaInfo.ContainsExistenceSubselects || metaInfo.ContainsSingleRowSubselects;
		}

		public static MetaInfo GetMetaInfo(AstNode astNode)
		{
			MetaInfoFinder metaInfoFinder = new MetaInfoFinder();
			metaInfoFinder.Visit(astNode);
			return metaInfoFinder.GetMetaInfo();
		}

		public static ColumnRefBinding[] GetUngroupedAndUnaggregatedColumns(ExpressionNode[] groupByExpressions, ExpressionNode expression)
		{
			UngroupedAndUnaggregatedColumnFinder ungroupedAndUnaggregatedColumnFinder = new UngroupedAndUnaggregatedColumnFinder(groupByExpressions);
			ungroupedAndUnaggregatedColumnFinder.Visit(expression);
			return ungroupedAndUnaggregatedColumnFinder.GetColumns();
		}

		public static bool ExpressionYieldsNullOrFalseIfRowBufferEntryNull(ExpressionNode expressionNode, RowBufferEntry rowBufferEntry)
		{
			NullRejectionChecker checker = new NullRejectionChecker(rowBufferEntry);
			checker.Visit(expressionNode);

			if (checker.ExpressionRejectsNull)
				return true;

			return false;
		}

		public static bool AllowsLeftPushOver(JoinAlgebraNode.JoinOperator joinOperator)
		{
			return joinOperator == JoinAlgebraNode.JoinOperator.InnerJoin ||
				   joinOperator == JoinAlgebraNode.JoinOperator.LeftOuterJoin ||
				   joinOperator == JoinAlgebraNode.JoinOperator.LeftSemiJoin ||
				   joinOperator == JoinAlgebraNode.JoinOperator.RightSemiJoin ||
				   joinOperator == JoinAlgebraNode.JoinOperator.LeftAntiSemiJoin ||
				   joinOperator == JoinAlgebraNode.JoinOperator.RightAntiSemiJoin;
		}

		public static bool AllowsRightPushOver(JoinAlgebraNode.JoinOperator joinOperator)
		{
			return joinOperator == JoinAlgebraNode.JoinOperator.InnerJoin ||
				   joinOperator == JoinAlgebraNode.JoinOperator.RightOuterJoin ||
				   joinOperator == JoinAlgebraNode.JoinOperator.LeftSemiJoin ||
				   joinOperator == JoinAlgebraNode.JoinOperator.RightSemiJoin ||
				   joinOperator == JoinAlgebraNode.JoinOperator.LeftAntiSemiJoin ||
				   joinOperator == JoinAlgebraNode.JoinOperator.RightAntiSemiJoin;
		}

		public static bool AllowsLeftPushDown(JoinAlgebraNode.JoinOperator joinOperator)
		{
			return joinOperator == JoinAlgebraNode.JoinOperator.InnerJoin ||
				   joinOperator == JoinAlgebraNode.JoinOperator.RightOuterJoin ||
				   joinOperator == JoinAlgebraNode.JoinOperator.LeftSemiJoin ||
				   joinOperator == JoinAlgebraNode.JoinOperator.RightSemiJoin ||
				   joinOperator == JoinAlgebraNode.JoinOperator.LeftAntiSemiJoin ||
				   joinOperator == JoinAlgebraNode.JoinOperator.RightAntiSemiJoin;
		}

		public static bool AllowsRightPushDown(JoinAlgebraNode.JoinOperator joinOperator)
		{
			return joinOperator == JoinAlgebraNode.JoinOperator.InnerJoin ||
				   joinOperator == JoinAlgebraNode.JoinOperator.LeftOuterJoin ||
				   joinOperator == JoinAlgebraNode.JoinOperator.LeftSemiJoin ||
				   joinOperator == JoinAlgebraNode.JoinOperator.RightSemiJoin ||
				   joinOperator == JoinAlgebraNode.JoinOperator.LeftAntiSemiJoin ||
				   joinOperator == JoinAlgebraNode.JoinOperator.RightAntiSemiJoin;
		}

		public static bool ExpressionDoesNotReference(ExpressionNode expression, RowBufferEntry[] rowBufferEntries)
		{
			RowBufferEntry[] rowBufferEntryReferences = GetRowBufferEntryReferences(expression);

			return DoesNotReference(rowBufferEntryReferences, rowBufferEntries);
		}

		public static bool DoesNotReference(RowBufferEntry[] rowBufferEntryReferences, RowBufferEntry[] rowBufferEntries)
		{
			foreach (RowBufferEntry rowBufferColumnDependency in rowBufferEntryReferences)
			{
				if (ArrayHelpers.Contains(rowBufferEntries, rowBufferColumnDependency))
					return false;
			}

			return true;
		}

		public static List<QueryNode> FlattenBinaryQuery(BinaryQuery binaryQuery)
		{
			List<QueryNode> inputList = new List<QueryNode>();

			inputList.Add(binaryQuery.Left);
			inputList.Add(binaryQuery.Right);

			bool atLeastOneExpanded;

			do
			{
				atLeastOneExpanded = false;
				for (int i = inputList.Count - 1; i >= 0; i--)
				{
					BinaryQuery inputAsBinaryQuery = inputList[i] as BinaryQuery;

					if (inputAsBinaryQuery != null && inputAsBinaryQuery.Op == binaryQuery.Op)
					{
						inputList.RemoveAt(i);
						inputList.Insert(i, inputAsBinaryQuery.Left);
						inputList.Insert(i + 1, inputAsBinaryQuery.Right);
						atLeastOneExpanded = true;
					}
				}
			} while (atLeastOneExpanded);
			return inputList;
		}

		public static QueryNode CombineQueries(List<QueryNode> members, BinaryQueryOperator combineOperator)
		{
			if (members.Count == 0)
				return null;

			QueryNode currentNode = members[0];
			for (int i = 1; i < members.Count; i++)
			{
				BinaryQuery binaryQuery = new BinaryQuery();
				binaryQuery.Left = currentNode;
				binaryQuery.Right = members[i];
				binaryQuery.Op = combineOperator;
				currentNode = binaryQuery;
			}
			return currentNode;
		}
	}
}