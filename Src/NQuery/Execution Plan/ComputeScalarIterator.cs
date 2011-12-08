using System;

namespace NQuery.Runtime.ExecutionPlan
{
	internal sealed class ComputeScalarIterator : UnaryIterator
	{
		public RuntimeComputedValueOutput[] DefinedValues;

		public override void Open()
		{
			Input.Open();
		}

		public override bool Read()
		{
			if (!Input.Read())
				return false;

			WriteInputToRowBuffer();

			foreach (RuntimeComputedValueOutput definedValue in DefinedValues)
				RowBuffer[definedValue.TargetIndex] = definedValue.Expression.GetValue();

			return true;
		}
	}
}