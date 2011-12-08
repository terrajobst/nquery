using System;
using System.Collections.Generic;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class JoinOrderGenerator
	{
		private JoinCondition[] _joinConditions;
		private TableRefBinding[] _tables;
				
		private List<JoinOrder> _joinOrderList;
		private List<JoinCondition> _usedJoinConditionList;
		private List<TableRefBinding> _usedTableList;

		public JoinOrderGenerator(JoinCondition[] joinConditions, TableRefBinding[] tables)
		{
			_joinConditions = joinConditions;		
			_tables = tables;		
		}

		public JoinOrder[] GenerateAll()
		{
            _joinOrderList = new List<JoinOrder>();
            _usedJoinConditionList = new List<JoinCondition>();
            _usedTableList = new List<TableRefBinding>();

			for (int i = 0; i < _tables.Length; i++)
				Join(_tables[i], null);

			return _joinOrderList.ToArray();
		}

		private void Join(TableRefBinding table, JoinCondition joinCondition)
		{
			_usedTableList.Add(table);
			_usedJoinConditionList.Add(joinCondition);

			if (_usedTableList.Count == _tables.Length)
			{
				// We have joined all tables, create the join order.
				//
				// First we create a list of the join conditions we used.

				List<Join> joinList = new List<Join>();

				for (int i = 0; i < _usedTableList.Count; i++)
				{
					TableRefBinding usedTable = _usedTableList[i];
					JoinCondition usedJoinCondition = _usedJoinConditionList[i];
						
					// The first entry will be null, since we do not join from anywhere 

					if (usedJoinCondition != null)
					{
						// Since we swapping the join sides according to the direction
						// we are joining from we need to create a clone. The clone
						// is a flat copy meaning that the tables and expressions
						// themselves are not cloned.

						usedJoinCondition = usedJoinCondition.Clone();

						// If the right table is not the table we are joining to
						// swap the join sides.

						if (usedJoinCondition.RightTable != usedTable)
							usedJoinCondition.SwapSides();
					}

					Join join = new Join();
					join.JoinCondition = usedJoinCondition;
					join.TableRefBinding = usedTable;

					joinList.Add(join);
				}

				// Secondly and very important: We also have to create a list of all
				// join conditions NOT used. Later theses conditions will be combined
				// with the and-parts since they must also be checked.

				List<ExpressionNode> unusedConditionList = new List<ExpressionNode>();

				foreach (JoinCondition jc in _joinConditions)
				{
					if (!_usedJoinConditionList.Contains(jc))
						unusedConditionList.Add(jc.ToExpression());
				}

				JoinOrder joinOrder = new JoinOrder();
				joinOrder.Joins = joinList.ToArray();
				joinOrder.UnusedConditions = unusedConditionList.ToArray();

				_joinOrderList.Add(joinOrder);
			}
			else
			{
				// We are not yet finished with all tables. Find next table
				// to join with.

				bool hasJoin = false;

				foreach (JoinCondition nextJoin in _joinConditions)
				{
					if (!_usedJoinConditionList.Contains(nextJoin))
					{
						TableRefBinding nextTable;

						if (_usedTableList.Contains(nextJoin.LeftTable))
						{
							nextTable = nextJoin.RightTable;
						}
						else if (_usedTableList.Contains(nextJoin.RightTable))
						{
							nextTable = nextJoin.LeftTable;
						}
						else
						{
							continue;
						}

						if (_usedTableList.Contains(nextTable))
							continue;

						Join(nextTable, nextJoin);
						hasJoin = true;
					}
				}

				if (!hasJoin)
				{
					foreach (TableRefBinding t in _tables)
					{
						if (!_usedTableList.Contains(t))
							Join(t, null);
					}
				}
			}

			_usedJoinConditionList.RemoveAt(_usedJoinConditionList.Count - 1);
			_usedTableList.RemoveAt(_usedTableList.Count - 1);
		}
	}
}