using System;
using System.Collections.Generic;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class JoinOrderOptimizer : StandardVisitor
	{
		public JoinOrderOptimizer()
		{
		}

		#region Helpers

		private static JoinOrder GetBestJoinOrder(TableRefBinding[] tables, JoinCondition[] joinConditions, ICollection<ExpressionNode> andParts)
		{
			// Compute all join orders

			JoinOrderGenerator generator = new JoinOrderGenerator(joinConditions, tables);
			JoinOrder[] allJoinOrders = generator.GenerateAll();

			// Compute best join order

			JoinOrder bestJoinOrder = GetBestJoinOrder(allJoinOrders, andParts, tables);
			return bestJoinOrder;
		}

		private static JoinOrder GetBestJoinOrder(JoinOrder[] orders, ICollection<ExpressionNode> andParts, TableRefBinding[] tables)
		{
			// OK, now we try to find the best join order.
			//
			// To choose the best join order we will use a point system and score each possibility.
			// The more points a possibility has the better it is.
			//
			// To ensure we exclude as much rows as possible and as early as possible
			// we will use the count of conditions applicable to each combinations.
			//
			// A condition is said to be applicable if all referenced tables are already
			// joined. For example if we have to join four tables T1, T2, T3, T4. Accordding to
			// the join order T1 -> T2 -> T3 -> T4 the condition c is said to be applicable
			// at join index 0 if it only references columns of table T1, at index 1 if it
			// only references columns of tables T1 and T2 and so on.

			// Determine minimal count of cross joins

			int minCrossJoinCount = int.MaxValue;

			for (int i = 0; i < orders.Length; i++)
			{
				int crossJoinCount = GetCrossJoinCount(orders[i]);

				if (crossJoinCount < minCrossJoinCount)
					minCrossJoinCount = crossJoinCount;
			}

			int[] points = new int[orders.Length];

			for (int i = 0; i < orders.Length; i++)
			{
				points[i] = GetJoinOrderScore(orders[i], andParts, tables);

				if (GetCrossJoinCount(orders[i]) > minCrossJoinCount)
				{
					// If the current join order uses more cross joins than
					// the minimal one we reject this join order by setting
					// its score to minus one.
					points[i] = -1;
				}
			}

			Array.Sort(points, orders);
			JoinOrder bestJoinOrder = orders[orders.Length - 1];

			return bestJoinOrder;
		}

		private static int GetCrossJoinCount(JoinOrder joinOrder)
		{
			int result = 0;

			for (int i = 0; i < joinOrder.Joins.Length; i++)
			{
				if (i > 0 && joinOrder.Joins[i].JoinCondition == null)
					result++;
			}

			return result;
		}

		private static int GetJoinOrderScore(JoinOrder joinOder, ICollection<ExpressionNode> andParts, TableRefBinding[] tables)
		{
			int result = 0;

			Dictionary<RowBufferEntry, ColumnValueDefinition> introducedColumns = GetIntroducedColumns(joinOder);
            
			for (int joinIndex = 0; joinIndex < tables.Length; joinIndex++)
			{
				ExpressionNode[] applicableConditions = GetAndPartsApplicableToJoin(introducedColumns, joinOder, joinIndex, andParts, false);

				result *= 2;
				result += applicableConditions.Length;

				if (joinOder.Joins[joinIndex].JoinCondition != null)
					result += 1;
			}

			return result;
		}

		private static Dictionary<RowBufferEntry, ColumnValueDefinition> GetIntroducedColumns(JoinOrder bestJoinOrder)
		{
			Dictionary<RowBufferEntry, ColumnValueDefinition> introducedColumns = new Dictionary<RowBufferEntry, ColumnValueDefinition>();

			foreach (Join join in bestJoinOrder.Joins)
			{
				foreach (ColumnRefBinding columnRefBinding in join.TableRefBinding.ColumnRefs)
					introducedColumns.Add(columnRefBinding.ValueDefinition.Target, columnRefBinding.ValueDefinition);
			}

			return introducedColumns;
		}

		private static ExpressionNode ExtractConditionsApplicableToTable(IDictionary<RowBufferEntry, ColumnValueDefinition> introducedColumns, IList<ExpressionNode> conditionList, TableRefBinding tableRefBinding)
		{
			List<ExpressionNode> applicableConditionList = new List<ExpressionNode>();

			for (int i = conditionList.Count - 1; i >= 0; i--)
			{
				ExpressionNode condition = conditionList[i];
				RowBufferEntry[] rowBufferEntries = AstUtil.GetRowBufferEntryReferences(condition);

				bool dependentOnOtherTable = false;
				foreach (RowBufferEntry rowBufferEntry in rowBufferEntries)
				{
					ColumnValueDefinition columnValueDefinition;
					if (introducedColumns.TryGetValue(rowBufferEntry, out columnValueDefinition))
					{
						if (columnValueDefinition.ColumnRefBinding.TableRefBinding != tableRefBinding)
						{
							dependentOnOtherTable = true;
							break;
						}
					}
				}

				if (!dependentOnOtherTable)
				{
					conditionList.RemoveAt(i);
					applicableConditionList.Add(condition);
				}
			}

			if (applicableConditionList.Count == 0)
				return null;

			ExpressionNode[] applicableConditions = applicableConditionList.ToArray();
			return AstUtil.CombineConditions(LogicalOperator.And, applicableConditions);
		}

		private static ExpressionNode[] GetAndPartsApplicableToJoin(IDictionary<RowBufferEntry, ColumnValueDefinition> introducedColumns, JoinOrder joinOrder, int joinIndex, ICollection<ExpressionNode> andParts, bool removeApplicableAndParts)
		{
			List<RowBufferEntry> columnsInJoin = new List<RowBufferEntry>();

			for( int i = 0; i <= joinIndex; i++)
			{
				foreach (ColumnRefBinding columnRefBinding in joinOrder.Joins[i].TableRefBinding.ColumnRefs)
					columnsInJoin.Add(columnRefBinding.ValueDefinition.Target);
			}

			List<ExpressionNode> applicableAndParts = new List<ExpressionNode>();

			foreach (ExpressionNode andPart in andParts)
			{
				RowBufferEntry[] rowBufferEntries = AstUtil.GetRowBufferEntryReferences(andPart);

				bool allDependenciesInJoin = true;
				foreach (RowBufferEntry entry in rowBufferEntries)
				{
					if (!columnsInJoin.Contains(entry))
					{
						// Row buffer entry not available in the join the current position.
						// It could be an outer reference. Check if the row buffer entry
						// is in the set of introduced row buffer entries.

						if (introducedColumns.ContainsKey(entry))
						{
							allDependenciesInJoin = false;
							break;
						}
					}
				}

				if (allDependenciesInJoin)
					applicableAndParts.Add(andPart);
					
			}

			if (removeApplicableAndParts)
			{
				foreach (ExpressionNode andPart in applicableAndParts)
					andParts.Remove(andPart);
			}

			return applicableAndParts.ToArray();
		}

		private static JoinOperator JoinOperatorFromBinaryOperator(BinaryOperator op)
		{
			// TODO: Uncomment the following lines if we can create optimized nodes
			//       for other joins than equijoin.

			if (op == BinaryOperator.Equal)
				return JoinOperator.Equal;
//			else if (expression.Op == BinaryOperator.Less)
//				return JoinOperator.Less;
//			else if (expression.Op == BinaryOperator.LessOrEqual)
//				return JoinOperator.LessOrEqual;
//			else if (expression.Op == BinaryOperator.Greater)
//				return JoinOperator.Greater;
//			else if (expression.Op == BinaryOperator.GreaterOrEqual)
//				return JoinOperator.GreaterOrEqual;
			else
				return JoinOperator.None;
		}

		private static TableRefBinding[] GetTableReferences(IDictionary<RowBufferEntry, ColumnRefBinding> rowBufferColumnDictionary, ExpressionNode expression)
		{
			List<TableRefBinding> result = new List<TableRefBinding>();

			RowBufferEntry[] rowBufferEntries = AstUtil.GetRowBufferEntryReferences(expression);
			foreach (RowBufferEntry rowBufferEntry in rowBufferEntries)
			{
				ColumnRefBinding columnRefBinding;
				if (rowBufferColumnDictionary.TryGetValue(rowBufferEntry, out columnRefBinding))
				{
					if (!result.Contains(columnRefBinding.TableRefBinding))
						result.Add(columnRefBinding.TableRefBinding);
				}
			}

			return result.ToArray();
		}

		private static JoinCondition ConvertToJoinCondition(IDictionary<RowBufferEntry, ColumnRefBinding> rowBufferColumnDictionary, ExpressionNode expression)
		{
			BinaryExpression binaryExpression = expression as BinaryExpression;
			if (binaryExpression == null)
				return null;

			JoinOperator joinOperator = JoinOperatorFromBinaryOperator(binaryExpression.Op);
			if (joinOperator == JoinOperator.None)
				return null;

			TableRefBinding[] leftTableReferences = GetTableReferences(rowBufferColumnDictionary, binaryExpression.Left);
			TableRefBinding[] rightTableReferences = GetTableReferences(rowBufferColumnDictionary, binaryExpression.Right);

			if (leftTableReferences.Length == 1 && rightTableReferences.Length == 1)
			{
				if (leftTableReferences[0] != rightTableReferences[0])
				{
					// Both expressions depend on extactly one table reference but they are not refering to the same table.
					//
					// That means we found a join condition.

					JoinCondition joinCondition = new JoinCondition();
					joinCondition.Op = joinOperator;
					joinCondition.LeftExpression = binaryExpression.Left;
					joinCondition.LeftTable = leftTableReferences[0];
					joinCondition.RightExpression = binaryExpression.Right;
					joinCondition.RightTable = rightTableReferences[0];
					return joinCondition;
				}
			}

			return null;
		}

		#endregion

		private class InnerJoinTableExtractor : StandardVisitor
		{
			private List<ExpressionNode> _filters = new List<ExpressionNode>();
			private List<TableAlgebraNode> _tables = new List<TableAlgebraNode>();
			private bool _consistsOnlyOfInnerJoinsFiltersAndTables = true;

			public bool ConsistsOnlyOfInnerJoinsFiltersAndTables
			{
				get { return _consistsOnlyOfInnerJoinsFiltersAndTables; }
			}

			public ExpressionNode[] GetFilters()
			{
				return _filters.ToArray();
			}

			public TableAlgebraNode[] GetTableNodes()
			{
				return _tables.ToArray();
			}
            
			public override AstNode Visit(AstNode node)
			{
				switch (node.NodeType)
				{
					case AstNodeType.JoinAlgebraNode:
					case AstNodeType.FilterAlgebraNode:
					case AstNodeType.TableAlgebraNode:
						return base.Visit(node);
                        
					default:
						_consistsOnlyOfInnerJoinsFiltersAndTables = false;
						return node;
				}
			}

			public override AlgebraNode VisitJoinAlgebraNode(JoinAlgebraNode node)
			{
				if (node.Op != JoinAlgebraNode.JoinOperator.InnerJoin)
				{
					_consistsOnlyOfInnerJoinsFiltersAndTables = false;
					return node;
				}

				if (node.Predicate != null)
					_filters.Add(node.Predicate);

				Visit(node.Left);
				Visit(node.Right);
                
				return node;
			}

			public override AlgebraNode VisitFilterAlgebraNode(FilterAlgebraNode node)
			{
				Visit(node.Input);
				_filters.Add(node.Predicate);
				return node;
			}

			public override AlgebraNode VisitTableAlgebraNode(TableAlgebraNode node)
			{
				_tables.Add(node);
				return node;
			}
		}

		public override AlgebraNode VisitJoinAlgebraNode(JoinAlgebraNode node)
		{
			// Check if node only consists of INNER join nodes and simple table reference nodes.
			// This algorithm assumes that the table references have been lineraized so that
			//
			// - JoinedTableReference appear on the LHS only (the last one must be NamedTableReference ovbiviously)
			// - NamedTableReference appear on the RHS only
			//
			// While scanning the node's children we create a list of all JoinedTableReferences and
			// NamedTableReferences.

			InnerJoinTableExtractor innerJoinTableExtractor = new InnerJoinTableExtractor();
			innerJoinTableExtractor.Visit(node);

			if (!innerJoinTableExtractor.ConsistsOnlyOfInnerJoinsFiltersAndTables)
			{
				node.Left = VisitAlgebraNode(node.Left);
				node.Right = VisitAlgebraNode(node.Right);
				return node;                
			}
			else
			{
				TableAlgebraNode[] algebraNodes = innerJoinTableExtractor.GetTableNodes();
				Dictionary<TableRefBinding,TableAlgebraNode> tableRefToNodeDictionary = new Dictionary<TableRefBinding, TableAlgebraNode>();
				List<TableRefBinding> tableList = new List<TableRefBinding>();
				foreach (TableAlgebraNode algebraNode in algebraNodes)
				{
					tableRefToNodeDictionary.Add(algebraNode.TableRefBinding, algebraNode);
					tableList.Add(algebraNode.TableRefBinding);
				}

				// Create a mapping RowBufferEntry -> ColumnRefBinding

				Dictionary<RowBufferEntry, ColumnRefBinding> rowBufferColumnDictionary = new Dictionary<RowBufferEntry, ColumnRefBinding>();
				foreach (TableRefBinding tableRefBinding in tableList)
				{
					foreach (ColumnRefBinding columnRefBinding in tableRefBinding.ColumnRefs)
						rowBufferColumnDictionary.Add(columnRefBinding.ValueDefinition.Target, columnRefBinding);
				}

				// Create list of all possible join conditions and remaining AND-parts.

				List<JoinCondition> joinConditionList = new List<JoinCondition>();
				List<ExpressionNode> andPartList = new List<ExpressionNode>();

				ExpressionNode filter = AstUtil.CombineConditions(LogicalOperator.And, innerJoinTableExtractor.GetFilters());

				foreach (ExpressionNode andPart in AstUtil.SplitCondition(LogicalOperator.And, filter))
				{
					JoinCondition joinCondition = ConvertToJoinCondition(rowBufferColumnDictionary, andPart);

					if (joinCondition != null)
						joinConditionList.Add(joinCondition);
					else
						andPartList.Add(andPart);
				}

				// After creating the list of all join conditions and AND-parts we have all we need to create
				// an optimimal join order between all tables of this part of the table tree.
				JoinOrder bestJoinOrder = GetBestJoinOrder(tableList.ToArray(), joinConditionList.ToArray(), andPartList.ToArray());

				// Get all tables that are introduced by this join order
                
				Dictionary<RowBufferEntry, ColumnValueDefinition> introducedColumns = GetIntroducedColumns(bestJoinOrder);

				// Combine AND-part list with all unused join conditions.

				andPartList.AddRange(bestJoinOrder.UnusedConditions);

				// Now we will re-create this part of the tree using the this join order.

				AlgebraNode lastAlgebraNode = null;
				for (int joinIndex = 0; joinIndex < bestJoinOrder.Joins.Length; joinIndex++)
				{
					Join join = bestJoinOrder.Joins[joinIndex];

					AlgebraNode tableInput;
					TableAlgebraNode tableNode = tableRefToNodeDictionary[join.TableRefBinding];

					ExpressionNode tableFilter = ExtractConditionsApplicableToTable(introducedColumns, andPartList, join.TableRefBinding);
					if (tableFilter == null)
					{
						tableInput = tableNode;
					}
					else
					{
						FilterAlgebraNode filterAlgebraNode = new FilterAlgebraNode();
						filterAlgebraNode.Input = tableNode;
						filterAlgebraNode.Predicate = tableFilter;
						tableInput = filterAlgebraNode;
					}

					if (lastAlgebraNode == null)
					{
						// This was the first one.
						lastAlgebraNode = tableInput;
					}
					else
					{
						// Not the first one, we can create a join with the current table reference
						// and last table reference.

						// Get all AND-parts that can be applied to the tables already joined.
						// This expression is merged to one condition.
						ExpressionNode[] applicableAndParts = GetAndPartsApplicableToJoin(introducedColumns, bestJoinOrder, joinIndex, andPartList, true);
						ExpressionNode condition = AstUtil.CombineConditions(LogicalOperator.And, applicableAndParts);

						ExpressionNode joinCondition;
						if (join.JoinCondition == null)
							joinCondition = null;
						else
							joinCondition = join.JoinCondition.ToExpression();

						ExpressionNode completeCondition = AstUtil.CombineConditions(LogicalOperator.And, condition, joinCondition);

						JoinAlgebraNode joinAlgebraNode = new JoinAlgebraNode();
						joinAlgebraNode.Op = JoinAlgebraNode.JoinOperator.InnerJoin;
						joinAlgebraNode.Left = lastAlgebraNode;
						joinAlgebraNode.Right = tableInput;
						joinAlgebraNode.Predicate = completeCondition;

						// Next time this newly created join is the last table reference.
						lastAlgebraNode = joinAlgebraNode;
					}
				}

				return lastAlgebraNode;
			}
		}
	}
}