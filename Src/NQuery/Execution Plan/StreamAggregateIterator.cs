using System;

namespace NQuery.Runtime.ExecutionPlan
{
	internal sealed class StreamAggregateIterator : UnaryIterator
	{
		public IteratorInput[] GroupByEntries;
		public RuntimeAggregateValueOutput[] DefinedValues;

		private object[] _lastRowGroupByValues;
		private object[] _thisRowGroupByValues;
		private bool _eof;
		private bool _isFirstRecord;

		public StreamAggregateIterator()
		{
		}

		private void InitializeAggregates()
		{
			foreach (RuntimeAggregateValueOutput definedValueOutput in DefinedValues)
			{
				try
				{
					definedValueOutput.Aggregator.Init();
				}
				catch (NQueryException)
				{
					throw;
				}
				catch (Exception ex)
				{
					throw ExceptionBuilder.IAggregatorInitFailed(ex);
				}
			}
		}

		private void AccumulateAggregates()
		{
			foreach (RuntimeAggregateValueOutput definedValueOutput in DefinedValues)
			{
				object argument = definedValueOutput.Argument.GetValue();

				try
				{
					definedValueOutput.Aggregator.Accumulate(argument);
				}
				catch (NQueryException)
				{
					throw;
				}
				catch (Exception ex)
				{
					throw ExceptionBuilder.IAggregatorAccumulateFailed(ex);
				}
			}
		}

		private void TerminatedAggregates()
		{
			foreach (RuntimeAggregateValueOutput definedValueOutput in DefinedValues)
			{
				try
				{
					RowBuffer[definedValueOutput.TargetIndex] = definedValueOutput.Aggregator.Terminate();
				}
				catch (NQueryException)
				{
					throw;
				}
				catch (Exception ex)
				{
					throw ExceptionBuilder.IAggregatorTerminateFailed(ex);
				}
			}
		}

		private void FillGroupByExpressions(object[] target)
		{
			for (int i = 0; i < GroupByEntries.Length; i++)
				target[i] = Input.RowBuffer[GroupByEntries[i].SourceIndex];
		}

		private bool CheckIfCurrentRowIsInSameGroup()
		{
			// Assume we are in the same group until proven otherwise.
			bool inSameGroup = true;

			// Get values of GROUP BY expressions

			FillGroupByExpressions(_thisRowGroupByValues);

			// We have a last one row (which means the current row in _rowBuffer is
			// not the first record).
			//
			// Compare this GROUP BY values with the last ones.

			for (int i = 0; i < _lastRowGroupByValues.Length; i++)
			{
				object valueOfLastRow = _lastRowGroupByValues[i];
				object valueOfThisRow = _thisRowGroupByValues[i];

				if (valueOfThisRow == valueOfLastRow)
				{
					// Same instance, values are equal.
					// Check next one.
					continue;
				}

				if (valueOfThisRow == null || valueOfLastRow == null || !valueOfThisRow.Equals(valueOfLastRow))
				{
					// They are not equal. This means the current row in the row buffer
					// belongs to another group.
					inSameGroup = false;
					break;
				}
			}

			// Remember current row.
			Array.Copy(_thisRowGroupByValues, _lastRowGroupByValues, 0);

			return inSameGroup;
		}

		public override void Open()
		{
			Input.Open();
			_eof = !Input.Read();
			_isFirstRecord = true;

			_lastRowGroupByValues = new object[GroupByEntries.Length];
			_thisRowGroupByValues = new object[GroupByEntries.Length];
		}

		public override bool Read()
		{
			if (_eof)
			{
				if (GroupByEntries.Length == 0 && _isFirstRecord)
				{
					_isFirstRecord = false;
					InitializeAggregates();
					TerminatedAggregates();
					return true;
				}

				return false;
			}

			_isFirstRecord = false;

			WriteInputToRowBuffer();

			InitializeAggregates();
			FillGroupByExpressions(_lastRowGroupByValues);
			do
			{
				AccumulateAggregates();

				// Next one please.
				if (!Input.Read())
				{
					_eof = true;
					break;
				}
			}
			while (CheckIfCurrentRowIsInSameGroup());

			TerminatedAggregates();

			return true;
		}
	}
}