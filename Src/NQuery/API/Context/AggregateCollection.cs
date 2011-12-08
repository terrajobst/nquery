using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using NQuery.Runtime;

namespace NQuery
{
    public sealed class AggregateCollection : BindingCollection<AggregateBinding>
	{
    	internal AggregateCollection()
    	{
    	}

		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public IEnumerable<AggregateBinding> GetDefaults()
		{
			yield return new CountAggregateBinding("COUNT");
			yield return new AverageAggregateBinding("AVG");
			yield return new FirstAggregateBinding("FIRST");
			yield return new LastAggregateBinding("LAST");
			yield return new MaxAggregateBinding("MAX");
			yield return new MinAggregateBinding("MIN");
			yield return new SumAggregateBinding("SUM");
			yield return new StdDevAggregateBinding("STDEV");
			yield return new VarAggregateBinding("VAR");
			yield return new ConcatAggregateBinding("CONCAT");
		}

		public void AddDefaults()
		{
			foreach (AggregateBinding aggregateBinding in GetDefaults())
				Add(aggregateBinding);
		}

		public void RemoveDefaults()
		{
			foreach (AggregateBinding aggregateBinding in GetDefaults())
				Remove(aggregateBinding.Name);
		}
	}
}