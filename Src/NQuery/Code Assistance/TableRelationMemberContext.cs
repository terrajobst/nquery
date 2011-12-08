using System;
using System.Collections.Generic;

using NQuery.Compilation;
using NQuery.Runtime;

namespace NQuery.CodeAssistance
{
	internal sealed class TableRelationMemberContext : MemberCompletionContext
	{
		private Scope _scope;
		private QueryScope _queryScope;
		
		public TableRelationMemberContext(SourceLocation sourceLocation, Identifier remainingPart, Scope scope, QueryScope queryScope)
			: base(sourceLocation, remainingPart)
		{
			_scope = scope;
			_queryScope = queryScope;
		}

		private class TableRelationData
		{
			public TableRelation Relation;
			public TableRefBinding Parent;
			public TableRefBinding Child;
		}

		public override void Enumerate(IMemberCompletionAcceptor acceptor)
		{
			List<TableRelationData> tableRelationDataList = new List<TableRelationData>();

			TableRefBinding[] tableRefBindings = _queryScope.GetAllTableRefBindings();
			Array.Reverse(tableRefBindings);

			foreach (TableRefBinding joinSourceTableRef in tableRefBindings)
			{
				TableBinding joinSource = joinSourceTableRef.TableBinding;
                IList<TableRelation> tableRelations = _scope.DataContext.TableRelations.GetRelations(joinSource);
				
				foreach (TableRelation tableRelation in tableRelations)
				{
					TableBinding joinTarget;

					if (tableRelation.ParentTable == joinSource)
						joinTarget = tableRelation.ChildTable;
					else
						joinTarget = tableRelation.ParentTable;

					foreach (TableRefBinding joinTargetTableRef in tableRefBindings)
					{
						if (joinTargetTableRef.TableBinding == joinTarget)
						{
							TableRelationData tableRelationData = new TableRelationData();
							tableRelationData.Relation = tableRelation;
							tableRelationData.Child = joinTargetTableRef;
							tableRelationData.Parent = joinSourceTableRef;
							tableRelationDataList.Add(tableRelationData);
						}
					}
				}
			}

			foreach (TableRelationData tableRelationData in tableRelationDataList)
				acceptor.AcceptRelation(tableRelationData.Parent, tableRelationData.Child, tableRelationData.Relation);
		}
	}
}
