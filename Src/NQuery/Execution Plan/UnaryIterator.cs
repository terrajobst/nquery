using System;

namespace NQuery.Runtime.ExecutionPlan
{
	internal abstract class UnaryIterator : Iterator
	{
		public Iterator Input;
		public IteratorOutput[] InputOutput;

		public override void Initialize()
		{
			base.Initialize();
			Input.Initialize();
		}

		protected void WriteInputToRowBuffer()
		{
			foreach (IteratorOutput output in InputOutput)
				RowBuffer[output.TargetIndex] = Input.RowBuffer[output.SourceIndex];
		}
	}
}