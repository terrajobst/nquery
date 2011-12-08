using System;
using System.Globalization;

namespace NQuery.Runtime.ExecutionPlan
{
	internal sealed class FilterIterator : UnaryIterator
	{
		public RuntimeExpression Predicate;

		private bool CheckPredicate()
		{
			object predicateResult = Predicate.GetValue();
			return (predicateResult != null && Convert.ToBoolean(predicateResult, CultureInfo.InvariantCulture));
		}

		public override void Open()
		{
			Input.Open();
		}

		public override bool Read()
		{
			bool predicateIsTrue = false;
			while (!predicateIsTrue)
			{
				if (!Input.Read())
					break;

				predicateIsTrue = CheckPredicate();
			}

			WriteInputToRowBuffer();
			return predicateIsTrue;
		}
	}
}