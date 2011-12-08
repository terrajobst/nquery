using System;

namespace NQuery.Runtime.ExecutionPlan
{
	internal sealed class RuntimeAggregateValueOutput : RuntimeValueOutput
	{
		public IAggregator Aggregator;
		public RuntimeExpression Argument;
	}
}