using System;
using System.Collections.Generic;

namespace NQuery.Compilation
{
	internal sealed class ExistsSubselect : SubselectExpression
	{
		private bool _negated;

		public bool Negated
		{
			get { return _negated; }
			set { _negated = value; }
		}

		public override object GetValue()
		{
			throw ExceptionBuilder.InternalErrorGetValueNotSupported(GetType());
		}

		public override AstNodeType NodeType
		{
			get { return AstNodeType.ExistsSubselect; }
		}

		public override AstElement Clone(Dictionary<AstElement, AstElement> alreadyClonedElements)
		{
			ExistsSubselect result = new ExistsSubselect();
			result.Negated = _negated;
			result.Query = (QueryNode)Query.Clone(alreadyClonedElements);
			return result;
		}
		
		public override Type ExpressionType
		{
			get { return typeof(bool); }
		}
	}
}