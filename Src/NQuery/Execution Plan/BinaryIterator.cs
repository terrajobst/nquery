using System;

namespace NQuery.Runtime.ExecutionPlan
{
	internal abstract class BinaryIterator : Iterator
	{
		public Iterator Left;
		public Iterator Right;
		public IteratorOutput[] LeftOutput;
		public IteratorOutput[] RightOutput;

		public override void Initialize()
		{
			base.Initialize();
			Left.Initialize();
			Right.Initialize();
		}

		protected void WriteLeftAndRightToRowBuffer()
		{
			foreach (IteratorOutput output in LeftOutput)
				RowBuffer[output.TargetIndex] = Left.RowBuffer[output.SourceIndex];

			foreach (IteratorOutput output in RightOutput)
				RowBuffer[output.TargetIndex] = Right.RowBuffer[output.SourceIndex];
		}

		protected void WriteLeftWithNullRightToRowBuffer()
		{
			foreach (IteratorOutput output in LeftOutput)
				RowBuffer[output.TargetIndex] = Left.RowBuffer[output.SourceIndex];

			foreach (IteratorOutput output in RightOutput)
				RowBuffer[output.TargetIndex] = null;
		}

		protected void WriteRightWithNullLeftToRowBuffer()
		{
			foreach (IteratorOutput output in LeftOutput)
				RowBuffer[output.TargetIndex] = null;

			foreach (IteratorOutput output in RightOutput)
				RowBuffer[output.TargetIndex] = Right.RowBuffer[output.SourceIndex];
		}
	}
}