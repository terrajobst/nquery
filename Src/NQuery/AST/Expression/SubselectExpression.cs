using System;

namespace NQuery.Compilation
{
	internal abstract class SubselectExpression : ExpressionNode
	{
		private QueryNode _query;

		public QueryNode Query
		{
			get { return _query; }
			set { _query = value; }
		}
	}
}