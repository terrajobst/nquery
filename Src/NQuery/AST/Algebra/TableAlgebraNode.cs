using System;
using System.Collections.Generic;

using NQuery.Runtime;

namespace NQuery.Compilation
{
	internal sealed class TableAlgebraNode : AlgebraNode
	{
		private TableRefBinding _tableRefBinding;
		private ColumnValueDefinition[] _definedValues;

		public TableAlgebraNode()
		{
		}

		public TableRefBinding TableRefBinding
		{
			get { return _tableRefBinding; }
			set { _tableRefBinding = value; }
		}

		public ColumnValueDefinition[] DefinedValues
		{
			get { return _definedValues; }
			set { _definedValues = value; }
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.TableAlgebraNode; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			TableAlgebraNode result = new TableAlgebraNode();
			result.StatisticsIterator = StatisticsIterator;
			result.OutputList = ArrayHelpers.Clone(OutputList);
			result.TableRefBinding = _tableRefBinding;
			result.DefinedValues = ArrayHelpers.CreateDeepCopyOfAstElementArray(_definedValues, alreadyClonedElements);
			return result;
		}
	}
}