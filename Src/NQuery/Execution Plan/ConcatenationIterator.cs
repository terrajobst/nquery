using System;

namespace NQuery.Runtime.ExecutionPlan
{
	internal sealed class ConcatenationIterator : Iterator
	{
		public Iterator[] Inputs;
		public IteratorOutput[][] InputOutput;
		private int _currentInputIndex;
		private bool _currentInputIsOpen;

		public ConcatenationIterator()
		{
		}

		public override void Initialize()
		{
			base.Initialize();

			foreach (Iterator input in Inputs)
				input.Initialize();
		}

		public override void Open()
		{
			_currentInputIndex = 0;
			_currentInputIsOpen = false;
		}

		public override bool Read()
		{
			while (_currentInputIndex < Inputs.Length)
			{
				Iterator currentInput = Inputs[_currentInputIndex];
				IteratorOutput[] currentInputOutput = InputOutput[_currentInputIndex];

				if (!_currentInputIsOpen)
				{
					currentInput.Open();
					_currentInputIsOpen = true;
				}

				if (currentInput.Read())
				{
					foreach (IteratorOutput output in currentInputOutput)
						RowBuffer[output.TargetIndex] = currentInput.RowBuffer[output.SourceIndex];
					return true;
				}

				_currentInputIndex++;
				_currentInputIsOpen = false;
			}

			return false;
		}
	}
}