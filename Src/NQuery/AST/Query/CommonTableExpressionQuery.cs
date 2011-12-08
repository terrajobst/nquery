using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class CommonTableExpressionQuery : QueryNode
	{
		private CommonTableExpression[] _commonTableExpressions;
		private QueryNode _input;

		public CommonTableExpression[] CommonTableExpressions
		{
			get { return _commonTableExpressions; }
			set { _commonTableExpressions = value; }
		}

		public QueryNode Input
		{
			get { return _input; }
			set { _input = value; }
		}

		public override SelectColumn[] GetColumns()
		{
			return _input.GetColumns();
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.CommonTableExpressionQuery; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			CommonTableExpressionQuery result = new CommonTableExpressionQuery();
			result.CommonTableExpressions = ArrayHelpers.CreateDeepCopyOfAstElementArray(_commonTableExpressions, alreadyClonedElements);
			result.Input = (QueryNode)_input.Clone(alreadyClonedElements);
			return result;
		}
	}
}
