using System;
using System.Text;

using NQuery.Runtime;

namespace NQuery.Samples.CustomAggregate
{
	#region AggregateBinding

	public class CustomAggregate : AggregateBinding
	{
		public CustomAggregate(string name) : base(name)
		{
		}

		public override IAggregator CreateAggregator(Type inputType)
		{
			// If the aggregate does not support a given input type the
			// aggregate should return null.
			if (inputType != typeof(string))
				return null;

			// Each aggregate must create an aggregator to perform the
			// aggregation. This pattern is similar to IEnumerable and
			// IEnumerator where the actual work is performed by IEnumerator.
			// This is important to allow multiple concurrent aggregations.
			return new CustomAggregator();
		}
	}

	#endregion

	#region Aggregator

	public class CustomAggregator : IAggregator
	{
		private StringBuilder _sb = new StringBuilder();

		public void Init()
		{
			// This method is called before aggregation of a group starts.
			// Here you should do some cleanup/init stuff.

			_sb = new StringBuilder();
		}

		public void Accumulate(object value)
		{
			// This method is called foreach value in the group.
			//
			// Is is guaranteed that all null representations (e.g. null, DBNull.Value, INullable, Nullable<T>, Sql*)
			// are converted simply to null.

			if (value != null)
			{
				if (_sb.Length > 0)
					_sb.Append(", ");

				_sb.Append(value);
			}
		}

		public object Terminate()
		{
			// This method is called at the end of each group to 
			// create the result of the aggregation.

			return _sb.ToString();
		}

		public Type ReturnType
		{
			get { return typeof(string); }
		}
	}

#endregion
}
