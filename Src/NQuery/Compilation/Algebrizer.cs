using System;
using System.Collections.Generic;
using System.Diagnostics;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class Algebrizer : StandardVisitor
	{
		private CommonTableBinding _currentCommonTableBinding;
		private AlgebraNode _lastAlgebraNode;

		private Algebrizer(CommonTableBinding currentCommonTableBinding)
		{
			_currentCommonTableBinding = currentCommonTableBinding;
		}

		#region Helpers

		public static ResultAlgebraNode Convert(QueryNode queryNode)
		{
			return Convert(null, queryNode);
		}

		private static ResultAlgebraNode Convert(CommonTableBinding currentCommonTableBinding, QueryNode queryNode)
		{
			Algebrizer algebrizer = new Algebrizer(currentCommonTableBinding);
			AlgebraNode result = algebrizer.ConvertAstNode(queryNode);
			result = new SubqueryExpander().VisitAlgebraNode(result);
			return (ResultAlgebraNode)result;
		}

		private void SetLastAlgebraNode(AlgebraNode algebraNode)
		{
			_lastAlgebraNode = algebraNode;
		}

		private AlgebraNode GetLastAlgebraNode()
		{
			AlgebraNode result = _lastAlgebraNode;
			_lastAlgebraNode = null;
			return result;
		}

		private AlgebraNode ConvertAstNode(AstNode astNode)
		{
			Visit(astNode);
			return GetLastAlgebraNode();
		}

		private static SortOrder[] CreateAscendingSortOrders(int length)
		{
			SortOrder[] result = new SortOrder[length];
			for (int i = 0; i < result.Length; i++)
				result[i] = SortOrder.Ascending;
			return result;
		}

		private void EmitComputeScalarIfNeeded(ComputedValueDefinition[] definedValues)
		{
			if (definedValues != null && definedValues.Length > 0)
			{
				ComputeScalarAlgebraNode computeScalarAlgebraNode = new ComputeScalarAlgebraNode();
				computeScalarAlgebraNode.Input = GetLastAlgebraNode();
				computeScalarAlgebraNode.DefinedValues = definedValues;
				SetLastAlgebraNode(computeScalarAlgebraNode);
			}
		}

		#endregion

		#region Common Table Expression Helpers

		private bool IsTableReferenceToCurrentCommonTableBinding(TableReference tableReference)
		{
			NamedTableReference namedTableReference = tableReference as NamedTableReference;
			if (namedTableReference == null)
				return false;

			return namedTableReference.TableRefBinding.TableBinding == _currentCommonTableBinding;
		}

		private class CteColumnMapping
		{
			public RowBufferEntry VirtualBufferEntry;
			public RowBufferEntry SpoolBufferEntry;
		}

		private class CteColumnMappingFinder : StandardVisitor
		{
			private CommonTableBinding _commonTableBinding;
			private RowBufferEntry[] _spoolBufferEntries;
			private List<CteColumnMapping> _mappings = new List<CteColumnMapping>();

			public CteColumnMappingFinder(CommonTableBinding commonTableBinding, RowBufferEntry[] spoolBufferEntries)
			{
				_commonTableBinding = commonTableBinding;
				_spoolBufferEntries = spoolBufferEntries;
			}

			public CteColumnMapping[] GetMappings()
			{
				return _mappings.ToArray();
			}

			public override TableReference VisitNamedTableReference(NamedTableReference node)
			{
				if (node.TableRefBinding.TableBinding == _commonTableBinding)
				{
					for (int i = 0; i < node.TableRefBinding.ColumnRefs.Length; i++)
					{
						CteColumnMapping columnMapping = new CteColumnMapping();
						columnMapping.VirtualBufferEntry = node.TableRefBinding.ColumnRefs[i].ValueDefinition.Target;
						columnMapping.SpoolBufferEntry = _spoolBufferEntries[i];
						_mappings.Add(columnMapping);
					}
				}

				return node;
			}
		}

		private class CteTableDefinedValuesReinitializer : StandardVisitor
		{
			private Dictionary<RowBufferEntry,RowBufferEntry> _rowBufferMappings = new Dictionary<RowBufferEntry, RowBufferEntry>();
			private RowBufferEntry ReplaceRowBuffer(RowBufferEntry oldRowBufferEntry)
			{
				RowBufferEntry newRowBufferEntry;
				if (_rowBufferMappings.TryGetValue(oldRowBufferEntry, out newRowBufferEntry))
					return newRowBufferEntry;
				return oldRowBufferEntry;
			}

			public override AstNode Visit(AstNode node)
			{
				node = base.Visit(node);

				AlgebraNode algebraNode = node as AlgebraNode;
				if (algebraNode != null && algebraNode.OutputList != null)
				{
					for (int i = 0; i < algebraNode.OutputList.Length; i++)
						algebraNode.OutputList[i] = ReplaceRowBuffer(algebraNode.OutputList[i]);
				}

				return node;
			}

			public override ExpressionNode VisitRowBufferEntryExpression(RowBufferEntryExpression expression)
			{
				return new RowBufferEntryExpression(ReplaceRowBuffer(expression.RowBufferEntry));
			}

			public override AlgebraNode VisitTableAlgebraNode(TableAlgebraNode node)
			{
				TableRefBinding oldTableRefBinding = node.TableRefBinding;
				TableRefBinding newTableRefBinding = new TableRefBinding(oldTableRefBinding.Scope, oldTableRefBinding.TableBinding, oldTableRefBinding.Name);

				List<ColumnValueDefinition> definedValues = new List<ColumnValueDefinition>();
				for (int i = 0; i < newTableRefBinding.ColumnRefs.Length; i++)
				{
					definedValues.Add(newTableRefBinding.ColumnRefs[i].ValueDefinition);
					RowBufferEntry oldRowBufferEntry = oldTableRefBinding.ColumnRefs[i].ValueDefinition.Target;
					RowBufferEntry newRowBufferEntry = newTableRefBinding.ColumnRefs[i].ValueDefinition.Target;
					_rowBufferMappings.Add(oldRowBufferEntry, newRowBufferEntry);
				}

				node.TableRefBinding = newTableRefBinding;
				node.DefinedValues = definedValues.ToArray();

				return node;
			}

			public override AlgebraNode VisitConcatAlgebraNode(ConcatAlgebraNode node)
			{
				base.VisitConcatAlgebraNode(node);

				foreach (UnitedValueDefinition definedValue in node.DefinedValues)
				{
					for (int i = 0; i < definedValue.DependendEntries.Length; i++)
						definedValue.DependendEntries[i] = ReplaceRowBuffer(definedValue.DependendEntries[i]);
				}

				return node;
			}
		}

		private static AlgebraNode AlgebrizeRecursiveCte(CommonTableBinding commonTableBinding)
		{
			// It is a recursive query.
			//
			// Create row buffer entry that is used to guard the recursion and the primary table spool
			// that spools the results needed by nested recursion calls.

			ExpressionBuilder expressionBuilder = new ExpressionBuilder();
			StackedTableSpoolAlgebraNode primaryTableSpool = new StackedTableSpoolAlgebraNode();

			RowBufferEntry anchorRecursionLevel;
			RowBufferEntry[] anchorOutput;
			AlgebraNode anchorNode;

			#region Anchor member
			{
				// Emit anchor member.

				AlgebraNode algebrizedAnchor = Convert(commonTableBinding.AnchorMember);

				// Emit compute scalar that initializes the recursion level to 0.

				anchorRecursionLevel = new RowBufferEntry(typeof(int));
				ComputedValueDefinition computedValueDefinition1 = new ComputedValueDefinition();
				computedValueDefinition1.Target = anchorRecursionLevel;
				computedValueDefinition1.Expression = LiteralExpression.FromInt32(0);

				ComputeScalarAlgebraNode computeScalarAlgebraNode = new ComputeScalarAlgebraNode();
				computeScalarAlgebraNode.Input = algebrizedAnchor;
				computeScalarAlgebraNode.DefinedValues = new ComputedValueDefinition[] { computedValueDefinition1 };

				anchorOutput = algebrizedAnchor.OutputList;
				anchorNode = computeScalarAlgebraNode;
			}
			#endregion

			RowBufferEntry incrementedRecursionLevel;
			RowBufferEntry[] tableSpoolOutput;
			AlgebraNode tableSpoolNode;

			#region Table spool
			{
				// Emit table spool reference.

				RowBufferEntry recursionLevelRefEntry = new RowBufferEntry(typeof(int));
				tableSpoolOutput = new RowBufferEntry[anchorOutput.Length];
				for (int i = 0; i < tableSpoolOutput.Length; i++)
					tableSpoolOutput[i] = new RowBufferEntry(anchorOutput[i].DataType);

				StackedTableSpoolRefAlgebraNode tableSpoolReference = new StackedTableSpoolRefAlgebraNode();
				tableSpoolReference.PrimarySpool = primaryTableSpool;
				tableSpoolReference.DefinedValues = ArrayHelpers.JoinArrays(new RowBufferEntry[] { recursionLevelRefEntry }, tableSpoolOutput);

				// Emit compute scalar that increases the recursion level by one and renames
				// columns from the spool to the CTE column buffer entries.

				expressionBuilder.Push(new RowBufferEntryExpression(recursionLevelRefEntry));
				expressionBuilder.Push(LiteralExpression.FromInt32(1));
				expressionBuilder.PushBinary(BinaryOperator.Add);

				incrementedRecursionLevel = new RowBufferEntry(typeof(int));
				ComputedValueDefinition incremenedRecLevelValueDefinition = new ComputedValueDefinition();
				incremenedRecLevelValueDefinition.Target = incrementedRecursionLevel;
				incremenedRecLevelValueDefinition.Expression = expressionBuilder.Pop();

				CteColumnMappingFinder cteColumnMappingFinder = new CteColumnMappingFinder(commonTableBinding, tableSpoolOutput);
				foreach (QueryNode recursiveMember in commonTableBinding.RecursiveMembers)
					cteColumnMappingFinder.Visit(recursiveMember);

				CteColumnMapping[] cteColumnMappings = cteColumnMappingFinder.GetMappings();

				List<ComputedValueDefinition> definedValues = new List<ComputedValueDefinition>();
				definedValues.Add(incremenedRecLevelValueDefinition);
				foreach (CteColumnMapping cteColumnMapping in cteColumnMappings)
				{
					ComputedValueDefinition definedValue = new ComputedValueDefinition();
					definedValue.Target = cteColumnMapping.VirtualBufferEntry;
					definedValue.Expression = new RowBufferEntryExpression(cteColumnMapping.SpoolBufferEntry);
					definedValues.Add(definedValue);
				}

				ComputeScalarAlgebraNode computeScalarAlgebraNode = new ComputeScalarAlgebraNode();
				computeScalarAlgebraNode.Input = tableSpoolReference;
				computeScalarAlgebraNode.DefinedValues = definedValues.ToArray();

				tableSpoolNode = computeScalarAlgebraNode;
			}
			#endregion

			RowBufferEntry[] recursiveOutput;
			AlgebraNode recursiveNode;

			#region Recursive member(s)
			{
				// Emit all recursive parts. The join conditions to the recursive part are replaced by simple filters
				// in the nested Convert() call.

				ConcatAlgebraNode concatAlgebraNode = new ConcatAlgebraNode();
				concatAlgebraNode.Inputs = new AlgebraNode[commonTableBinding.RecursiveMembers.Length];
				for (int i = 0; i < commonTableBinding.RecursiveMembers.Length; i++)
					concatAlgebraNode.Inputs[i] = Convert(commonTableBinding, commonTableBinding.RecursiveMembers[i]);

				concatAlgebraNode.DefinedValues = new UnitedValueDefinition[anchorOutput.Length];
				for (int i = 0; i < anchorOutput.Length; i++)
				{
					List<RowBufferEntry> dependencies = new List<RowBufferEntry>();
					foreach (ResultAlgebraNode algebrizedRecursivePart in concatAlgebraNode.Inputs)
						dependencies.Add(algebrizedRecursivePart.OutputList[i]);

					concatAlgebraNode.DefinedValues[i] = new UnitedValueDefinition();
					concatAlgebraNode.DefinedValues[i].Target = new RowBufferEntry(anchorOutput[i].DataType);
					concatAlgebraNode.DefinedValues[i].DependendEntries = dependencies.ToArray();
				}

				// Calculate the recursive output.

				recursiveOutput = new RowBufferEntry[concatAlgebraNode.DefinedValues.Length];
				for (int i = 0; i < concatAlgebraNode.DefinedValues.Length; i++)
					recursiveOutput[i] = concatAlgebraNode.DefinedValues[i].Target;

				// Emit cross join

				JoinAlgebraNode crossJoinNode = new JoinAlgebraNode();
				crossJoinNode.Left = tableSpoolNode;
				crossJoinNode.Right = concatAlgebraNode;

				// Emit assert that ensures that the recursion level is <= 100.

				expressionBuilder.Push(new RowBufferEntryExpression(incrementedRecursionLevel));
				expressionBuilder.Push(LiteralExpression.FromInt32(100));
				expressionBuilder.PushBinary(BinaryOperator.Greater);

				CaseExpression caseExpression = new CaseExpression();
				caseExpression.WhenExpressions = new ExpressionNode[1];
				caseExpression.WhenExpressions[0] = expressionBuilder.Pop();
				caseExpression.ThenExpressions = new ExpressionNode[1];
				caseExpression.ThenExpressions[0] = LiteralExpression.FromInt32(0);

				AssertAlgebraNode assertAlgebraNode = new AssertAlgebraNode();
				assertAlgebraNode.Input = crossJoinNode;
				assertAlgebraNode.AssertionType = AssertionType.BelowRecursionLimit;
				assertAlgebraNode.Predicate = caseExpression;

				recursiveNode = assertAlgebraNode;
			}
			#endregion

			RowBufferEntry[] algebrizedOutput;
			AlgebraNode algebrizedCte;

			#region Combination
			{
				// Create concat node to combine anchor and recursive part.

				ConcatAlgebraNode concatAlgebraNode = new ConcatAlgebraNode();
				concatAlgebraNode.Inputs = new AlgebraNode[2];
				concatAlgebraNode.Inputs[0] = anchorNode;
				concatAlgebraNode.Inputs[1] = recursiveNode;

				concatAlgebraNode.DefinedValues = new UnitedValueDefinition[anchorOutput.Length + 1];
				concatAlgebraNode.DefinedValues[0] = new UnitedValueDefinition();
				concatAlgebraNode.DefinedValues[0].Target = new RowBufferEntry(anchorRecursionLevel.DataType);
				concatAlgebraNode.DefinedValues[0].DependendEntries = new RowBufferEntry[] { anchorRecursionLevel, incrementedRecursionLevel };

				for (int i = 0; i < anchorOutput.Length; i++)
				{
					concatAlgebraNode.DefinedValues[i + 1] = new UnitedValueDefinition();
					concatAlgebraNode.DefinedValues[i + 1].Target = new RowBufferEntry(anchorOutput[i].DataType);
					concatAlgebraNode.DefinedValues[i + 1].DependendEntries = new RowBufferEntry[] { anchorOutput[i], recursiveOutput[i] };
				}

				algebrizedOutput = new RowBufferEntry[concatAlgebraNode.DefinedValues.Length];
				for (int i = 0; i < concatAlgebraNode.DefinedValues.Length; i++)
					algebrizedOutput[i] = concatAlgebraNode.DefinedValues[i].Target;

				// Assign the combination as the input to the primray spool

				primaryTableSpool.Input = concatAlgebraNode;

				// The primary spool represents the result of the "inlined" CTE.

				algebrizedCte = primaryTableSpool;
			}
			#endregion

			algebrizedCte.OutputList = algebrizedOutput;
			return algebrizedCte;
		}

		private static AlgebraNode InstantiateCte(AlgebraNode algebrizedCte, CommonTableBinding commonTableBinding, TableRefBinding commonTableRefBinding)
		{
			// Replace row buffers to base tables by new ones. This must be done because a CTE could be referenced multiple times.
			// Since same row buffer entries means that the underlying data will be stored in the same physical data slot this
			// will lead to problems if, for example, two instances of the same CTE are joined together. Any join condition that
			// operates on the same column will always compare data coming from the same join side (and therefor will always
			// evaluate to true).
			//
			// Some notes on the implementation:
			//
			//      1. Note that just replacing references to row buffers of base tables in RowBufferExpression is not enough;
			//         instead they must also be replaced in output lists, defined value references (esp. ConcatAlgebraNode) etc.
			//      2. Also note that although the QueryNodes are re-algebrized every time a CTE is references the expressions
			//         are still copied from the QueryNodes (instead of cloned). Therefore two algrebrized CTEs will share the same
			//         expression AST instances. That means that replacing the row buffers leads to failure.

			// HACK: This is a workaround for issue 2. However, 
			//       I am not quite sure how one should implement row buffer entry replacement without cloning the algebrized query.
			algebrizedCte = (AlgebraNode) algebrizedCte.Clone();

			CteTableDefinedValuesReinitializer cteTableDefinedValuesReinitializer = new CteTableDefinedValuesReinitializer();
			cteTableDefinedValuesReinitializer.Visit(algebrizedCte);

			RowBufferEntry[] outputList = algebrizedCte.OutputList;
			int skipRecursionLevel = commonTableBinding.IsRecursive ? 1 : 0;

			// Rename the query columns to the CTE columns
			List<ComputedValueDefinition> definedValues = new List<ComputedValueDefinition>();
			for (int i = 0; i < commonTableRefBinding.ColumnRefs.Length; i++)
			{
				RowBufferEntry targetRowBufferEntry = commonTableRefBinding.ColumnRefs[i].ValueDefinition.Target;
				RowBufferEntry sourceRowBufferEntry = outputList[i + skipRecursionLevel];

				ComputedValueDefinition definedValue = new ComputedValueDefinition();
				definedValue.Target = targetRowBufferEntry;
				definedValue.Expression = new RowBufferEntryExpression(sourceRowBufferEntry);
				definedValues.Add(definedValue);
			}

			ComputeScalarAlgebraNode computeScalarAlgebraNode = new ComputeScalarAlgebraNode();
			computeScalarAlgebraNode.Input = algebrizedCte;
			computeScalarAlgebraNode.DefinedValues = definedValues.ToArray();
			return computeScalarAlgebraNode;
		}

		#endregion

		public override TableReference VisitNamedTableReference(NamedTableReference node)
		{
			CommonTableBinding tableBindingAsCommonTable = node.TableRefBinding.TableBinding as CommonTableBinding;

			if (tableBindingAsCommonTable == null)
			{
				// It is a regular table reference. Emit just a table node.

				List<ColumnValueDefinition> definedValues = new List<ColumnValueDefinition>();
				foreach (ColumnRefBinding columnRefBinding in node.TableRefBinding.ColumnRefs)
					definedValues.Add(columnRefBinding.ValueDefinition);

				TableAlgebraNode algebraNode = new TableAlgebraNode();
				algebraNode.TableRefBinding = node.TableRefBinding;
				algebraNode.DefinedValues = definedValues.ToArray();
				SetLastAlgebraNode(algebraNode);
			}
			else
			{
				// It is a reference to a CTE. Instead of emitting a table node we have to replace the
				// reference to the CTE by the algebrized representation of the CTE. One could speak
				// of "inlining" the CTE.

				if (tableBindingAsCommonTable == _currentCommonTableBinding)
				{
					// This should never happen. The JoinAlgebraNode should ignore these table references.
					Debug.Fail("The current common table binding should never be reached as a table reference.");
				}

				AlgebraNode algebrizedCte;
				if (tableBindingAsCommonTable.IsRecursive)
					algebrizedCte = AlgebrizeRecursiveCte(tableBindingAsCommonTable);
				else
					algebrizedCte = Convert(tableBindingAsCommonTable.AnchorMember);

				// In order to use the CTE we have to instantiate it. See commonets in InstantiateCte() for
				// details.

				AlgebraNode computeScalarAlgebraNode = InstantiateCte(algebrizedCte, tableBindingAsCommonTable, node.TableRefBinding);
				SetLastAlgebraNode(computeScalarAlgebraNode);
			}

			return node;
		}

		public override TableReference VisitJoinedTableReference(JoinedTableReference node)
		{
			TableReference nonCteTableRef;
			if (IsTableReferenceToCurrentCommonTableBinding(node.Left))
				nonCteTableRef = node.Right;
			else if (IsTableReferenceToCurrentCommonTableBinding(node.Right))
				nonCteTableRef = node.Left;
			else
				nonCteTableRef = null;

			if (nonCteTableRef != null)
			{
				Debug.Assert(node.JoinType == JoinType.Inner);
				AlgebraNode algebrizedPath = ConvertAstNode(nonCteTableRef);
				SetLastAlgebraNode(algebrizedPath);

				if (node.Condition != null)
				{
					FilterAlgebraNode filterAlgebraNode = new FilterAlgebraNode();
					filterAlgebraNode.Input = GetLastAlgebraNode();
					filterAlgebraNode.Predicate = node.Condition;
					SetLastAlgebraNode(filterAlgebraNode);
				}
				return node;
			}

			JoinAlgebraNode algebraNode = new JoinAlgebraNode();
			algebraNode.Left = ConvertAstNode(node.Left);
			algebraNode.Right = ConvertAstNode(node.Right);
			algebraNode.Predicate = node.Condition;

			switch (node.JoinType)
			{
				case JoinType.Inner:
					algebraNode.Op = JoinAlgebraNode.JoinOperator.InnerJoin;
					break;
				case JoinType.LeftOuter:
					algebraNode.Op = JoinAlgebraNode.JoinOperator.LeftOuterJoin;
					break;
				case JoinType.RightOuter:
					algebraNode.Op = JoinAlgebraNode.JoinOperator.RightOuterJoin;
					break;
				case JoinType.FullOuter:
					algebraNode.Op = JoinAlgebraNode.JoinOperator.FullOuterJoin;
					break;
			}

			SetLastAlgebraNode(algebraNode);
			return node;
		}

		public override TableReference VisitDerivedTableReference(DerivedTableReference node)
		{
			AlgebraNode algebrizedQuery = Convert(node.Query);

			List<ComputedValueDefinition> definedValues = new List<ComputedValueDefinition>();
			for (int i = 0; i < node.DerivedTableBinding.ColumnRefs.Length; i++)
			{
				RowBufferEntry targetRowBufferEntry = node.DerivedTableBinding.ColumnRefs[i].ValueDefinition.Target;
				RowBufferEntry sourceRowBufferEntry = algebrizedQuery.OutputList[i];

				ComputedValueDefinition definedValue = new ComputedValueDefinition();
				definedValue.Target = targetRowBufferEntry;
				definedValue.Expression = new RowBufferEntryExpression(sourceRowBufferEntry);
				definedValues.Add(definedValue);
			}

			ComputeScalarAlgebraNode computeScalarAlgebraNode = new ComputeScalarAlgebraNode();
			computeScalarAlgebraNode.Input = algebrizedQuery;
			computeScalarAlgebraNode.DefinedValues = definedValues.ToArray();
			SetLastAlgebraNode(computeScalarAlgebraNode);

			return node;
		}

		public override QueryNode VisitSelectQuery(SelectQuery query)
		{
			// Calculate needed row buffers

			RowBufferCalculator rowBufferCalculator = query.RowBufferCalculator;

			// Emit FROM

			if (query.TableReferences != null)
			{
				Visit(query.TableReferences);
			}
			else
			{
				ConstantScanAlgebraNode constantScanAlgebraNode = new ConstantScanAlgebraNode();
				constantScanAlgebraNode.DefinedValues = new ComputedValueDefinition[0];
				SetLastAlgebraNode(constantScanAlgebraNode);
			}

			// Emit WHERE

			if (query.WhereClause != null)
			{
				FilterAlgebraNode filterAlgebraNode = new FilterAlgebraNode();
				filterAlgebraNode.Input = GetLastAlgebraNode();
				filterAlgebraNode.Predicate = query.WhereClause;
				SetLastAlgebraNode(filterAlgebraNode);
			}

			// Emit GROUP BY

			if (query.GroupByColumns != null || query.IsAggregated)
			{
				EmitComputeScalarIfNeeded(rowBufferCalculator.ComputedGroupColumns);

				List<AggregatedValueDefinition> definedValues = new List<AggregatedValueDefinition>();
				foreach (AggregateExpression aggregateDependency in query.AggregateDependencies)
					definedValues.Add(aggregateDependency.ValueDefinition);

				if (query.GroupByColumns != null)
				{
					SortAlgebraNode sortAlgebraNode = new SortAlgebraNode();
					sortAlgebraNode.Input = GetLastAlgebraNode();
					sortAlgebraNode.SortEntries = rowBufferCalculator.GroupColumns;
					sortAlgebraNode.SortOrders = CreateAscendingSortOrders(sortAlgebraNode.SortEntries.Length);
					SetLastAlgebraNode(sortAlgebraNode);
				}

				AggregateAlgebraNode algebraNode = new AggregateAlgebraNode();
				algebraNode.Input = GetLastAlgebraNode();
				algebraNode.DefinedValues = definedValues.ToArray();
				algebraNode.Groups = rowBufferCalculator.GroupColumns;
				SetLastAlgebraNode(algebraNode);
			}

			// Emit HAVING

			if (query.HavingClause != null)
			{
				FilterAlgebraNode filterAlgebraNode = new FilterAlgebraNode();
				filterAlgebraNode.Input = GetLastAlgebraNode();
				filterAlgebraNode.Predicate = query.HavingClause;
				SetLastAlgebraNode(filterAlgebraNode);
			}

			// Emit compute scalar to calculate expressions needed in SELECT and ORDER BY

			EmitComputeScalarIfNeeded(rowBufferCalculator.ComputedSelectAndOrderColumns);

			// Emit DISTINCT and ORDER BY

			if (query.IsDistinct && query.OrderByColumns != null)
			{
				List<RowBufferEntry> sortEntries = new List<RowBufferEntry>();
				List<SortOrder> sortOrderList = new List<SortOrder>();

				for (int i = 0; i < query.OrderByColumns.Length; i++)
				{
					sortEntries.Add(rowBufferCalculator.OrderColumns[i]);
					sortOrderList.Add(query.OrderByColumns[i].SortOrder);
				}

				foreach (RowBufferEntry selectColumn in rowBufferCalculator.SelectColumns)
				{
					bool selectColumnMustBeSorted = !sortEntries.Contains(selectColumn);
					if (selectColumnMustBeSorted)
					{
						sortEntries.Add(selectColumn);
						sortOrderList.Add(SortOrder.Ascending);
					}
				}

				SortAlgebraNode sortAlgebraNode = new SortAlgebraNode();
				sortAlgebraNode.Distinct = true;
				sortAlgebraNode.Input = GetLastAlgebraNode();
				sortAlgebraNode.SortEntries = sortEntries.ToArray();
				sortAlgebraNode.SortOrders = sortOrderList.ToArray();

				SetLastAlgebraNode(sortAlgebraNode);
			}
			else
			{
				if (query.IsDistinct)
				{
					SortAlgebraNode sortAlgebraNode = new SortAlgebraNode();
					sortAlgebraNode.Distinct = true;
					sortAlgebraNode.Input = GetLastAlgebraNode();
					sortAlgebraNode.SortEntries = rowBufferCalculator.SelectColumns;
					sortAlgebraNode.SortOrders = CreateAscendingSortOrders(sortAlgebraNode.SortEntries.Length);
					SetLastAlgebraNode(sortAlgebraNode);
				}

				if (query.OrderByColumns != null)
				{
					List<SortOrder> sortOrderList = new List<SortOrder>();
					foreach (OrderByColumn orderByColumn in query.OrderByColumns)
						sortOrderList.Add(orderByColumn.SortOrder);

					SortAlgebraNode sortAlgebraNode = new SortAlgebraNode();
					sortAlgebraNode.Input = GetLastAlgebraNode();
					sortAlgebraNode.SortEntries = rowBufferCalculator.OrderColumns;
					sortAlgebraNode.SortOrders = sortOrderList.ToArray();
					SetLastAlgebraNode(sortAlgebraNode);
				}
			}

			// Emit TOP

			if (query.TopClause != null)
			{
				TopAlgebraNode algebraNode = new TopAlgebraNode();
				algebraNode.Input = GetLastAlgebraNode();
				algebraNode.Limit = query.TopClause.Value;

				if (query.TopClause.WithTies)
					algebraNode.TieEntries = rowBufferCalculator.OrderColumns;

				SetLastAlgebraNode(algebraNode);
			}

			// Emit select list

			List<string> columnNames = new List<string>();
			foreach (SelectColumn columnSource in query.SelectColumns)
			{
				if (columnSource.Alias != null)
					columnNames.Add(columnSource.Alias.Text);
				else
					columnNames.Add(null);
			}

			ResultAlgebraNode resultAlgebraNode = new ResultAlgebraNode();
			resultAlgebraNode.Input = GetLastAlgebraNode();
			resultAlgebraNode.OutputList = rowBufferCalculator.SelectColumns;
			resultAlgebraNode.ColumnNames = columnNames.ToArray();
			SetLastAlgebraNode(resultAlgebraNode);

			return query;
		}

		public override QueryNode VisitBinaryQuery(BinaryQuery query)
		{
			switch (query.Op)
			{
				case BinaryQueryOperator.Intersect:
				case BinaryQueryOperator.Except:
				{
					ResultAlgebraNode left = ((ResultAlgebraNode)ConvertAstNode(query.Left));
					ResultAlgebraNode right = ((ResultAlgebraNode)ConvertAstNode(query.Right));

					// Create distinct sort

					SortAlgebraNode sortAlgebraNode = new SortAlgebraNode();
					sortAlgebraNode.Distinct = true;
					sortAlgebraNode.Input = left;
					sortAlgebraNode.SortEntries = left.OutputList;
					sortAlgebraNode.SortOrders = CreateAscendingSortOrders(sortAlgebraNode.SortEntries.Length);

					// Insert left (anti) semi join to (except) intersect left and right.

					ExpressionBuilder expressionBuilder = new ExpressionBuilder();
					for (int i = 0; i < left.OutputList.Length; i++)
					{
						RowBufferEntryExpression leftExpr = new RowBufferEntryExpression();
						leftExpr.RowBufferEntry = left.OutputList[i];

						RowBufferEntryExpression rightExpr = new RowBufferEntryExpression();
						rightExpr.RowBufferEntry = right.OutputList[i];

						expressionBuilder.Push(leftExpr);
						expressionBuilder.Push(rightExpr);
						expressionBuilder.PushBinary(BinaryOperator.Equal);
						expressionBuilder.Push(leftExpr);
						expressionBuilder.PushIsNull();
						expressionBuilder.Push(rightExpr);
						expressionBuilder.PushIsNull();
						expressionBuilder.PushBinary(BinaryOperator.LogicalAnd);
						expressionBuilder.PushBinary(BinaryOperator.LogicalOr);
					}
					expressionBuilder.PushNAry(LogicalOperator.And);
					ExpressionNode joinCondition = expressionBuilder.Pop();

					JoinAlgebraNode joinAlgebraNode = new JoinAlgebraNode();
					if (query.Op == BinaryQueryOperator.Intersect)
						joinAlgebraNode.Op = JoinAlgebraNode.JoinOperator.LeftSemiJoin;
					else
						joinAlgebraNode.Op = JoinAlgebraNode.JoinOperator.LeftAntiSemiJoin;
					joinAlgebraNode.Left = sortAlgebraNode;
					joinAlgebraNode.Right = right;
					joinAlgebraNode.Predicate = joinCondition;
					SetLastAlgebraNode(joinAlgebraNode);

					ResultAlgebraNode resultAlgebraNode = new ResultAlgebraNode();
					resultAlgebraNode.Input = GetLastAlgebraNode();
					resultAlgebraNode.OutputList = left.OutputList;
					resultAlgebraNode.ColumnNames = left.ColumnNames;
					SetLastAlgebraNode(resultAlgebraNode);
					break;
				}

				case BinaryQueryOperator.Union:
				case BinaryQueryOperator.UnionAll:
				{
					// Build a flat list with all inputs.

					List<QueryNode> inputList = AstUtil.FlattenBinaryQuery(query);

					AlgebraNode[] inputs = new AlgebraNode[inputList.Count];
					for (int i = 0; i < inputs.Length; i++)
						inputs[i] = ConvertAstNode(inputList[i]);

					int outputColumnCount = inputs[0].OutputList.Length;

					UnitedValueDefinition[] definedValues = new UnitedValueDefinition[outputColumnCount];
					List<RowBufferEntry> definedValueEntries = new List<RowBufferEntry>();

					for (int i = 0; i < outputColumnCount; i++)
					{
						RowBufferEntry rowBufferEntry = new RowBufferEntry(inputs[0].OutputList[i].DataType);
						definedValueEntries.Add(rowBufferEntry);

						UnitedValueDefinition definedValue = new UnitedValueDefinition();
						definedValue.Target = rowBufferEntry;

						List<RowBufferEntry> dependencies = new List<RowBufferEntry>();
						foreach (ResultAlgebraNode node in inputs)
							dependencies.Add(node.OutputList[i]);
						definedValue.DependendEntries = dependencies.ToArray();

						definedValues[i] = definedValue;
					}

					ConcatAlgebraNode concatAlgebraNode = new ConcatAlgebraNode();
					concatAlgebraNode.Inputs = inputs;
					concatAlgebraNode.DefinedValues = definedValues;
					SetLastAlgebraNode(concatAlgebraNode);

					if (query.Op == BinaryQueryOperator.Union)
					{
						SortAlgebraNode sortAlgebraNode = new SortAlgebraNode();
						sortAlgebraNode.Distinct = true;
						sortAlgebraNode.Input = GetLastAlgebraNode();
						sortAlgebraNode.SortEntries = definedValueEntries.ToArray();
						sortAlgebraNode.SortOrders = CreateAscendingSortOrders(sortAlgebraNode.SortEntries.Length);
						SetLastAlgebraNode(sortAlgebraNode);
					}

					ResultAlgebraNode unionResultAlgebraNode = new ResultAlgebraNode();
					unionResultAlgebraNode.Input = GetLastAlgebraNode();
					unionResultAlgebraNode.ColumnNames = ((ResultAlgebraNode)inputs[0]).ColumnNames;
					unionResultAlgebraNode.OutputList = definedValueEntries.ToArray();
					SetLastAlgebraNode(unionResultAlgebraNode);

					break;
				}
			}

			return query;
		}

		public override QueryNode VisitSortedQuery(SortedQuery query)
		{
			ResultAlgebraNode input = (ResultAlgebraNode)ConvertAstNode(query.Input);

			List<RowBufferEntry> sortColumns = new List<RowBufferEntry>();
			List<SortOrder> sortOrders = new List<SortOrder>();
			foreach (OrderByColumn orderByColumn in query.OrderByColumns)
			{
				RowBufferEntry sortColumn = input.OutputList[orderByColumn.ColumnIndex];
				sortColumns.Add(sortColumn);
				sortOrders.Add(orderByColumn.SortOrder);
			}

			SortAlgebraNode sortAlgebraNode = new SortAlgebraNode();
			sortAlgebraNode.Input = input.Input;
			sortAlgebraNode.SortEntries = sortColumns.ToArray();
			sortAlgebraNode.SortOrders = sortOrders.ToArray();
			input.Input = sortAlgebraNode;

			SetLastAlgebraNode(input);

			return query;
		}

		public override QueryNode VisitCommonTableExpressionQuery(CommonTableExpressionQuery query)
		{
			// Here we don't visit the common table expressions. These are only algebrized if they
			// are referenced by the actual query.
			Visit(query.Input);
			return query;
		}
	}
}
