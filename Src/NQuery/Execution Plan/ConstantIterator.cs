using System;

namespace NQuery.Runtime.ExecutionPlan
{
	internal sealed class ConstantIterator : Iterator
	{
		public RuntimeComputedValueOutput[] DefinedValues;
		private bool _isEof;

		public override void Open()
		{
			_isEof = false;

			foreach (RuntimeComputedValueOutput definedValue in DefinedValues)
				RowBuffer[definedValue.TargetIndex] = definedValue.Expression.GetValue();
		}

		public override bool Read()
		{
			if (_isEof)
				return false;

			_isEof = true;
			return true;
		}
	}
}