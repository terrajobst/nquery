using System;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal class Join
	{
		private TableRefBinding _tableRefBinding;
		private JoinCondition _joinCondition;

		public TableRefBinding TableRefBinding
		{
			get { return _tableRefBinding; }
			set { _tableRefBinding = value; }
		}

		public JoinCondition JoinCondition
		{
			get { return _joinCondition; }
			set { _joinCondition = value; }
		}
	}
}