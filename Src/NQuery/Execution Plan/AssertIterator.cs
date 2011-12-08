using System;
using System.Globalization;

namespace NQuery.Runtime.ExecutionPlan
{
	internal sealed class AssertIterator : UnaryIterator
	{
		public RuntimeExpression Predicate;
		public string Message;

		private bool CheckPredicate()
		{
			object predicateResult = Predicate.GetValue();
			return (predicateResult == null || Convert.ToBoolean(predicateResult, CultureInfo.InvariantCulture));
		}

		public override void Open()
		{
			Input.Open();
		}

		public override bool Read()
		{
			if (!Input.Read())
				return false;

			if (!CheckPredicate())
				throw ExceptionBuilder.AssertionFailed(Message);

			WriteInputToRowBuffer();
			return true;
		}
	}
}