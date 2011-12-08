using System;
using System.Globalization;

namespace NQuery.Runtime.ExecutionPlan
{
	internal abstract class NestedLoopsIterator : BinaryIterator
	{
		public RuntimeExpression PassthruPredicate;
		public RuntimeExpression Predicate;

		protected bool _bof;
		protected bool _outerRowHadMatchingInnerRow;
		protected bool _advanceOuter;

		protected bool CheckPredicate()
		{
			if (Predicate == null)
				return true;

			object result = Predicate.GetValue();
			return (result != null && Convert.ToBoolean(result, CultureInfo.InvariantCulture));
		}

		protected bool CheckPassthruPredicate()
		{
			if (PassthruPredicate == null)
				return false;

			object result = PassthruPredicate.GetValue();
			return (result != null && Convert.ToBoolean(result, CultureInfo.InvariantCulture));
		}

		public override void Open()
		{
			Left.Open();
			_advanceOuter = false;
			_outerRowHadMatchingInnerRow = false;
			_bof = true;
		}
	}
}