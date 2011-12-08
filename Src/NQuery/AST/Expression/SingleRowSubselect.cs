using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class SingleRowSubselect : SubselectExpression
	{
		public override object GetValue()
		{
			throw ExceptionBuilder.InternalErrorGetValueNotSupported(GetType());
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.SingleRowSubselect; }
		}

		public override Type ExpressionType
		{
			get
			{
				if (Query == null)
					return null;
				
				SelectColumn[] selectColumns = Query.GetColumns();
				if (selectColumns == null || selectColumns.Length == 0 || selectColumns[0].Expression == null)
					return null;
				
				return selectColumns[0].Expression.ExpressionType;
			}
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			SingleRowSubselect result = new SingleRowSubselect();
			result.Query = (QueryNode)Query.Clone(alreadyClonedElements);
			return result;
		}
	}
}