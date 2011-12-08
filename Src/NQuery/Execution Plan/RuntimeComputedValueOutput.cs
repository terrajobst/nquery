using System;

namespace NQuery.Runtime.ExecutionPlan
{
	internal sealed class RuntimeComputedValueOutput : RuntimeValueOutput
	{
		public RuntimeExpression Expression;
	}
}