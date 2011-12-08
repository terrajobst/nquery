using System;

using NQuery.Runtime.ExecutionPlan;

namespace NQuery.Compilation
{
	internal abstract class AlgebraNode : AstNode
	{
		private StatisticsIterator _statisticsIterator;
		private RowBufferEntry[] _outputList;

		protected AlgebraNode()
		{
		}

		public RowBufferEntry[] OutputList
		{
			get { return _outputList; }
			set { _outputList = value; }
		}

		public StatisticsIterator StatisticsIterator
		{
			get { return _statisticsIterator; }
			set { _statisticsIterator = value; }
		}
	}
}
