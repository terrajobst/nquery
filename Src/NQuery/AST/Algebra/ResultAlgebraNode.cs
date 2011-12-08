using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class ResultAlgebraNode : UnaryAlgebraNode
	{
		private string[] _columnNames;

		public ResultAlgebraNode()
		{
		}

		public string[] ColumnNames
		{
			get { return _columnNames; }
			set { _columnNames = value; }
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.ResultAlgebraNode; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			ResultAlgebraNode result = new ResultAlgebraNode();
			result.StatisticsIterator = StatisticsIterator;
			result.OutputList = ArrayHelpers.Clone(OutputList);
			result.Input = (AlgebraNode)Input.Clone(alreadyClonedElements);
			result.ColumnNames = ArrayHelpers.Clone(_columnNames);
			return result;
		}
	}
}