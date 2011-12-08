using System;
using System.Collections.Generic;

using NQuery.Compilation;
using NQuery.Runtime;

namespace NQuery.CodeAssistance
{
	internal sealed class JoinTableMemberContext : MemberCompletionContext
	{
		private Scope _scope;
		private QueryScope _queryScope;

		public JoinTableMemberContext(SourceLocation sourceLocation, Identifier remainingPart, Scope scope, QueryScope queryScope)
			: base(sourceLocation, remainingPart)
		{
			_scope = scope;
			_queryScope = queryScope;
		}

		public override void Enumerate(IMemberCompletionAcceptor acceptor)
		{
			List<TableBinding> joinTargetList = new List<TableBinding>();

			TableRefBinding[] tableRefBindings = _queryScope.GetAllTableRefBindings();
			foreach (TableRefBinding tableRefBinding in tableRefBindings)
			{
				TableBinding joinSource = tableRefBinding.TableBinding;
				IList<TableRelation> tableRelations = _scope.DataContext.TableRelations.GetRelations(joinSource);
				
				foreach (TableRelation tableRelation in tableRelations)
				{
					TableBinding joinTarget;

					if (tableRelation.ParentTable == joinSource)
						joinTarget = tableRelation.ChildTable;
					else
						joinTarget = tableRelation.ParentTable;

					if (!joinTargetList.Contains(joinTarget))
						joinTargetList.Add(joinTarget);
				}
			}

			if (joinTargetList.Count > 0)
			{
				foreach (TableBinding joinTarget in joinTargetList)
					acceptor.AcceptTable(joinTarget);
			}
			else
			{
				foreach (TableBinding joinTarget in _scope.DataContext.Tables)
					acceptor.AcceptTable(joinTarget);
			}
		}
	}
}
