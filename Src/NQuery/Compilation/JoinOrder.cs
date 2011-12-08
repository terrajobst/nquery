using System;

namespace NQuery.Compilation
{
	internal class JoinOrder
	{
		public Join[] Joins;
		public ExpressionNode[] UnusedConditions;
	}
}