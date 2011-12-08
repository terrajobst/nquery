using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class StackedTableSpoolRefAlgebraNode : AlgebraNode
	{
		private StackedTableSpoolAlgebraNode _primarySpool;
		private RowBufferEntry[] _definedValues;

		public override AstNodeType NodeType
		{
			get { return AstNodeType.StackedTableSpoolRefAlgebraNode; }
		}

		public StackedTableSpoolAlgebraNode PrimarySpool
		{
			get { return _primarySpool; }
			set { _primarySpool = value; }
		}

		public RowBufferEntry[] DefinedValues
		{
			get { return _definedValues; }
			set { _definedValues = value; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			StackedTableSpoolRefAlgebraNode result = new StackedTableSpoolRefAlgebraNode();
			result.StatisticsIterator = StatisticsIterator;
			result.OutputList = ArrayHelpers.Clone(OutputList);
			result.DefinedValues = ArrayHelpers.Clone(_definedValues);
			result.PrimarySpool = (StackedTableSpoolAlgebraNode) alreadyClonedElements[_primarySpool];
			return result;
		}
	}
}