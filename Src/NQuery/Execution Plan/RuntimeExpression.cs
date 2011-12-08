using System;
using System.Diagnostics;

namespace NQuery.Runtime.ExecutionPlan
{
	[DebuggerDisplay("{Source}")]
	internal abstract class RuntimeExpression
	{
		public abstract object GetValue();
		public abstract Type ExpressionType { get; }
		protected abstract string Source { get; }
	}
}